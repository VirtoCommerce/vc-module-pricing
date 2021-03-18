using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace VirtoCommerce.PricingModule.Core.Model.ExportImport
{
    public class ConfigureSimpleExportOptions : IConfigureOptions<SimpleExportOptions>
    {
        private readonly IConfiguration _configuration;

        public ConfigureSimpleExportOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(SimpleExportOptions options)
        {
            _configuration.GetSection("Pricing:SimpleExport").Bind(options);
        }
    }
}
