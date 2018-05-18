using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Data.StreamJsonFetcher;

namespace VirtoCommerce.PricingModule.Web.ExportImport
{
    public sealed class BackupObject
    {
        public ICollection<Pricelist> Pricelists { get; set; }
        public ICollection<Price> Prices { get; set; }
        public ICollection<PricelistAssignment> Assignments { get; set; }
    }

    public sealed class PricingExportImport
    {
        private readonly IPricingService _pricingService;
        private readonly IPricingSearchService _pricingSearchService;
        private readonly ISettingsManager _settingsManager;
        private readonly StreamFetcherFactory _streamFetcherFactory;

        private int? _batchSize;

        public PricingExportImport(IPricingService pricingService, IPricingSearchService pricingSearchService, ISettingsManager settingsManager, StreamFetcherFactory fetcherFactory)
        {
            _pricingService = pricingService;
            _pricingSearchService = pricingSearchService;
            _settingsManager = settingsManager;
            _streamFetcherFactory = fetcherFactory;
        }

        private int BatchSize
        {
            get
            {
                if (_batchSize == null)
                {
                    _batchSize = _settingsManager.GetValue("Pricing.ExportImport.PageSize", 50);
                }

                return (int) _batchSize;
            }
        }

        public void DoExport(Stream backupStream, Action<ExportImportProgressInfo> progressCallback)
        {
            var backupObject = GetBackupObject(progressCallback);
            backupObject.SerializeJson(backupStream);
        }

        public void DoImport(Stream backupStream, Action<ExportImportProgressInfo> progressCallback)
        {
            var progressInfo = new ExportImportProgressInfo();

            using (var fetcher = _streamFetcherFactory.Create(backupStream))
            {
                var pricelists = fetcher.FetchArray<Pricelist>("Pricelists").ToArray();


                progressInfo.Description = $"{pricelists.Count()} price lists importing...";
                progressCallback(progressInfo);

                _pricingService.SavePricelists(pricelists);

                var prices = fetcher.FetchArray<Price>("Prices");

                var chunkPrices = new List<Price>();

                foreach (var price in prices)
                {
                    if (chunkPrices.Count < BatchSize)
                    {
                        chunkPrices.Add(price);
                    }
                    else
                    {
                        ShowPricesProgressInfo(progressCallback, progressInfo, chunkPrices.Count);

                        SavePricesAndClearCollection(chunkPrices);
                    }
                }

                if (chunkPrices.Count > 0)
                {
                    ShowPricesProgressInfo(progressCallback, progressInfo, chunkPrices.Count);

                    SavePricesAndClearCollection(chunkPrices);
                }

                var assignments = fetcher.FetchArray<PricelistAssignment>("Assignments").ToArray();

                progressInfo.Description = $"{assignments.Count()} assignments importing...";
                progressCallback(progressInfo);

                _pricingService.SavePricelistAssignments(assignments);
            }
        }

        private void SavePricesAndClearCollection(ICollection<Price> prices)
        {
            _pricingService.SavePrices(prices.ToArray());
            prices.Clear();
        }

        private void ShowPricesProgressInfo(Action<ExportImportProgressInfo> progressCallback, ExportImportProgressInfo progressInfo, int chunkCount)
        {
            progressInfo.ProcessedCount += chunkCount;
            progressInfo.Description = $"Prices: {progressInfo.ProcessedCount} importing...";
            progressCallback(progressInfo);
        }
   
        private BackupObject GetBackupObject(Action<ExportImportProgressInfo> progressCallback)
        {
            var priceListsResult = _pricingSearchService.SearchPricelists(new Domain.Pricing.Model.Search.PricelistSearchCriteria { Take = int.MaxValue });
            //remove redundant info to decrease serialization size
            foreach(var priceList in priceListsResult.Results)
            {
                priceList.Assignments = null;
            }
            var assignmentsResult = _pricingSearchService.SearchPricelistAssignments(new Domain.Pricing.Model.Search.PricelistAssignmentsSearchCriteria { Take = int.MaxValue });
            foreach (var assignment in assignmentsResult.Results)
            {
                assignment.Pricelist = null;
                assignment.DynamicExpression = null;
            }
            var progressInfo = new ExportImportProgressInfo { Description = String.Format("{0} price lists loading..." , priceListsResult.TotalCount)};
            progressCallback(progressInfo);
            var retVal = new BackupObject
            {
                Pricelists = priceListsResult.Results,
                Assignments = assignmentsResult.Results,
                Prices = new List<Price>()
            };

            foreach(var priceList in retVal.Pricelists)
            {
                progressInfo.Description = String.Format("Loading {0} prices ...", priceList.Name);
                progressCallback(progressInfo);
                var result = _pricingSearchService.SearchPrices(new Domain.Pricing.Model.Search.PricesSearchCriteria { Take = int.MaxValue, PriceListId = priceList.Id });
                foreach (var price in result.Results)
                {
                    price.Pricelist = null;               
                }
                retVal.Prices.AddRange(result.Results);
            }
            return retVal;
        }

    }
}
