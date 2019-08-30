using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.Practices.Unity;
using VirtoCommerce.Domain.Catalog.Events;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.ExportModule.Core.Services;
using VirtoCommerce.ExportModule.Data.Extensions;
using VirtoCommerce.ExportModule.Data.Services;
using VirtoCommerce.Platform.Core.Bus;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.Platform.Data.Repositories;
using VirtoCommerce.PricingModule.Data.ExportImport;
using VirtoCommerce.PricingModule.Data.Handlers;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Search;
using VirtoCommerce.PricingModule.Data.Services;
using VirtoCommerce.PricingModule.Web.ExportImport;
using VirtoCommerce.PricingModule.Web.JsonConverters;
using VirtoCommerce.PricingModule.Web.Security;

namespace VirtoCommerce.PricingModule.Web
{
    public class Module : ModuleBase, ISupportExportImportModule
    {
        private readonly string _connectionString = ConfigurationHelper.GetConnectionStringValue("VirtoCommerce.Pricing") ?? ConfigurationHelper.GetConnectionStringValue("VirtoCommerce");
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void SetupDatabase()
        {
            using (var context = new PricingRepositoryImpl(_connectionString, _container.Resolve<AuditableInterceptor>()))
            {
                var initializer = new SetupDatabaseInitializer<PricingRepositoryImpl, Data.Migrations.Configuration>();
                initializer.InitializeDatabase(context);
            }
        }

        public override void Initialize()
        {
            var extensionManager = new DefaultPricingExtensionManagerImpl();
            _container.RegisterInstance<IPricingExtensionManager>(extensionManager);

            _container.RegisterType<IPricingRepository>(new InjectionFactory(c => new PricingRepositoryImpl(_connectionString, new EntityPrimaryKeyGeneratorInterceptor(), _container.Resolve<AuditableInterceptor>()
                , new ChangeLogInterceptor(_container.Resolve<Func<IPlatformRepository>>(), ChangeLogPolicy.Cumulative, new[] { nameof(PriceEntity) }))));

            _container.RegisterType<IPricingPriorityFilterPolicy, DefaultPricingPriorityFilterPolicy>();
            _container.RegisterType<IPricingService, PricingServiceImpl>();
            _container.RegisterType<IPricingSearchService, PricingSearchServiceImpl>();

            var eventHandlerRegistrar = _container.Resolve<IHandlerRegistrar>();

            eventHandlerRegistrar.RegisterHandler<ProductChangedEvent>(async (message, token) => await _container.Resolve<DeletePricesProductChangedEvent>().Handle(message));
            _container.RegisterType<IPricingDocumentChangesProvider, ProductPriceDocumentChangesProvider>();

            _container.RegisterType<PriceExportPagedDataSourceFactory>();
            _container.RegisterType<PricelistAssignmentExportPagedDataSourceFactory>();
            _container.RegisterType<PricelistExportPagedDataSourceFactory>();
        }

        public override void PostInitialize()
        {
            base.PostInitialize();

            // Next lines allow to use polymorph types in API controller methods
            var httpConfiguration = _container.Resolve<HttpConfiguration>();
            var storeJsonConverter = _container.Resolve<PolymorphicPricingJsonConverter>();
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(storeJsonConverter);

            #region Search

            var settingsManager = _container.Resolve<ISettingsManager>();
            var priceIndexingEnabled = settingsManager.GetValue("Pricing.Indexing.Enable", true);
            if (priceIndexingEnabled)
            {
                // Add price document source to the product indexing configuration
                var productIndexingConfigurations = _container.Resolve<IndexDocumentConfiguration[]>();
                if (productIndexingConfigurations != null)
                {
                    var productPriceDocumentSource = new IndexDocumentSource
                    {
                        ChangesProvider = _container.Resolve<IPricingDocumentChangesProvider>(),
                        DocumentBuilder = _container.Resolve<ProductPriceDocumentBuilder>(),
                    };

                    foreach (var configuration in productIndexingConfigurations.Where(c =>
                        c.DocumentType == KnownDocumentTypes.Product))
                    {
                        if (configuration.RelatedSources == null)
                        {
                            configuration.RelatedSources = new List<IndexDocumentSource>();
                        }

                        configuration.RelatedSources.Add(productPriceDocumentSource);
                    }
                }
            }

            #endregion

            var registrar = _container.Resolve<IKnownExportTypesRegistrar>();

            registrar.RegisterType(
                ExportedTypeDefinitionBuilder.Build<ExportablePrice, PriceExportDataQuery>()
                    .WithDataSourceFactory(_container.Resolve<PriceExportPagedDataSourceFactory>())
                    .WithPermissionAuthorization(PricingPredefinedPermissions.Export, PricingPredefinedPermissions.Read)
                    .WithMetadata(typeof(ExportablePrice).GetPropertyNames())
                    .WithTabularMetadata(typeof(TabularPrice).GetPropertyNames()));

            registrar.RegisterType(
                ExportedTypeDefinitionBuilder.Build<ExportablePricelist, PricelistExportDataQuery>()
                    .WithDataSourceFactory(_container.Resolve<PricelistExportPagedDataSourceFactory>())
                    .WithPermissionAuthorization(PricingPredefinedPermissions.Export, PricingPredefinedPermissions.Read)
                    .WithMetadata(typeof(ExportablePricelist).GetPropertyNames())
                    .WithTabularMetadata(typeof(TabularPricelist).GetPropertyNames()));

            registrar.RegisterType(
                ExportedTypeDefinitionBuilder.Build<ExportablePricelistAssignment, PricelistAssignmentExportDataQuery>()
                    .WithDataSourceFactory(_container.Resolve<PricelistAssignmentExportPagedDataSourceFactory>())
                    .WithPermissionAuthorization(PricingPredefinedPermissions.Export, PricingPredefinedPermissions.Read)
                    .WithMetadata(typeof(ExportablePricelistAssignment).GetPropertyNames())
                    .WithTabularMetadata(typeof(TabularPricelistAssignment).GetPropertyNames()));
        }

        #endregion

        #region ISupportExportImportModule Members

        public void DoExport(System.IO.Stream outStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var exportJob = _container.Resolve<PricingExportImport>();
            exportJob.DoExport(outStream, progressCallback);
        }

        public void DoImport(System.IO.Stream inputStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var exportJob = _container.Resolve<PricingExportImport>();
            exportJob.DoImport(inputStream, progressCallback);
        }

        public string ExportDescription
        {
            get
            {
                var settingManager = _container.Resolve<ISettingsManager>();
                return settingManager.GetValue("Pricing.ExportImport.Description", string.Empty);
            }
        }

        #endregion
    }
}
