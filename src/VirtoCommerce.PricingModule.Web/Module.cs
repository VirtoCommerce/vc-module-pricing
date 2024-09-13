using System;
using System.Collections.Generic;
using System.IO;
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
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Extensions;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Core.Settings.Events;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.Platform.Data.MySql.Extensions;
using VirtoCommerce.Platform.Data.PostgreSql.Extensions;
using VirtoCommerce.Platform.Data.SqlServer.Extensions;
using VirtoCommerce.Platform.Security.Authorization;
using VirtoCommerce.PricingModule.Core;
using VirtoCommerce.PricingModule.Core.Events;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Conditions;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Common;
using VirtoCommerce.PricingModule.Data.ExportImport;
using VirtoCommerce.PricingModule.Data.Handlers;
using VirtoCommerce.PricingModule.Data.MySql;
using VirtoCommerce.PricingModule.Data.PostgreSql;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Search;
using VirtoCommerce.PricingModule.Data.Services;
using VirtoCommerce.PricingModule.Data.SqlServer;
using VirtoCommerce.PricingModule.Data.Validators;

namespace VirtoCommerce.PricingModule.Web
{
    public class Module : IModule, IExportSupport, IImportSupport, IHasConfiguration, IHasModuleCatalog
    {
        private IApplicationBuilder _applicationBuilder;

        public ManifestModuleInfo ModuleInfo { get; set; }
        public IConfiguration Configuration { get; set; }
        public IModuleCatalog ModuleCatalog { get; set; }

        private const string GenericExportModuleId = "VirtoCommerce.Export";

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<PricingDbContext>(options =>
            {
                var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
                var connectionString = Configuration.GetConnectionString(ModuleInfo.Id) ?? Configuration.GetConnectionString("VirtoCommerce");

                switch (databaseProvider)
                {
                    case "MySql":
                        options.UseMySqlDatabase(connectionString, typeof(MySqlDataAssemblyMarker), Configuration);
                        break;
                    case "PostgreSql":
                        options.UsePostgreSqlDatabase(connectionString, typeof(PostgreSqlDataAssemblyMarker), Configuration);
                        break;
                    default:
                        options.UseSqlServerDatabase(connectionString, typeof(SqlServerDataAssemblyMarker), Configuration);
                        break;
                }
            });

            serviceCollection.AddTransient<IPricingRepository, PricingRepositoryImpl>();
            serviceCollection.AddTransient<Func<IPricingRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<IPricingRepository>());
            serviceCollection.AddTransient<IPricingEvaluatorService, PricingEvaluatorService>();
            serviceCollection.AddTransient<IPricelistAssignmentSearchService, PricelistAssignmentSearchService>();
            serviceCollection.AddTransient<IPricelistSearchService, PricelistSearchService>();
            serviceCollection.AddTransient<IPriceSearchService, PriceSearchService>();
            serviceCollection.AddTransient<IPricelistAssignmentService, PricelistAssignmentService>();
            serviceCollection.AddTransient<IPricelistService, PricelistService>();
            serviceCollection.AddTransient<IPriceService, PriceService>();
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

            var permissionsRegistrar = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsRegistrar.RegisterPermissions(ModuleInfo.Id, "Pricing", ModuleConstants.Security.Permissions.AllPermissions);

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
            appBuilder.RegisterEventHandler<ObjectSettingChangedEvent, ObjectSettingEntryChangedEventHandler>();

            //Configure Search
            var moduleConfigurator = appBuilder.ApplicationServices.GetService<ModuleConfigurator>();
            moduleConfigurator.ConfigureSearchAsync().GetAwaiter().GetResult();

            appBuilder.RegisterEventHandler<PriceChangedEvent, LogChangesChangedEventHandler>();
            appBuilder.RegisterEventHandler<ProductChangedEvent, DeletePricesProductChangedEventHandler>();
            appBuilder.RegisterEventHandler<PriceChangedEvent, IndexPricesProductChangedEventHandler>();

            foreach (var conditionTree in AbstractTypeFactory<PriceConditionTreePrototype>.TryCreateInstance().Traverse<IConditionTree>(x => x.AvailableChildren))
            {
                AbstractTypeFactory<IConditionTree>.RegisterType(conditionTree.GetType());
            }

            if (ModuleCatalog.IsModuleInstalled(GenericExportModuleId))
            {
                var exportTypesRegistrar = appBuilder.ApplicationServices.GetService<IKnownExportTypesRegistrar>();

                exportTypesRegistrar.RegisterType(
                     ExportedTypeDefinitionBuilder.Build<ExportablePrice, PriceExportDataQuery>()
                        .WithDataSourceFactory(appBuilder.ApplicationServices.GetService<IPricingExportPagedDataSourceFactory>())
                        .WithMetadata(typeof(ExportablePrice).GetPropertyNames())
                        .WithTabularMetadata(typeof(TabularPrice).GetPropertyNames()));

                exportTypesRegistrar.RegisterType(
                     ExportedTypeDefinitionBuilder.Build<ExportablePricelist, PricelistExportDataQuery>()
                        .WithDataSourceFactory(appBuilder.ApplicationServices.GetService<IPricingExportPagedDataSourceFactory>())
                        .WithMetadata(typeof(ExportablePricelist).GetPropertyNames())
                        .WithTabularMetadata(typeof(TabularPricelist).GetPropertyNames()));

                exportTypesRegistrar.RegisterType(
                     ExportedTypeDefinitionBuilder.Build<ExportablePricelistAssignment, PricelistAssignmentExportDataQuery>()
                        .WithDataSourceFactory(appBuilder.ApplicationServices.GetService<IPricingExportPagedDataSourceFactory>())
                        .WithMetadata(typeof(ExportablePricelistAssignment).GetPropertyNames())
                        .WithTabularMetadata(typeof(TabularPricelistAssignment).GetPropertyNames()));
            }
        }

        public void Uninstall()
        {
            // no need to perform actions for now (Comment to remove Sonar warning)
        }

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
    }
}
