using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Pricing.Model.Search;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using coreModel = VirtoCommerce.Domain.Pricing.Model;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricingSearchServiceImpl : IPricingSearchService
    {
        private readonly Func<IPricingRepository> _repositoryFactory;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly IPricingService _pricingService;

        private readonly Dictionary<string, string> _pricesSortingAliases = new Dictionary<string, string>();

        public PricingSearchServiceImpl(Func<IPricingRepository> repositoryFactory, ICatalogSearchService catalogSearchService, IPricingService pricingService)
        {
            _repositoryFactory = repositoryFactory;
            _catalogSearchService = catalogSearchService;
            _pricingService = pricingService;

            _pricesSortingAliases["prices"] = ReflectionUtility.GetPropertyName<coreModel.Price>(x => x.List);
        }


        #region IPricingSearchService Members

        public virtual PricingSearchResult<coreModel.Price> SearchPrices(PricesSearchCriteria criteria)
        {
            var result = new PricingSearchResult<coreModel.Price>();

            ICollection<CatalogProduct> products;

            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var query = GetPricesQuery(repository, criteria, out products);

                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[] { new SortInfo { SortColumn = ReflectionUtility.GetPropertyName<coreModel.Price>(x => x.List) } };
                }
                //Try to replace sorting columns names
                TryTransformSortingInfoColumnNames(_pricesSortingAliases, sortInfos);

                query = query.OrderBySortInfos(sortInfos).ThenBy(x => x.Id);

                if (criteria.GroupByProducts)
                {
                    var groupedQuery = query.GroupBy(x => x.ProductId).OrderBy(x => 1);
                    result.TotalCount = groupedQuery.Count();
                    query = groupedQuery.Skip(criteria.Skip).Take(criteria.Take).SelectMany(x => x);
                }
                else
                {
                    result.TotalCount = query.Count();
                    query = query.Skip(criteria.Skip).Take(criteria.Take);
                }

                var pricesIds = query.Select(x => x.Id).ToList();
                result.Results = _pricingService.GetPricesById(pricesIds.ToArray())
                                            .OrderBy(x => pricesIds.IndexOf(x.Id))
                                            .ToList();
            }

            return result;
        }

        public virtual PricingSearchResult<coreModel.Pricelist> SearchPricelists(PricelistSearchCriteria criteria)
        {
            var result = new PricingSearchResult<coreModel.Pricelist>();

            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var query = GetPricelistsQuery(repository, criteria);

                result.TotalCount = query.Count();

                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[] { new SortInfo { SortColumn = ReflectionUtility.GetPropertyName<coreModel.Pricelist>(x => x.Name) } };
                }

                query = query.OrderBySortInfos(sortInfos).ThenBy(x => x.Id);

                query = query.Skip(criteria.Skip).Take(criteria.Take);

                var pricelistsIds = query.Select(x => x.Id).ToList();
                result.Results = _pricingService.GetPricelistsById(pricelistsIds.ToArray())
                                                .OrderBy(x => pricelistsIds.IndexOf(x.Id)).ToList();
            }
            return result;
        }

        public virtual PricingSearchResult<coreModel.PricelistAssignment> SearchPricelistAssignments(PricelistAssignmentsSearchCriteria criteria)
        {
            var result = new PricingSearchResult<coreModel.PricelistAssignment>();

            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var query = GetPricelistAssignmentsQuery(repository, criteria);

                result.TotalCount = query.Count();

                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[] { new SortInfo { SortColumn = ReflectionUtility.GetPropertyName<coreModel.PricelistAssignment>(x => x.Priority) } };
                }

                query = query.OrderBySortInfos(sortInfos).ThenBy(x => x.Id);

                query = query.Skip(criteria.Skip).Take(criteria.Take);

                var pricelistAssignmentsIds = query.Select(x => x.Id).ToList();
                result.Results = _pricingService.GetPricelistAssignmentsById(pricelistAssignmentsIds.ToArray())
                                                .OrderBy(x => pricelistAssignmentsIds.IndexOf(x.Id))
                                                .ToList();
            }

            return result;
        }
        #endregion


        protected virtual IQueryable<PriceEntity> GetPricesQuery(IPricingRepository repository, PricesSearchCriteria criteria, out ICollection<CatalogProduct> products)
        {
            products = new List<CatalogProduct>();

            var query = repository.Prices;

            if (!criteria.PriceListIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.PriceListIds.Contains(x.PricelistId));
            }

            if (!criteria.ProductIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.ProductIds.Contains(x.ProductId));
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var catalogSearchCriteria = new SearchCriteria
                {
                    Keyword = criteria.Keyword,
                    Skip = criteria.Skip,
                    Take = criteria.Take,
                    Sort = criteria.Sort.Replace("product.", string.Empty),
                    ResponseGroup = SearchResponseGroup.WithProducts,
                };
                var catalogSearchResult = _catalogSearchService.Search(catalogSearchCriteria);

                var productIds = catalogSearchResult.Products.Select(x => x.Id).ToArray();
                //preserve resulting products for future assignment to prices
                products = catalogSearchResult.Products;

                query = query.Where(x => productIds.Contains(x.ProductId));
            }

            if (criteria.ModifiedSince.HasValue)
            {
                query = query.Where(x => x.ModifiedDate >= criteria.ModifiedSince);
            }

            return query;
        }

        protected virtual IQueryable<PricelistEntity> GetPricelistsQuery(IPricingRepository repository, PricelistSearchCriteria criteria)
        {
            var query = repository.Pricelists;

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword) || x.Description.Contains(criteria.Keyword));
            }

            if (!criteria.Currencies.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.Currencies.Contains(x.Currency));
            }

            return query;
        }

        protected virtual IQueryable<PricelistAssignmentEntity> GetPricelistAssignmentsQuery(IPricingRepository repository, PricelistAssignmentsSearchCriteria criteria)
        {
            var query = repository.PricelistAssignments;

            if (!criteria.PriceListIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.PriceListIds.Contains(x.PricelistId));
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword) || x.Description.Contains(criteria.Keyword));
            }

            if (!criteria.CatalogIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.CatalogIds.Contains(x.CatalogId));
            }

            return query;
        }


        private static void TryTransformSortingInfoColumnNames(IDictionary<string, string> transformationMap, SortInfo[] sortingInfos)
        {
            //Try to replace sorting columns names
            foreach (var sortInfo in sortingInfos)
            {
                if (transformationMap.TryGetValue(sortInfo.SortColumn.ToLowerInvariant(), out var newColumnName))
                {
                    sortInfo.SortColumn = newColumnName;
                }
            }
        }


    }
}

