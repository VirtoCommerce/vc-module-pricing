using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.ExportImport
{
    public sealed class PricingExportImport
    {
        private readonly IPriceSearchService _priceSearchService;
        private readonly IPriceService _priceService;
        private readonly IPricelistSearchService _pricelistSearchService;
        private readonly IPricelistService _pricelistService;
        private readonly IPricelistAssignmentSearchService _pricelistAssignmentSearchService;
        private readonly IPricelistAssignmentService _pricelistAssignmentService;
        private readonly ISettingsManager _settingsManager;
        private readonly JsonSerializer _jsonSerializer;

        private int? _batchSize;

        public PricingExportImport(
            IPriceSearchService priceSearchService,
            IPriceService priceService,
            IPricelistSearchService pricelistSearchService,
            IPricelistService pricelistService,
            IPricelistAssignmentSearchService pricelistAssignmentSearchService,
            IPricelistAssignmentService pricelistAssignmentService,
            ISettingsManager settingsManager,
            JsonSerializer jsonSerializer)
        {
            _priceSearchService = priceSearchService;
            _priceService = priceService;
            _pricelistSearchService = pricelistSearchService;
            _pricelistService = pricelistService;
            _pricelistAssignmentSearchService = pricelistAssignmentSearchService;
            _pricelistAssignmentService = pricelistAssignmentService;
            _settingsManager = settingsManager;
            _jsonSerializer = jsonSerializer;
        }

        private int BatchSize
        {
            get
            {
                _batchSize ??= _settingsManager.GetValue<int>(ModuleConstants.Settings.General.ExportImportPageSize);

                return (int)_batchSize;
            }
        }

        public async Task DoExportAsync(Stream backupStream, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var progressInfo = new ExportImportProgressInfo { Description = "loading data..." };
            progressCallback(progressInfo);

            using (var sw = new StreamWriter(backupStream, Encoding.UTF8))
            using (var writer = new JsonTextWriter(sw))
            {
                await writer.WriteStartObjectAsync();

                progressInfo.Description = "Price lists exporting...";
                progressCallback(progressInfo);

                #region Export price lists

                await writer.WritePropertyNameAsync("Pricelists");

                await writer.SerializeArrayWithPagingAsync(_jsonSerializer, BatchSize, async (skip, take) =>
                    (GenericSearchResult<Pricelist>)await _pricelistSearchService.SearchNoCloneAsync(new PricelistSearchCriteria { Skip = skip, Take = take })
                , (processedCount, totalCount) =>
                {
                    progressInfo.Description = $"{processedCount} of {totalCount} pricelists have been exported";
                    progressCallback(progressInfo);
                }, cancellationToken);

                #endregion

                #region Export price list assignments

                await writer.WritePropertyNameAsync("Assignments");

                await writer.SerializeArrayWithPagingAsync(_jsonSerializer, BatchSize, async (skip, take) =>
                    (GenericSearchResult<PricelistAssignment>)await _pricelistAssignmentSearchService.SearchNoCloneAsync(new PricelistAssignmentsSearchCriteria { Skip = skip, Take = take })
                , (processedCount, totalCount) =>
                {
                    progressInfo.Description = $"{processedCount} of {totalCount} pricelist assignments have been exported";
                    progressCallback(progressInfo);
                }, cancellationToken);

                #endregion

                #region Export prices

                await writer.WritePropertyNameAsync("Prices");

                await writer.SerializeArrayWithPagingAsync(_jsonSerializer, BatchSize, async (skip, take) =>
                    (GenericSearchResult<Price>)await _priceSearchService.SearchNoCloneAsync(new PricesSearchCriteria { Skip = skip, Take = take })
                , (processedCount, totalCount) =>
                {
                    progressInfo.Description = $"{processedCount} of {totalCount} prices have been exported";
                    progressCallback(progressInfo);
                }, cancellationToken);

                #endregion

                await writer.WriteEndObjectAsync();
                await writer.FlushAsync();
            }
        }

        public async Task DoImportAsync(Stream stream, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var progressInfo = new ExportImportProgressInfo();

            using (var streamReader = new StreamReader(stream))
            using (var reader = new JsonTextReader(streamReader))
            {
                while (await reader.ReadAsync())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        var readerValue = reader.Value.ToString();

                        if (readerValue == "Pricelists")
                        {
                            await reader.DeserializeArrayWithPagingAsync<Pricelist>(_jsonSerializer, BatchSize, _pricelistService.SaveChangesAsync, processedCount =>
                            {
                                progressInfo.Description = $"{processedCount} price lists have been imported";
                                progressCallback(progressInfo);
                            }, cancellationToken);
                        }
                        else if (readerValue == "Prices")
                        {
                            await reader.DeserializeArrayWithPagingAsync<Price>(_jsonSerializer, BatchSize, _priceService.SaveChangesAsync, processedCount =>
                            {
                                progressInfo.Description = $"Prices: {progressInfo.ProcessedCount} have been imported";
                                progressCallback(progressInfo);
                            }, cancellationToken);
                        }
                        else if (readerValue == "Assignments")
                        {
                            await reader.DeserializeArrayWithPagingAsync<PricelistAssignment>(_jsonSerializer, BatchSize, _pricelistAssignmentService.SaveChangesAsync, processedCount =>
                            {
                                progressInfo.Description = $"{progressInfo.ProcessedCount} assignments have been imported";
                                progressCallback(progressInfo);
                            }, cancellationToken);
                        }
                    }
                }
            }
        }
    }
}
