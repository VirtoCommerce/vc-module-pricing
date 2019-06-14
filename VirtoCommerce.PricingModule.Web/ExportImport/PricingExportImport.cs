using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Domain.Pricing.Model.Search;
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

                return (int)_batchSize;
            }
        }

        public void DoExport(Stream backupStream, Action<ExportImportProgressInfo> progressCallback)
        {
            var progressInfo = new ExportImportProgressInfo { Description = "loading data..." };
            progressCallback(progressInfo);

            using (var sw = new StreamWriter(backupStream, Encoding.UTF8))
            using (var writer = new JsonTextWriter(sw))
            {
                writer.WriteStartObject();

                progressInfo.Description = "Price lists are exporting...";
                progressCallback(progressInfo);

                #region Export price lists
                var totalCount = _pricingSearchService.SearchPricelists(new PricelistSearchCriteria { Take = 0 }).TotalCount;
                writer.WritePropertyName("PricelistsTotalCount");
                writer.WriteValue(totalCount);

                writer.WritePropertyName("Pricelists");
                writer.WriteStartArray();

                for (var i = 0; i < totalCount; i += BatchSize)
                {
                    var searchResponse = _pricingSearchService.SearchPricelists(new PricelistSearchCriteria { Skip = i, Take = BatchSize });
                    foreach (var priceList in searchResponse.Results)
                    {
                        priceList.Assignments = null;
                        _jsonSerializer.Serialize(writer, priceList);
                    }
                    writer.Flush();
                    progressInfo.Description = $"{ Math.Min(totalCount, i + BatchSize) } of { totalCount } price lists have been exported";
                    progressCallback(progressInfo);
                }
                writer.WriteEndArray();
                #endregion

                #region Export price list assignments
                totalCount = _pricingSearchService.SearchPricelistAssignments(new PricelistAssignmentsSearchCriteria { Take = 0 }).TotalCount;
                writer.WritePropertyName("AssignmentsTotalCount");
                writer.WriteValue(totalCount);

                writer.WritePropertyName("Assignments");
                writer.WriteStartArray();

                for (var i = 0; i < totalCount; i += BatchSize)
                {
                    var searchResponse = _pricingSearchService.SearchPricelistAssignments(new PricelistAssignmentsSearchCriteria { Skip = i, Take = BatchSize });
                    foreach (var assignment in searchResponse.Results)
                    {
                        assignment.Pricelist = null;
                        assignment.DynamicExpression = null;

                        _jsonSerializer.Serialize(writer, assignment);
                    }
                    writer.Flush();
                    progressInfo.Description = $"{ Math.Min(totalCount, i + BatchSize) } of { totalCount } price lits assignments have been exported";
                    progressCallback(progressInfo);
                }
                writer.WriteEndArray();
                #endregion

                #region Export prices
                totalCount = _pricingSearchService.SearchPrices(new PricesSearchCriteria { Take = 0 }).TotalCount;
                writer.WritePropertyName("PricesTotalCount");
                writer.WriteValue(totalCount);

                writer.WritePropertyName("Prices");
                writer.WriteStartArray();

                for (var i = 0; i < totalCount; i += BatchSize)
                {
                    var searchResponse = _pricingSearchService.SearchPrices(new PricesSearchCriteria { Skip = i, Take = BatchSize });
                    foreach (var price in searchResponse.Results)
                    {
                        price.Pricelist = null;
                        _jsonSerializer.Serialize(writer, price);
                    }
                    writer.Flush();
                    progressInfo.Description = $"{ Math.Min(totalCount, i + BatchSize) } of { totalCount } prices have been exported";
                    progressCallback(progressInfo);
                }
                writer.WriteEndArray();
                #endregion

                writer.WriteEndObject();
                writer.Flush();
            }
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

                            var pricelistArrayType = AbstractTypeFactory<Pricelist>.TryCreateInstance().GetType().MakeArrayType();
                            var pricelists = _jsonSerializer.Deserialize(reader, pricelistArrayType) as Pricelist[];

                            progressInfo.Description = $"{pricelists.Count()} price lists are importing...";
                            progressCallback(progressInfo);

                            _pricingService.SavePricelists(pricelists);

                        }
                        else if (readerValue == "Prices")
                        {
                            reader.Read();

                            if (reader.TokenType == JsonToken.StartArray)
                            {
                                reader.Read();

                                var pricesChunk = new List<Price>();

                                while (reader.TokenType != JsonToken.EndArray)
                                {
                                    var price = AbstractTypeFactory<Price>.TryCreateInstance();

                                    price = _jsonSerializer.Deserialize(reader, price.GetType()) as Price;

                                    pricesChunk.Add(price);

                                    reader.Read();

                                    if (pricesChunk.Count >= BatchSize || reader.TokenType == JsonToken.EndArray)
                                    {
                                        _pricingService.SavePrices(pricesChunk.ToArray());
                                        progressInfo.ProcessedCount += pricesChunk.Count;
                                        progressInfo.Description = $"Prices: {progressInfo.ProcessedCount} have been imported";
                                        progressCallback(progressInfo);

                                        pricesChunk.Clear();
                                    }
                                }
                            }
                        }
                        else if (readerValue == "Assignments")
                        {

                            reader.Read();

                            var assignmentArrayType = AbstractTypeFactory<PricelistAssignment>.TryCreateInstance().GetType().MakeArrayType();
                            var assignments = _jsonSerializer.Deserialize(reader, assignmentArrayType) as PricelistAssignment[];

                            progressInfo.Description = $"{assignments.Count()} assignments are importing...";
                            progressCallback(progressInfo);

                            _pricingService.SavePricelistAssignments(assignments);
                        }
                    }
                }
            }
        }
    }
}
