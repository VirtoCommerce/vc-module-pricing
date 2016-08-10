using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CacheManager.Core;
using Common.Logging;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Common;
using VirtoCommerce.Domain.Pricing.Model.Search;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Serialization;
using VirtoCommerce.Platform.Data.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Data.Converters;
using VirtoCommerce.PricingModule.Data.Repositories;
using coreModel = VirtoCommerce.Domain.Pricing.Model;
using dataModel = VirtoCommerce.PricingModule.Data.Model;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricingServiceImpl : ServiceBase, IPricingService, IPricingSearchService
    {
        private readonly Func<IPricingRepository> _repositoryFactory;
        private readonly IItemService _productService;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly ICatalogService _catalogService;
        private readonly ILog _logger;
        private readonly ICacheManager<object> _cacheManager;
        private readonly IExpressionSerializer _expressionSerializer;

        public PricingServiceImpl(Func<IPricingRepository> repositoryFactory, IItemService productService, ILog logger, ICacheManager<object> cacheManager, IExpressionSerializer expressionSerializer, ICatalogService catalogService, ICatalogSearchService catalogSearchService)
        {
            _repositoryFactory = repositoryFactory;
            _productService = productService;
            _logger = logger;
            _cacheManager = cacheManager;
            _expressionSerializer = expressionSerializer;
            _catalogService = catalogService;
            _catalogSearchService = catalogSearchService;
        }


        #region IPricingSearchService Members
        public SearchResult Search(SearchCriteria criteria)
        {
            SearchResult retVal = new SearchResult();
            using (var repository = _repositoryFactory())
            {
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
                    var catalogSearchResult = _catalogSearchService.Search(new Domain.Catalog.Model.SearchCriteria { Keyword = criteria.Keyword, Skip = criteria.Skip, Take = criteria.Take, Sort = criteria.Sort, ResponseGroup = Domain.Catalog.Model.SearchResponseGroup.WithProducts });
                    var productIds = catalogSearchResult.Products.Select(x => x.Id).ToArray();
                    query = query.Where(x => productIds.Contains(x.ProductId));
                }
                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[] { new SortInfo { SortColumn = "List" } };
                }


                query = query.OrderBySortInfos(sortInfos);

                if (criteria.GroupByProducts)
                {
                    var groupedQuery = query.GroupBy(x => x.ProductId).OrderBy(x => 1);
                    retVal.TotalCount = groupedQuery.Count();
                    query = groupedQuery.Skip(criteria.Skip).Take(criteria.Take).SelectMany(x => x);
                }
                else
                {
                    retVal.TotalCount = query.Count();
                    query = query.Skip(criteria.Skip).Take(criteria.Take);
                }

                retVal.Prices = query.ToArray().Select(x => x.ToCoreModel()).ToList();
            }
            return retVal;
        }
        #endregion

        #region IPricingService Members
        /// <summary>
        /// Evaluate pricelists for special context. All resulting pricelists ordered by priority
        /// </summary>
        /// <param name="evalContext"></param>
        /// <returns></returns>
        public IEnumerable<coreModel.Pricelist> EvaluatePriceLists(coreModel.PriceEvaluationContext evalContext)
        {
            var retVal = new List<coreModel.Pricelist>();

            Func<coreModel.PricelistAssignment[]> assignemntsGetters = () =>
            {
                using (var repository = _repositoryFactory())
                {
                    var allAssignments = repository.PricelistAssignments.Include(x => x.Pricelist).ToArray().Select(x => x.ToCoreModel()).ToArray();
                    foreach (var assignment in allAssignments)
                    {
                        try
                        {
                            //Deserialize conditions
                            assignment.Condition = _expressionSerializer.DeserializeExpression<Func<IEvaluationContext, bool>>(assignment.ConditionExpression);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex);
                        }
                    }
                    return allAssignments;
                };
            };
            IQueryable<coreModel.PricelistAssignment> query = null;
            if (_cacheManager != null)
            {
                query = _cacheManager.Get("PricingServiceImpl.EvaluatePriceLists", "PricingModuleRegion", assignemntsGetters).AsQueryable();
            }
            else
            {
                query = assignemntsGetters().AsQueryable();
            }

            if (evalContext.CatalogId != null)
            {
                //filter by catalog
                query = query.Where(x => x.CatalogId == evalContext.CatalogId);
            }

            if (evalContext.Currency != null)
            {
                //filter by currency
                query = query.Where(x => x.Pricelist.Currency == evalContext.Currency.ToString());
            }
            if (evalContext.CertainDate != null)
            {
                //filter by date expiration
                query = query.Where(x => (x.StartDate == null || evalContext.CertainDate >= x.StartDate) && (x.EndDate == null || x.EndDate >= evalContext.CertainDate));
            }
            var assignments = query.OrderByDescending(x => x.Priority).ThenByDescending(x => x.Name).ToArray();
            retVal.AddRange(assignments.Where(x => x.Condition == null).Select(x => x.Pricelist));

            foreach (var assignment in assignments.Where(x => x.Condition != null))
            {
                try
                {
                    if (assignment.Condition(evalContext))
                    {
                        if (retVal.All(p => p.Id != assignment.Pricelist.Id))
                        {
                            retVal.Add(assignment.Pricelist);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            return retVal;
        }

        public IEnumerable<coreModel.Price> EvaluateProductPrices(coreModel.PriceEvaluationContext evalContext)
        {
            if (evalContext == null)
            {
                throw new ArgumentNullException("evalContext");
            }
            if (evalContext.ProductIds == null)
            {
                throw new MissingFieldException("ProductIds");
            }

            var retVal = new List<coreModel.Price>();
            coreModel.Price[] prices;
            using (var repository = _repositoryFactory())
            {
                //Get a price range satisfying by passing context
                var query = repository.Prices.Include(x => x.Pricelist)
                                             .Where(x => evalContext.ProductIds.Contains(x.ProductId))
                                             .Where(x => evalContext.Quantity >= x.MinQuantity || evalContext.Quantity == 0);

                if (evalContext.PricelistIds.IsNullOrEmpty())
                {
                    evalContext.PricelistIds = EvaluatePriceLists(evalContext).Select(x => x.Id).ToArray();
                }
                //query = query.Where(x => evalContext.PricelistIds.Contains(x.PricelistId));
                prices = query.ToArray().Select(x => x.ToCoreModel()).ToArray();
            }

            //add to result prices with passed price lists
            var resultingPrices = prices.Where(x => evalContext.PricelistIds.Contains(x.PricelistId)).ToList();
            //then for missed products prices need return all prices in system (default prices)
            var productIdsWithoutPrice = evalContext.ProductIds.Except(retVal.Select(x => x.ProductId).Distinct()).ToArray();
            resultingPrices.AddRange(prices.Where(x => productIdsWithoutPrice.Contains(x.ProductId)));
            //Then variation inherited prices
            if (_productService != null)
            {
                productIdsWithoutPrice = evalContext.ProductIds.Except(retVal.Select(x => x.ProductId).Distinct()).ToArray();
                //Variation price inheritance
                //Need find products without price it may be a variation without implicitly price defined and try to get price from main product
                if (productIdsWithoutPrice.Any())
                {
                    var variations = _productService.GetByIds(productIdsWithoutPrice, Domain.Catalog.Model.ItemResponseGroup.ItemInfo).Where(x => x.MainProductId != null).ToList();
                    evalContext.ProductIds = variations.Select(x => x.MainProductId).Distinct().ToArray();

                    foreach (var inheritedPrice in EvaluateProductPrices(evalContext))
                    {
                        foreach (var variation in variations.Where(x => x.MainProductId == inheritedPrice.ProductId))
                        {
                            var variationPrice = (coreModel.Price)inheritedPrice.Clone();
                            //For correct override price in possible update 
                            variationPrice.Id = null;
                            variationPrice.ProductId = variation.Id;
                            resultingPrices.Add(variationPrice);
                        }
                    }
                }
            }

            //Order by priority
            foreach (var currencyPricesGroup in resultingPrices.GroupBy(x => x.Currency))
            {
                var groupPrices = currencyPricesGroup.OrderBy(x => 1);
                if (evalContext.PricelistIds != null)
                {
                    //Construct ordered groups of list prices (ordered by pricelist priority taken from pricelistid array as index)
                    groupPrices = groupPrices.OrderBy(x => Array.IndexOf(evalContext.PricelistIds, x.PricelistId));
                }
                //Order by  price value
                var orderedPrices = groupPrices.ThenBy(x => Math.Min(x.Sale ?? x.List, x.List));
                retVal.AddRange(orderedPrices);
            }

            return retVal;
        }

        public virtual coreModel.Price GetPriceById(string id)
        {
            coreModel.Price retVal = null;

            using (var repository = _repositoryFactory())
            {
                var entity = repository.GetPriceById(id);
                if (entity != null)
                {
                    retVal = entity.ToCoreModel();
                }
            }
            return retVal;
        }

        public virtual IEnumerable<coreModel.Price> GetPricesById(IEnumerable<string> ids)
        {
            coreModel.Price[] result = null;

            if (ids != null)
            {
                using (var repository = _repositoryFactory())
                {
                    var entities = repository.Prices
                        .Where(p => ids.Contains(p.Id))
                        .ToList();

                    result = entities
                        .Select(entity => entity.ToCoreModel())
                        .ToArray();
                }
            }

            return result;
        }


        public IEnumerable<coreModel.Pricelist> GetPriceLists()
        {
            List<coreModel.Pricelist> retVal;

            using (var repository = _repositoryFactory())
            {
                retVal = repository.Pricelists.ToArray().Select(x => x.ToCoreModel()).ToList();
            }

            return retVal;
        }

        public virtual coreModel.Pricelist GetPricelistById(string id)
        {
            coreModel.Pricelist retVal = null;

            using (var repository = _repositoryFactory())
            {
                var entity = repository.GetPricelistById(id);

                if (entity != null)
                {
                    retVal = entity.ToCoreModel();

                    var assignments = repository.GetAllPricelistAssignments(id);
                    retVal.Assignments = assignments.Select(x => x.ToCoreModel()).ToList();
                }
            }
            return retVal;
        }

        public void SavePrices(coreModel.Price[] prices)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            using (var repository = _repositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var pricesIds = prices.Select(x => x.Id).Where(x => x != null).Distinct().ToArray();
                var alreadyExistPricesEntities = repository.Prices.Where(x => pricesIds.Contains(x.Id)).ToArray();

                //Create default priceLists for prices without pricelist 
                foreach(var priceWithoutPricelistGroup in prices.Where(x => x.PricelistId == null).GroupBy(x=>x.Currency))
                {
                    var defaultPriceListId = GetDefaultPriceListName(priceWithoutPricelistGroup.Key);
                    if(GetPricelistById(defaultPriceListId) == null)
                    {
                        var defaultPriceList = new coreModel.Pricelist
                        {
                            Id = defaultPriceListId,
                            Currency = priceWithoutPricelistGroup.Key,
                            Name = defaultPriceListId,
                            Description = defaultPriceListId
                        }.ToDataModel(pkMap);
                        repository.Add(defaultPriceList);
                    }
                    foreach(var priceWithoutPricelist in priceWithoutPricelistGroup)
                    {
                        priceWithoutPricelist.PricelistId = defaultPriceListId;
                    }
                }

                foreach (var price in prices)
                {
                    var sourceEntity = price.ToDataModel(pkMap);
                    var targetEntity = alreadyExistPricesEntities.FirstOrDefault(x => x.Id == price.Id);
                    if (targetEntity != null)
                    {
                        changeTracker.Attach(targetEntity);
                        sourceEntity.Patch(targetEntity);
                    }
                    else
                    {                     
                        repository.Add(sourceEntity);
                    }
                }
                CommitChanges(repository);
                pkMap.ResolvePrimaryKeys();
                ResetCache();
            }
        }

        public void SavePricelists(coreModel.Pricelist[] priceLists)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            using (var repository = _repositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var pricelistsIds = priceLists.Select(x => x.Id).Where(x => x != null).Distinct().ToArray();
                var alreadyExistEntities = repository.Pricelists.Where(x => pricelistsIds.Contains(x.Id)).ToArray();
                foreach (var pricelist in priceLists)
                {
                    var sourceEntity = pricelist.ToDataModel(pkMap);
                    var targetEntity = alreadyExistEntities.FirstOrDefault(x => x.Id == pricelist.Id);
                    if (targetEntity != null)
                    {
                        changeTracker.Attach(targetEntity);
                        sourceEntity.Patch(targetEntity);
                    }
                    else
                    {
                        repository.Add(sourceEntity);
                    }
                }
                CommitChanges(repository);
                pkMap.ResolvePrimaryKeys();
                ResetCache();
            }

        }

        public virtual void DeletePrices(string[] ids)
        {
            GenericDelete(ids, (repository, id) => repository.GetPriceById(id));
        }
        public virtual void DeletePricelists(string[] ids)
        {
            GenericDelete(ids, (repository, id) => repository.GetPricelistById(id));
        }


        public coreModel.PricelistAssignment GetPricelistAssignmentById(string id)
        {
            dataModel.PricelistAssignment retVal;

            using (var repository = _repositoryFactory())
            {
                retVal = repository.GetPricelistAssignmentById(id);
            }

            return retVal != null ? retVal.ToCoreModel() : null;
        }

        public IEnumerable<coreModel.PricelistAssignment> GetPriceListAssignments()
        {
            List<coreModel.PricelistAssignment> retVal;

            using (var repository = _repositoryFactory())
            {
                retVal = repository.PricelistAssignments.ToArray().Select(x => x.ToCoreModel()).ToList();
            }

            return retVal;
        }

        public void SavePricelistAssignments(coreModel.PricelistAssignment[] assignments)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            using (var repository = _repositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var assignmentsIds = assignments.Select(x => x.Id).Where(x => x != null).Distinct().ToArray();
                var alreadyExistEntities = repository.PricelistAssignments.Where(x => assignmentsIds.Contains(x.Id)).ToArray();
                foreach (var assignment in assignments)
                {
                    var sourceEntity = assignment.ToDataModel(pkMap);
                    var targetEntity = alreadyExistEntities.FirstOrDefault(x => x.Id == assignment.Id);
                    if (targetEntity != null)
                    {
                        changeTracker.Attach(targetEntity);
                        sourceEntity.Patch(targetEntity);
                    }
                    else
                    {
                        repository.Add(sourceEntity);
                    }
                }
                CommitChanges(repository);
                pkMap.ResolvePrimaryKeys();
                ResetCache();
            }
        }

        public void DeletePricelistsAssignments(string[] ids)
        {
            GenericDelete(ids, (repository, id) => repository.GetPricelistAssignmentById(id));
        }

        #endregion



        private static string GetDefaultPriceListName(string currency)
        {
            var retVal = "Default" + currency;
            return retVal;
        }

        private void GenericDelete(string[] ids, Func<IPricingRepository, string, Entity> getter)
        {
            using (var repository = _repositoryFactory())
            {
                foreach (var id in ids)
                {
                    var entity = getter(repository, id);
                    repository.Remove(entity);
                }
                CommitChanges(repository);
                ResetCache();
            }
        }

        private void ResetCache()
        {
            //Clear cache (Smart cache implementation) 
            _cacheManager.ClearRegion("PricingModuleRegion");
        }

  
      
       
    }


}
