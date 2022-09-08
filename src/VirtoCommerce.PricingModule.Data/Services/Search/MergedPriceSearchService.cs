using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule.Data.Services.Search
{
    public class MergedPriceSearchService : IMergedPriceSearchService
    {
        private readonly Func<IPricingRepository> _repositoryFactory;
        private readonly IItemService _itemService;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        //private readonly IBlobUrlResolver _blobUrlResolver;
        //private readonly ICrudService<Price> _crudService;

        public MergedPriceSearchService(Func<IPricingRepository> repositoryFactory, IItemService itemService, IPlatformMemoryCache platformMemoryCache)
        {
            _repositoryFactory = repositoryFactory;
            _platformMemoryCache = platformMemoryCache;
            //_crudService = crudService;
        }

        public async Task<MergedPriceGroupSearchResult> SearchGroupsAsync(MergedPriceSearchCriteria criteria)
        {
            var result = AbstractTypeFactory<MergedPriceGroupSearchResult>.TryCreateInstance();

            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var query = BuildQuery(repository, criteria);

                var groupedQuery = query.GroupBy(x => x.ProductId);

                result.TotalCount = await groupedQuery.CountAsync();

                groupedQuery = groupedQuery
                .OrderBy(x => x.Key)
                .Skip(criteria.Skip)
                .Take(criteria.Take);

                var resultGroups = await groupedQuery.Select(x => new MergedPriceGroupEntity
                {
                    ProductId = x.Key,
                    GroupPricesCount = x.Count(),
                    MinListPrice = x.Min(g => g.List),
                    MaxListPrice = x.Max(g => g.List),
                    MinSalePrice = x.Min(g => g.Sale),
                    MaxSalePrice = x.Max(g => g.Sale),
                    GroupState = x.Max(g => g.State)
                }).ToListAsync();

                result.Results = resultGroups
                .Select(x => x.ToModel(AbstractTypeFactory<MergedPriceGroup>.TryCreateInstance()))
                .ToList();
            }

            var products = await _itemService.GetByIdsAsync(result.Results.Select(x => x.ProductId).ToArray(), ItemResponseGroup.ItemInfo.ToString());
            foreach (var group in result.Results)
            {
                var product = products.FirstOrDefault(x => x.Id == group.ProductId);
                if (product != null)
                {
                    group.ProductCode = product.Code;
                    group.ProductName = product.Name;
                    group.ProductImg = product.ImgSrc;
                }
            }

            return result;
        }

        public async Task<MergedPriceSearchResult> SearchAsync(MergedPriceSearchCriteria criteria)
        {
            var result = AbstractTypeFactory<MergedPriceSearchResult>.TryCreateInstance();

            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var query = BuildQuery(repository, criteria);

                result.TotalCount = await query.CountAsync();

                query = query
                    .OrderBy(x => x.Id)
                    .Skip(criteria.Skip)
                    .Take(criteria.Take);

                var results = await query.ToListAsync();

                result.Results = results
                    .Select(x => x.ToModel(AbstractTypeFactory<MergedPrice>.TryCreateInstance()))
                    .ToList();
            }

            return result;
        }

        protected IQueryable<MergedPriceEntity> BuildQuery(IPricingRepository repository, MergedPriceSearchCriteria criteria)
        {
            var query = repository.GetMergedPrices(criteria.BasePriceListId, criteria.PriorityPriceListId);

            if (!criteria.ProductIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.ProductIds.Contains(x.ProductId));
            }

            return query;
        }
    }
}
