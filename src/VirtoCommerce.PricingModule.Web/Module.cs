using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.ExportModule.Core.Services;
using VirtoCommerce.ExportModule.Data.Extensions;
using VirtoCommerce.ExportModule.Data.Services;
using VirtoCommerce.Platform.Core.Bus;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Core.Settings.Events;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.Platform.Security.Authorization;
using VirtoCommerce.PricingModule.Core;
using VirtoCommerce.PricingModule.Core.Events;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Conditions;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Common;
using VirtoCommerce.PricingModule.Data.ExportImport;
using VirtoCommerce.PricingModule.Data.Handlers;
using VirtoCommerce.PricingModule.Data.MySql;
using VirtoCommerce.PricingModule.Data.PostgreSql;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Search;
using VirtoCommerce.PricingModule.Data.Services;
using VirtoCommerce.PricingModule.Data.Services.Search;
using VirtoCommerce.PricingModule.Data.SqlServer;
using VirtoCommerce.PricingModule.Data.Validators;

#pragma warning disable CS0618 // Allow to use obsoleted

namespace VirtoCommerce.PricingModule.Web
{
    public class Module : IModule, IExportSupport, IImportSupport, IHasConfiguration
    {
        private IApplicationBuilder _applicationBuilder;
        public IConfiguration Configuration { get; set; }


        #region IModule Members

        public ManifestModuleInfo ModuleInfo { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<PricingDbContext>((provider, options) =>
            {
                var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
                var connectionString = Configuration.GetConnectionString(ModuleInfo.Id) ?? Configuration.GetConnectionString("VirtoCommerce");

                switch (databaseProvider)
                {
                    case "MySql":
                        options.UseMySqlDatabase(connectionString);
                        break;
                    case "PostgreSql":
                        options.UsePostgreSqlDatabase(connectionString);
                        break;
                    default:
                        options.UseSqlServerDatabase(connectionString);
                        break;
                }
            });

            serviceCollection.AddTransient<IPricingRepository, PricingRepositoryImpl>();
            serviceCollection.AddTransient<Func<IPricingRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<IPricingRepository>());
            serviceCollection.AddTransient<IPricingEvaluatorService, PricingEvaluatorService>();
            serviceCollection.AddTransient<ISearchService<PricelistAssignmentsSearchCriteria, PricelistAssignmentSearchResult, PricelistAssignment>, PricelistAssignmentSearchService>();
            serviceCollection.AddTransient<ISearchService<PricelistSearchCriteria, PricelistSearchResult, Pricelist>, PricelistSearchService>();
            serviceCollection.AddTransient<ISearchService<PricesSearchCriteria, PriceSearchResult, Price>, PriceSearchService>();
            serviceCollection.AddTransient<ICrudService<PricelistAssignment>, PricelistAssignmentService>();
            serviceCollection.AddTransient<ICrudService<Pricelist>, PricelistService>();
            serviceCollection.AddTransient<ICrudService<Price>, PriceService>();
            serviceCollection.AddTransient<IPricingService, PricingServiceImpl>();
            serviceCollection.AddTransient<IPricingSearchService, PricingSearchServiceImpl>();
            serviceCollection.AddTransient<IPricingPriorityFilterPolicy, DefaultPricingPriorityFilterPolicy>();
            serviceCollection.AddTransient<PricingExportImport>();
            serviceCollection.AddTransient<IPricingDocumentChangesProvider, ProductPriceDocumentChangesProvider>();
            serviceCollection.AddTransient<ProductPriceDocumentBuilder>();
            serviceCollection.AddTransient<LogChangesChangedEventHandler>();
            serviceCollection.AddTransient<DeletePricesProductChangedEventHandler>();
            serviceCollection.AddTransient<IndexPricesProductChangedEventHandler>();
            serviceCollection.AddTransient<ObjectSettingEntryChangedEventHandler>();
            serviceCollection.AddTransient<AbstractValidator<IEnumerable<PricelistAssignment>>, PricelistAssignmentsValidator>();
            serviceCollection.AddTransient<AbstractValidator<Pricelist>, PriceListValidator>();

            serviceCollection.AddTransient<IMergedPriceSearchService, MergedPriceSearchService>();

            serviceCollection.AddTransient<ModuleConfigurator>();

            serviceCollection.AddTransient<IPricingExportPagedDataSourceFactory, PricingExportPagedDataSourceFactory>();

            var requirements = new IAuthorizationRequirement[]
            {
                new PermissionAuthorizationRequirement(ModuleConstants.Security.Permissions.Export), new PermissionAuthorizationRequirement(ModuleConstants.Security.Permissions.Read)
            };

            var exportPolicy = new AuthorizationPolicyBuilder()
                .AddRequirements(requirements)
                .Build();

            serviceCollection.Configure<Microsoft.AspNetCore.Authorization.AuthorizationOptions>(configure =>
            {
                configure.AddPolicy(typeof(ExportablePricelist).FullName + "FullDataExportDataPolicy", exportPolicy);
                configure.AddPolicy(typeof(ExportablePricelist).FullName + "ExportDataPolicy", exportPolicy);
                configure.AddPolicy(typeof(ExportablePrice).FullName + "ExportDataPolicy", exportPolicy);
                configure.AddPolicy(typeof(ExportablePricelistAssignment).FullName + "ExportDataPolicy", exportPolicy);
            });
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            _applicationBuilder = appBuilder;

            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);

            var modulePermissions = ModuleConstants.Security.Permissions.AllPermissions.Select(p => new Permission
            {
                Name = p,
                GroupName = "Pricing",
                ModuleId = ModuleInfo.Id
            }).ToArray();
            var permissionsRegistrar = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsRegistrar.RegisterPermissions(modulePermissions);

            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<PricingDbContext>();
                if (databaseProvider == "SqlServer")
                {
                    dbContext.Database.MigrateIfNotApplied(MigrationName.GetUpdateV2MigrationName(ModuleInfo.Id));
                }
                dbContext.Database.Migrate();
            }

            //Subscribe for Search configuration changes
            var inProcessBus = appBuilder.ApplicationServices.GetService<IHandlerRegistrar>();
            inProcessBus.RegisterHandler<ObjectSettingChangedEvent>(async (message, token) => await appBuilder.ApplicationServices.GetService<ObjectSettingEntryChangedEventHandler>().Handle(message));

            //Configure Search
            var moduleConfigurator = appBuilder.ApplicationServices.GetService<ModuleConfigurator>();
            moduleConfigurator.ConfigureSearchAsync();

            inProcessBus.RegisterHandler<PriceChangedEvent>(async (message, token) => await appBuilder.ApplicationServices.GetService<LogChangesChangedEventHandler>().Handle(message));
            inProcessBus.RegisterHandler<ProductChangedEvent>(async (message, token) => await appBuilder.ApplicationServices.GetService<DeletePricesProductChangedEventHandler>().Handle(message));
            inProcessBus.RegisterHandler<PriceChangedEvent>(async (message, token) => await appBuilder.ApplicationServices.GetService<IndexPricesProductChangedEventHandler>().Handle(message));

            foreach (var conditionTree in AbstractTypeFactory<PriceConditionTreePrototype>.TryCreateInstance().Traverse<IConditionTree>(x => x.AvailableChildren))
            {
                AbstractTypeFactory<IConditionTree>.RegisterType(conditionTree.GetType());
            }

            var registrar = appBuilder.ApplicationServices.GetService<IKnownExportTypesRegistrar>();

            registrar.RegisterType(
                 ExportedTypeDefinitionBuilder.Build<ExportablePrice, PriceExportDataQuery>()
                    .WithDataSourceFactory(appBuilder.ApplicationServices.GetService<IPricingExportPagedDataSourceFactory>())
                    .WithMetadata(typeof(ExportablePrice).GetPropertyNames())
                    .WithTabularMetadata(typeof(TabularPrice).GetPropertyNames()));

            registrar.RegisterType(
                 ExportedTypeDefinitionBuilder.Build<ExportablePricelist, PricelistExportDataQuery>()
                    .WithDataSourceFactory(appBuilder.ApplicationServices.GetService<IPricingExportPagedDataSourceFactory>())
                    .WithMetadata(typeof(ExportablePricelist).GetPropertyNames())
                    .WithTabularMetadata(typeof(TabularPricelist).GetPropertyNames()));

            registrar.RegisterType(
                 ExportedTypeDefinitionBuilder.Build<ExportablePricelistAssignment, PricelistAssignmentExportDataQuery>()
                    .WithDataSourceFactory(appBuilder.ApplicationServices.GetService<IPricingExportPagedDataSourceFactory>())
                    .WithMetadata(typeof(ExportablePricelistAssignment).GetPropertyNames())
                    .WithTabularMetadata(typeof(TabularPricelistAssignment).GetPropertyNames()));
        }

        public void Uninstall()
        {
            // no need to perform actions for now (Comment to remove Sonar warning)
        }

        #endregion

        #region ISupportExportImportModule Members

        public Task ExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback,
            ICancellationToken cancellationToken)
        {
            var exportJob = _applicationBuilder.ApplicationServices.GetRequiredService<PricingExportImport>();
            return exportJob.DoExportAsync(outStream, progressCallback, cancellationToken);
        }

        public Task ImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback,
            ICancellationToken cancellationToken)
        {
            var importJob = _applicationBuilder.ApplicationServices.GetRequiredService<PricingExportImport>();
            return importJob.DoImportAsync(inputStream, progressCallback, cancellationToken);
        }

        #endregion       
    }
}
