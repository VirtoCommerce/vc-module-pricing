using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Settings;

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

        private int? _batchSize;

        public PricingExportImport(IPricingService pricingService, IPricingSearchService pricingSearchService, ISettingsManager settingsManager)
        {
            _pricingService = pricingService;
            _pricingSearchService = pricingSearchService;
            _settingsManager = settingsManager;
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
            var backupObject = backupStream.DeserializeJson<BackupObject>();
            var progressInfo = new ExportImportProgressInfo();

            progressInfo.Description = String.Format("{0} price lists importing...", backupObject.Pricelists.Count());
            progressCallback(progressInfo);

            _pricingService.SavePricelists(backupObject.Pricelists.ToArray());
            _pricingService.SavePricelistAssignments(backupObject.Assignments.ToArray());

            progressInfo.TotalCount = backupObject.Prices.Count();
            for (int i = 0; i <= backupObject.Prices.Count(); i += BatchSize)
            {
                var prices = backupObject.Prices.Skip(i).Take(BatchSize).ToArray();
                _pricingService.SavePrices(prices);
                progressInfo.ProcessedCount += prices.Count();
                progressInfo.Description = string.Format("Prices: {0} of {1} importing...", progressInfo.ProcessedCount, progressInfo.TotalCount);
                progressCallback(progressInfo);
            }
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
