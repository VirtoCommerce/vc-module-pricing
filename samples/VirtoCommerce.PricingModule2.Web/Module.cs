using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.PostgreSql.Extensions;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Services;
using VirtoCommerce.PricingModule2.Web.Entities;
using VirtoCommerce.PricingModule2.Web.Models;
using VirtoCommerce.PricingModule2.Web.Repositories;
using VirtoCommerce.PricingModule2.Web.Services;

namespace VirtoCommerce.PricingModule2.Web
{
    public class Module : IModule
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        internal static IConfiguration Configuration { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<Pricing2DbContext>((provider, options) =>
            {
                Configuration = provider.GetRequiredService<IConfiguration>();

                var connectionString = Configuration.GetConnectionString(ModuleInfo.Id) ??
                                       Configuration.GetConnectionString("VirtoCommerce.Pricing") ??
                                       Configuration.GetConnectionString("VirtoCommerce");

                options.UsePostgreSqlDatabase(connectionString, typeof(Pricing2DbContext), Configuration);
            });

            serviceCollection.AddScoped<IPricingRepository, PricingRepository2>();
            serviceCollection.AddScoped<PricingRepositoryImpl, PricingRepository2>();
            serviceCollection.AddTransient<IPricingEvaluatorService, PricingEvaluatorService2>();
            serviceCollection.AddTransient<PricingEvaluatorService, PricingEvaluatorService2>();
            serviceCollection.AddTransient<IPriceSearchService, PriceSearchService2>();
            serviceCollection.AddTransient<PriceSearchService, PriceSearchService2>();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.General.AllSettings, ModuleInfo.Id);

            AbstractTypeFactory<Price>.OverrideType<Price, Price2>();
            AbstractTypeFactory<PriceEntity>.OverrideType<PriceEntity, Price2Entity>();

            using var serviceScope = appBuilder.ApplicationServices.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<Pricing2DbContext>();
            dbContext.Database.Migrate();
        }

        public void Uninstall()
        {
            // Logic to run when the module is uninstalled.
        }
    }
}
