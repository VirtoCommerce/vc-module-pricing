using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Serialization;
using VirtoCommerce.Platform.Core.Web.Security;
using VirtoCommerce.PricingModule.Web.Converters;
using VirtoCommerce.PricingModule.Web.Security;
using coreModel = VirtoCommerce.Domain.Pricing.Model;
using webModel = VirtoCommerce.PricingModule.Web.Model;

namespace VirtoCommerce.PricingModule.Web.Controllers.Api
{
    [RoutePrefix("")]
    [CheckPermission(Permission = PricingPredefinedPermissions.Read)]
    public class PricingModuleController : ApiController
    {
        private readonly IPricingService _pricingService;
        private readonly IPricingSearchService _pricingSearchService;
        private readonly IItemService _itemService;
        private readonly ICatalogService _catalogService;
        private readonly IPricingExtensionManager _extensionManager;
        private readonly IExpressionSerializer _expressionSerializer;
        private readonly IBlobUrlResolver _blobUrlResolver;


        public PricingModuleController(IPricingService pricingService, IItemService itemService, ICatalogService catalogService, IPricingExtensionManager extensionManager, IExpressionSerializer expressionSerializer, IPricingSearchService pricingSearchService, IBlobUrlResolver blobUrlResolver)
        {
            _extensionManager = extensionManager;
            _expressionSerializer = expressionSerializer;
            _pricingService = pricingService;
            _itemService = itemService;
            _catalogService = catalogService;
            _pricingSearchService = pricingSearchService;
            _blobUrlResolver = blobUrlResolver;
        }

        /// <summary>
        /// Evaluate prices by given context
        /// </summary>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>Prices array</returns>
        [HttpPost]
        [ResponseType(typeof(webModel.Price[]))]
        [Route("api/pricing/evaluate")]
        public IHttpActionResult EvaluatePrices(coreModel.PriceEvaluationContext evalContext)
        {
            var retVal = _pricingService.EvaluateProductPrices(evalContext)
                                        .Select(x => x.ToWebModel())
                                        .ToArray();

            return Ok(retVal);
        }


        /// <summary>
        /// Evaluate pricelists by given context
        /// </summary>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>Pricelist array</returns>
        [HttpPost]
        [ResponseType(typeof(webModel.Pricelist[]))]
        [Route("api/pricing/pricelists/evaluate")]
        public IHttpActionResult EvaluatePriceLists(coreModel.PriceEvaluationContext evalContext)
        {
            var retVal = _pricingService.EvaluatePriceLists(evalContext)
                                        .Select(x => x.ToWebModel())
                                        .ToArray();
            return Ok(retVal);
        }
        /// <summary>
        /// Get pricelist assignment
        /// </summary>
        /// <param name="id">Pricelist assignment id</param>
        [HttpGet]
        [ResponseType(typeof(webModel.PricelistAssignment))]
        [Route("api/pricing/assignments/{id}")]
        public IHttpActionResult GetPricelistAssignmentById(string id)
        {
            var assignment = _pricingService.GetPricelistAssignmentsById(new[] { id }).FirstOrDefault();
            var result = assignment != null ? assignment.ToWebModel(_extensionManager.ConditionExpressionTree) : null;
            return result != null ? Ok(result) : (IHttpActionResult)NotFound();
        }

        /// <summary>
        /// Get a new pricelist assignment
        /// </summary>
        /// <remarks>Get a new pricelist assignment object. Create new pricelist assignment, but does not save one.</remarks>
        [HttpGet]
        [ResponseType(typeof(webModel.PricelistAssignment))]
        [Route("api/pricing/assignments/new")]
        public IHttpActionResult GetNewPricelistAssignments()
        {
            var result = new webModel.PricelistAssignment
            {
                Priority = 1,
                DynamicExpression = _extensionManager.ConditionExpressionTree
            };
            return Ok(result);
        }

        /// <summary>
        /// Get pricelists
        /// </summary>
        /// <remarks>Get all pricelists for all catalogs.</remarks>
        [HttpGet]
        [ResponseType(typeof(webModel.PricelistSearchResult))]
        [Route("api/pricing/pricelists")]
        public IHttpActionResult SearchPricelists([FromUri]coreModel.Search.PricelistSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new coreModel.Search.PricelistSearchCriteria();
            }
            var result = _pricingSearchService.SearchPricelists(criteria);
            var retVal = new webModel.PricelistSearchResult
            {
                TotalCount = result.TotalCount,
                Pricelists = result.Results.Select(x => x.ToWebModel()).ToList()
            };
            return Ok(retVal);
        }

        /// <summary>
        /// Search pricelist assignments
        /// </summary>
        /// <remarks>Search price list assignments by given criteria</remarks>
        [HttpGet]
        [ResponseType(typeof(webModel.PricelistAssignmentSearchResult))]
        [Route("api/pricing/assignments")]
        public IHttpActionResult SearchPricelistAssignments([FromUri]coreModel.Search.PricelistAssignmentsSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new Domain.Pricing.Model.Search.PricelistAssignmentsSearchCriteria();
            }
            var result = _pricingSearchService.SearchPricelistAssignments(criteria);
            var retVal = new webModel.PricelistAssignmentSearchResult
            {
                TotalCount = result.TotalCount,
                Assignments = result.Results.Select(x => x.ToWebModel(_extensionManager.ConditionExpressionTree)).ToList()
            };
            return Ok(retVal);
        }

        /// <summary>
        /// Search product prices 
        /// </summary>
        /// <remarks>Search product prices</remarks>
        [HttpGet]
        [ResponseType(typeof(webModel.ProductPricesSearchResult))]
        [Route("api/catalog/products/prices/search")]
        public IHttpActionResult SearchProductPrices([FromUri]coreModel.Search.PricesSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new Domain.Pricing.Model.Search.PricesSearchCriteria();
            }
            var result = _pricingSearchService.SearchPrices(criteria);
            var retVal = new webModel.ProductPricesSearchResult
            {
                TotalCount = result.TotalCount,
                ProductPrices = new List<webModel.ProductPrice>()
            };

            foreach (var productPricesGroup in result.Results.GroupBy(x => x.ProductId))
            {
                var productPrice = new webModel.ProductPrice
                {
                    ProductId = productPricesGroup.Key,
                    Prices = productPricesGroup.Select(y => y.ToWebModel()).ToList()
                };
                var product = productPricesGroup.Select(x => x.Product).FirstOrDefault(x => x != null);
                if (product != null)
                {
                    productPrice.Product = product.ToWebModel(_blobUrlResolver);
                }
                retVal.ProductPrices.Add(productPrice);
            }

            return Ok(retVal);
        }

        /// <summary>
        /// Evaluate  product prices
        /// </summary>
        /// <remarks>Get an array of valid product prices for each currency.</remarks>
        /// <param name="productId">Product id</param>
        [HttpGet]
        [ResponseType(typeof(webModel.Price[]))]
        [Route("api/products/{productId}/prices")]
        public IHttpActionResult EvaluateProductPrices(string productId)
        {
            var priceEvalContext = new coreModel.PriceEvaluationContext { ProductIds = new[] { productId } };
            var product = _itemService.GetByIds(new[] { productId }, Domain.Catalog.Model.ItemResponseGroup.ItemInfo).FirstOrDefault();
            if (product != null)
            {
                priceEvalContext.CatalogId = product.CatalogId;
            }
            return EvaluatePrices(priceEvalContext);
        }

        /// <summary>
        /// Create pricelist assignment
        /// </summary>
        /// <param name="assignment">PricelistAssignment</param>
        [HttpPost]
        [ResponseType(typeof(webModel.PricelistAssignment))]
        [Route("api/pricing/assignments")]
        [CheckPermission(Permission = PricingPredefinedPermissions.Create)]
        public IHttpActionResult CreatePricelistAssignment(webModel.PricelistAssignment assignment)
        {
            var coreAssignment = assignment.ToCoreModel(_expressionSerializer);
            _pricingService.SavePricelistAssignments(new[] { coreAssignment });
            var result = coreAssignment.ToWebModel(_extensionManager.ConditionExpressionTree);
            return Ok(result);
        }

        /// <summary>
        /// Update pricelist assignment
        /// </summary>
        /// <param name="assignment">PricelistAssignment</param>
        /// <todo>Return no any reason if can't update</todo>
        [HttpPut]
        [ResponseType(typeof(void))]
        [Route("api/pricing/assignments")]
        [CheckPermission(Permission = PricingPredefinedPermissions.Update)]
        public IHttpActionResult UpdatePriceListAssignment(webModel.PricelistAssignment assignment)
        {
            _pricingService.SavePricelistAssignments(new[] { assignment.ToCoreModel(_expressionSerializer) });
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPut]
        [ResponseType(typeof(void))]
        [Route("api/products/prices")]
        [CheckPermission(Permission = PricingPredefinedPermissions.Update)]
        public IHttpActionResult UpdateProductsPrices(webModel.ProductPrice[] productPrices)
        {
            var result = _pricingSearchService.SearchPrices(new Domain.Pricing.Model.Search.PricesSearchCriteria { Take = int.MaxValue, ProductIds = productPrices.Select(x => x.ProductId).ToArray() });
            var targetPricesGroups = result.Results.GroupBy(x => x.PricelistId);
            var sourcePricesGroups = productPrices.SelectMany(x => x.Prices).Select(x => x.ToCoreModel()).GroupBy(x => x.PricelistId);

            var changedPrices = new List<coreModel.Price>();
            var deletedPrices = new List<coreModel.Price>();

            foreach (var sourcePricesGroup in sourcePricesGroups)
            {
                var targetPricesGroup = targetPricesGroups.FirstOrDefault(x => x.Key == sourcePricesGroup.Key);
                if (targetPricesGroup != null)
                {
                    sourcePricesGroup.ToArray().CompareTo(targetPricesGroup.ToArray(), EqualityComparer<coreModel.Price>.Default, (state, x, y) =>
                    {
                        switch (state)
                        {
                            case EntryState.Modified:
                            case EntryState.Added:
                                changedPrices.Add(x);
                                break;
                            case EntryState.Deleted:
                                deletedPrices.Add(x);
                                break;
                        }
                    });
                }
                else
                {
                    changedPrices.AddRange(sourcePricesGroup);
                }
            }
            _pricingService.SavePrices(changedPrices.ToArray());
            if (!deletedPrices.IsNullOrEmpty())
            {
                _pricingService.DeletePrices(changedPrices.Select(x => x.Id).ToArray());
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPut]
        [ResponseType(typeof(void))]
        [Route("api/products/{productId}/prices")]
        [CheckPermission(Permission = PricingPredefinedPermissions.Update)]
        public IHttpActionResult UpdateProductPrices(webModel.ProductPrice productPrice)
        {
            return UpdateProductsPrices(new[] { productPrice });
        }

        /// <summary>
        /// Get all price lists for product
        /// </summary>
        /// <remarks>Get all price lists for given product.</remarks>
        /// <param name="productId">Product id</param>
        [HttpGet]
        [ResponseType(typeof(webModel.Pricelist[]))]
        [Route("api/catalog/products/{productId}/pricelists")]
        public IHttpActionResult GetProductPriceLists(string productId)
        {
            var productPrices = _pricingSearchService.SearchPrices(new Domain.Pricing.Model.Search.PricesSearchCriteria { Take = int.MaxValue, ProductId = productId }).Results
                                                      .Select(x => x.ToWebModel()).ToArray();
            var priceLists = _pricingSearchService.SearchPricelists(new coreModel.Search.PricelistSearchCriteria { Take = int.MaxValue }).Results
                                                  .Select(x => x.ToWebModel()).ToArray();
            foreach (var pricelist in priceLists)
            {
                pricelist.Prices = productPrices.Where(x => x.PricelistId == pricelist.Id).ToList();
            }
            return Ok(priceLists);
        }

        /// <summary>
        /// Get pricelist
        /// </summary>
        /// <param name="id">Pricelist id</param>
        [HttpGet]
        [ResponseType(typeof(webModel.Pricelist))]
        [Route("api/pricing/pricelists/{id}")]
        public IHttpActionResult GetPriceListById(string id)
        {
            webModel.Pricelist result = null;
            var pricelist = _pricingService.GetPricelistsById(new[] { id }).FirstOrDefault();
            if (pricelist != null)
            {
                result = pricelist.ToWebModel(_extensionManager.ConditionExpressionTree);
            }
            return result != null ? Ok(result) : (IHttpActionResult)NotFound();
        }


        /// <summary>
        /// Create pricelist
        /// </summary>
        [HttpPost]
        [ResponseType(typeof(webModel.Pricelist))]
        [Route("api/pricing/pricelists")]
        [CheckPermission(Permission = PricingPredefinedPermissions.Create)]
        public IHttpActionResult CreatePriceList(webModel.Pricelist priceList)
        {
            var corePriceList = priceList.ToCoreModel(_expressionSerializer);
            _pricingService.SavePricelists(new[] { corePriceList });
            return Ok(corePriceList.ToWebModel());
        }

        /// <summary>
        /// Update pricelist
        /// </summary>
        [HttpPut]
        [ResponseType(typeof(void))]
        [Route("api/pricing/pricelists")]
        [CheckPermission(Permission = PricingPredefinedPermissions.Update)]
        public IHttpActionResult UpdatePriceList(webModel.Pricelist priceList)
        {
            var corePriceList = priceList.ToCoreModel(_expressionSerializer);
            _pricingService.SavePricelists(new[] { corePriceList });
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Delete pricelist assignments
        /// </summary>
        /// <remarks>Delete pricelist assignment by given array of ids.</remarks>
        /// <param name="ids">An array of pricelist assignment ids</param>
        /// <todo>Return no any reason if can't update</todo>
        [HttpDelete]
        [ResponseType(typeof(void))]
        [Route("api/pricing/assignments")]
        [CheckPermission(Permission = PricingPredefinedPermissions.Delete)]
        public IHttpActionResult DeleteAssignments([FromUri] string[] ids)
        {
            _pricingService.DeletePricelistsAssignments(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Delete all prices for specified product in specified price list
        /// </summary>
        /// <param name="pricelistId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseType(typeof(void))]
        [Route("api/pricing/pricelists/{pricelistId}/products/prices")]
        [CheckPermission(Permission = PricingPredefinedPermissions.Update)]
        public IHttpActionResult DeleteProductPrices(string pricelistId, [FromUri]string[] productIds)
        {
            var result = _pricingSearchService.SearchPrices(new coreModel.Search.PricesSearchCriteria { PriceListId = pricelistId, ProductIds = productIds, Take = int.MaxValue });
            _pricingService.DeletePrices(result.Results.Select(x => x.Id).ToArray());
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Delete pricelists  
        /// </summary>
        /// <remarks>Delete pricelists by given array of pricelist ids.</remarks>
        /// <param name="ids">An array of pricelist ids</param>
        [HttpDelete]
        [ResponseType(typeof(void))]
        [Route("api/pricing/pricelists")]
        [CheckPermission(Permission = PricingPredefinedPermissions.Delete)]
        public IHttpActionResult DeletePricelists([FromUri] string[] ids)
        {
            _pricingService.DeletePricelists(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
