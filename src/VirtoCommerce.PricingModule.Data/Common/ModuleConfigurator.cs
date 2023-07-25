using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Search;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.PricingModule.Data.Common
{
    public class ModuleConfigurator
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IEnumerable<IndexDocumentConfiguration> _documentIndexingConfigurations;
        private readonly IPricingDocumentChangesProvider _changesProvider;
        private readonly ProductPriceDocumentBuilder _priceDocumentBuilder;

        public ModuleConfigurator(ISettingsManager settingsManager, IEnumerable<IndexDocumentConfiguration> documentIndexingConfigurations, IPricingDocumentChangesProvider changesProvider, ProductPriceDocumentBuilder priceDocumentBuilder)
        {
            _settingsManager = settingsManager;
            _documentIndexingConfigurations = documentIndexingConfigurations;
            _changesProvider = changesProvider;
            _priceDocumentBuilder = priceDocumentBuilder;
        }

        public async Task ConfigureSearchAsync()
        {
            if (_documentIndexingConfigurations != null)
            {
                // Add or remove price document source to catalog product indexing configuration
                foreach (var configuration in _documentIndexingConfigurations.Where(c => c.DocumentType == KnownDocumentTypes.Product))
                {
                    configuration.RelatedSources ??= new List<IndexDocumentSource>();

                    var oldSource = configuration.RelatedSources.FirstOrDefault(x => x.ChangesProvider.GetType() == _changesProvider.GetType() && x.DocumentBuilder.GetType() == _priceDocumentBuilder.GetType());
                    if (oldSource != null)
                    {
                        configuration.RelatedSources.Remove(oldSource);
                    }

                    var priceIndexingEnabled = await _settingsManager.GetValueAsync<bool>(Core.ModuleConstants.Settings.General.PricingIndexing);
                    if (priceIndexingEnabled)
                    {
                        var productPriceDocumentSource = new IndexDocumentSource
                        {
                            ChangesProvider = _changesProvider,
                            DocumentBuilder = _priceDocumentBuilder
                        };

                        configuration.RelatedSources.Add(productPriceDocumentSource);
                    }
                }
            }
        }
    }
}
