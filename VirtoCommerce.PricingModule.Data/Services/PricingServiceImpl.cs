using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Reflection;
using CacheManager.Core;
using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Common;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Serialization;
using VirtoCommerce.Platform.Data.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Data.Repositories;
using coreModel = VirtoCommerce.Domain.Pricing.Model;
using dataModel = VirtoCommerce.PricingModule.Data.Model;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricingServiceImpl : ServiceBase, IPricingService
    {
        private readonly Func<IPricingRepository> _repositoryFactory;
        private readonly IItemService _productService;
        private readonly ILog _logger;
        private readonly ICacheManager<object> _cacheManager;
        private readonly IExpressionSerializer _expressionSerializer;
        private readonly IPricingExtensionManager _extensionManager;

        public PricingServiceImpl(Func<IPricingRepository> repositoryFactory, IItemService productService, ILog logger, ICacheManager<object> cacheManager, IExpressionSerializer expressionSerializer,
                                  IPricingExtensionManager extensionManager)
        {
            _repositoryFactory = repositoryFactory;
            _productService = productService;
            _logger = logger;
            _cacheManager = cacheManager;
            _expressionSerializer = expressionSerializer;
            _extensionManager = extensionManager;
        }

        #region IPricingService Members
        /// <summary>
        /// Evaluate pricelists for special context. All resulting pricelists ordered by priority
        /// </summary>
        /// <param name="evalContext"></param>
        /// <returns></returns>
        public virtual IEnumerable<coreModel.Pricelist> EvaluatePriceLists(coreModel.PriceEvaluationContext evalContext)
        {
            var retVal = new List<coreModel.Pricelist>();

            Func<coreModel.PricelistAssignment[]> assignemntsGetters = () =>
            {
                using (var repository = _repositoryFactory())
                {
                    var allAssignments = repository.PricelistAssignments.Include(x => x.Pricelist).ToArray().Select(x => x.ToModel(AbstractTypeFactory<coreModel.PricelistAssignment>.TryCreateInstance())).ToArray();
                    foreach (var assignment in allAssignments.Where(x => !string.IsNullOrEmpty(x.ConditionExpression)))
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

        /// <summary>
        /// Evaluation product prices.
        /// Will get either all prices or one price per currency depending on the settings in evalContext.
        /// </summary>
        /// <param name="evalContext"></param>
        /// <returns></returns>
        public virtual IEnumerable<coreModel.Price> EvaluateProductPrices(coreModel.PriceEvaluationContext evalContext)
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

                evalContext.CertainDate = evalContext.CertainDate ?? DateTime.UtcNow;

                query = query
                    .Where(x => (x.StartDate <= evalContext.CertainDate && x.EndDate > evalContext.CertainDate)
                             || (x.StartDate <= evalContext.CertainDate && x.EndDate == null)
                             || (x.StartDate == null && x.EndDate > evalContext.CertainDate));

                if (evalContext.PricelistIds.IsNullOrEmpty())
                {
                    evalContext.PricelistIds = EvaluatePriceLists(evalContext).Select(x => x.Id).ToArray();
                }
                query = query.Where(x => evalContext.PricelistIds.Contains(x.PricelistId));
                prices = query
                    .ToArray()
                    .Select(x => x.ToModel(AbstractTypeFactory<coreModel.Price>.TryCreateInstance()))
                    .FilterByMostRelevant(evalContext.CertainDate.GetValueOrDefault())
                    .ToArray();
            }

            foreach (var productId in evalContext.ProductIds)
            {
                var productPrices = prices.Where(x => x.ProductId == productId);
                if (evalContext.ReturnAllMatchedPrices)
                {
                    // Get all prices, ordered by currency and amount.
                    var orderedPrices = productPrices.OrderBy(x => x.Currency).ThenBy(x => Math.Min(x.Sale ?? x.List, x.List));
                    retVal.AddRange(orderedPrices);
                }
                else
                {
                    //Order by priority
                    foreach (var currencyPricesGroup in productPrices.GroupBy(x => x.Currency))
                    {
                        var groupPrices = currencyPricesGroup.OrderBy(x => Math.Min(x.Sale ?? x.List, x.List));
                        if (!evalContext.PricelistIds.IsNullOrEmpty())
                        {
                            //return only prices from one prioritized price list
                            var prioritedPriceListId = evalContext.PricelistIds.FirstOrDefault(x => groupPrices.Any(y => y.PricelistId == x));
                            if (prioritedPriceListId != null)
                            {
                                retVal.AddRange(groupPrices.Where(x => x.PricelistId == prioritedPriceListId));
                            }
                        }
                    }
                }
            }

            //Then variation inherited prices
            if (_productService != null)
            {
                var productIdsWithoutPrice = evalContext.ProductIds.Except(retVal.Select(x => x.ProductId).Distinct()).ToArray();
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
                            var jObject = JObject.FromObject(inheritedPrice);
                            var variationPrice = (coreModel.Price)jObject.ToObject(inheritedPrice.GetType());
                            //For correct override price in possible update 
                            variationPrice.Id = null;
                            variationPrice.ProductId = variation.Id;
                            retVal.Add(variationPrice);
                        }
                    }
                }
            }

            return retVal;
        }

        // todo skip / take nullable
        public virtual IEnumerable<coreModel.PriceCalendarChange> GetCalendarChanges(DateTime? lastEvaluationTimestamp, DateTime? evaluationTimestamp, int? skip, int? take)
        {
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                // Increase command timeout to allow lengthy queries.
                var efContext = (ObjectContext)
                    repository.UnitOfWork.GetType().GetProperty("ObjectContext", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(repository.UnitOfWork);
                efContext.CommandTimeout = int.MaxValue;

                var query = repository.Prices
                    .Where(x => (x.EndDate < evaluationTimestamp && x.EndDate > lastEvaluationTimestamp)
                                || (x.StartDate <= evaluationTimestamp && x.StartDate > lastEvaluationTimestamp))
                    .OrderBy(x => x.ProductId) as IQueryable<dataModel.PriceEntity>;

                if (skip != null)
                    query = query.Skip(skip.Value);
                if (take != null)
                    query = query.Take(take.Value);

                var groupedQuery = query
                 .Select(x => x.ProductId)
                 .GroupBy(x => x);
                
                foreach (var calendarChange in groupedQuery.AsNoTracking())
                {
                    yield return new coreModel.PriceCalendarChange
                    {
                        ProductId = calendarChange.Key
                    };
                }
            }
        }

        public virtual coreModel.Price[] GetPricesById(string[] ids)
        {
            coreModel.Price[] result = null;
            if (ids != null)
            {
                using (var repository = _repositoryFactory())
                {
                    result = repository.GetPricesByIds(ids).Select(x => x.ToModel(AbstractTypeFactory<coreModel.Price>.TryCreateInstance())).ToArray();
                }
            }
            return result;
        }

        public virtual coreModel.PricelistAssignment[] GetPricelistAssignmentsById(string[] ids)
        {
            coreModel.PricelistAssignment[] result = null;
            if (ids != null)
            {
                using (var repository = _repositoryFactory())
                {
                    result = repository.GetPricelistAssignmentsById(ids).Select(x => x.ToModel(AbstractTypeFactory<coreModel.PricelistAssignment>.TryCreateInstance())).ToArray();
                }

                //Prepare expression tree for resulting assignments and include available  nodes to expression tree
                foreach (var assignment in result)
                {
                    var defaultExpressionTree = _extensionManager.ConditionExpressionTree;
                    //Set default expression tree first
                    assignment.DynamicExpression = defaultExpressionTree;
                    if (!string.IsNullOrEmpty(assignment.PredicateVisualTreeSerialized))
                    {
                        assignment.DynamicExpression = JsonConvert.DeserializeObject<ConditionExpressionTree>(assignment.PredicateVisualTreeSerialized);
                        if (defaultExpressionTree != null)
                        {
                            //Copy available elements from default tree because they not persisted
                            var sourceBlocks = ((DynamicExpression)defaultExpressionTree).Traverse(x => x.Children);
                            var targetBlocks = ((DynamicExpression)assignment.DynamicExpression).Traverse(x => x.Children).ToList();

                            foreach (var sourceBlock in sourceBlocks)
                            {
                                foreach (var targetBlock in targetBlocks.Where(x => x.Id == sourceBlock.Id))
                                {
                                    targetBlock.AvailableChildren = sourceBlock.AvailableChildren;
                                }
                            }
                            //copy available elements from default expression tree
                            assignment.DynamicExpression.AvailableChildren = defaultExpressionTree.AvailableChildren;
                        }
                    }
                }
            }
            return result;
        }

        public virtual coreModel.Pricelist[] GetPricelistsById(string[] ids)
        {
            coreModel.Pricelist[] result = null;
            if (ids != null)
            {
                using (var repository = _repositoryFactory())
                {
                    result = repository.GetPricelistByIds(ids).Select(x => x.ToModel(AbstractTypeFactory<coreModel.Pricelist>.TryCreateInstance())).ToArray();
                }
            }
            return result;
        }

        public virtual void SavePrices(coreModel.Price[] prices)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            using (var repository = _repositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var pricesIds = prices.Select(x => x.Id).Where(x => x != null).Distinct().ToArray();
                var alreadyExistPricesEntities = repository.Prices.Where(x => pricesIds.Contains(x.Id)).ToArray();

                //Create default priceLists for prices without pricelist 
                foreach (var priceWithoutPricelistGroup in prices.Where(x => x.PricelistId == null).GroupBy(x => x.Currency))
                {
                    var defaultPriceListId = GetDefaultPriceListName(priceWithoutPricelistGroup.Key);
                    if (GetPricelistsById(new[] { defaultPriceListId }).IsNullOrEmpty())
                    {
                        var defaultPriceList = AbstractTypeFactory<coreModel.Pricelist>.TryCreateInstance();
                        defaultPriceList.Id = defaultPriceListId;
                        defaultPriceList.Currency = priceWithoutPricelistGroup.Key;
                        defaultPriceList.Name = defaultPriceListId;
                        defaultPriceList.Description = defaultPriceListId;
                        repository.Add(AbstractTypeFactory<dataModel.PricelistEntity>.TryCreateInstance().FromModel(defaultPriceList, pkMap));
                    }
                    foreach (var priceWithoutPricelist in priceWithoutPricelistGroup)
                    {
                        priceWithoutPricelist.PricelistId = defaultPriceListId;
                    }
                }

                foreach (var price in prices)
                {
                    var sourceEntity = AbstractTypeFactory<dataModel.PriceEntity>.TryCreateInstance().FromModel(price, pkMap);
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

        public virtual void SavePricelists(coreModel.Pricelist[] priceLists)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            using (var repository = _repositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var pricelistsIds = priceLists.Select(x => x.Id).Where(x => x != null).Distinct().ToArray();
                var alreadyExistEntities = repository.Pricelists.Include(x => x.Assignments)
                                                     .Where(x => pricelistsIds.Contains(x.Id)).ToArray();
                foreach (var pricelist in priceLists)
                {
                    var sourceEntity = AbstractTypeFactory<dataModel.PricelistEntity>.TryCreateInstance().FromModel(pricelist, pkMap);
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

        public virtual void SavePricelistAssignments(coreModel.PricelistAssignment[] assignments)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            using (var repository = _repositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var assignmentsIds = assignments.Select(x => x.Id).Where(x => x != null).Distinct().ToArray();
                var alreadyExistEntities = repository.PricelistAssignments.Where(x => assignmentsIds.Contains(x.Id)).ToArray();
                foreach (var assignment in assignments)
                {
                    //Serialize condition expression 
                    if (assignment.DynamicExpression != null && assignment.DynamicExpression.Children != null)
                    {
                        var conditionExpression = assignment.DynamicExpression.GetConditionExpression();
                        assignment.ConditionExpression = _expressionSerializer.SerializeExpression(conditionExpression);

                        //Clear availableElements in expression (for decrease size)
                        assignment.DynamicExpression.AvailableChildren = null;
                        var allBlocks = ((DynamicExpression)assignment.DynamicExpression).Traverse(x => x.Children);
                        foreach (var block in allBlocks)
                        {
                            block.AvailableChildren = null;
                        }
                        assignment.PredicateVisualTreeSerialized = JsonConvert.SerializeObject(assignment.DynamicExpression);
                    }

                    var sourceEntity = AbstractTypeFactory<dataModel.PricelistAssignmentEntity>.TryCreateInstance().FromModel(assignment, pkMap);
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

        public virtual void DeletePrices(string[] ids)
        {
            using (var repository = _repositoryFactory())
            {
                repository.DeletePrices(ids);
                CommitChanges(repository);
                ResetCache();
            }
        }
        public virtual void DeletePricelists(string[] ids)
        {
            using (var repository = _repositoryFactory())
            {
                repository.DeletePricelists(ids);
                CommitChanges(repository);
                ResetCache();
            }
        }

        public virtual void DeletePricelistsAssignments(string[] ids)
        {
            using (var repository = _repositoryFactory())
            {
                repository.DeletePricelistAssignments(ids);
                CommitChanges(repository);
                ResetCache();
            }
        }
        #endregion



        private static string GetDefaultPriceListName(string currency)
        {
            var retVal = "Default" + currency;
            return retVal;
        }

        private void ResetCache()
        {
            //Clear cache (Smart cache implementation) 
            _cacheManager.ClearRegion("PricingModuleRegion");
        }


    }


}
