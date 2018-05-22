using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
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
        private readonly JsonSerializer _jsonSerializer;

        private int? _batchSize;

        public PricingExportImport(IPricingService pricingService, IPricingSearchService pricingSearchService, ISettingsManager settingsManager)
        {
            _pricingService = pricingService;
            _pricingSearchService = pricingSearchService;
            _settingsManager = settingsManager;

            _jsonSerializer = new JsonSerializer
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
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

        public void DoImport(Stream stream, Action<ExportImportProgressInfo> progressCallback)
        {
            var progressInfo = new ExportImportProgressInfo();

            using (var streamReader = new StreamReader(stream))
            using (var reader = new JsonTextReader(streamReader))
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        var readerValue = reader.Value.ToString();

                        if (readerValue == "Pricelists")
                        {
                            reader.Read();

                            var pricelists = _jsonSerializer.Deserialize<Pricelist[]>(reader);

                            progressInfo.Description = $"{pricelists.Count()} price lists importing...";
                            progressCallback(progressInfo);

                            _pricingService.SavePricelists(pricelists);

                        } else if (readerValue == "Prices")
                        {
                            reader.Read();

                            if (reader.TokenType == JsonToken.StartArray)
                            {
                                reader.Read();

                                var pricesChunk = new List<Price>();

                                while (reader.TokenType != JsonToken.EndArray)
                                {
                                    var price = _jsonSerializer.Deserialize<Price>(reader);
                                    pricesChunk.Add(price);

                                    reader.Read();

                                    if (pricesChunk.Count >= BatchSize || reader.TokenType == JsonToken.EndArray )
                                    {

                                        progressInfo.ProcessedCount += pricesChunk.Count;
                                        progressInfo.Description = $"Prices: {progressInfo.ProcessedCount} importing...";
                                        progressCallback(progressInfo);

                                        _pricingService.SavePrices(pricesChunk.ToArray());

                                        pricesChunk.Clear();
                                    }
                                }
                            }
                        } else if (readerValue == "Assignments")
                        {

                            reader.Read();

                            var assignments = _jsonSerializer.Deserialize<PricelistAssignment[]>(reader);

                            progressInfo.Description = $"{assignments.Count()} assignments importing...";
                            progressCallback(progressInfo);

                            _pricingService.SavePricelistAssignments(assignments);
                        }
                    }
                }
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
