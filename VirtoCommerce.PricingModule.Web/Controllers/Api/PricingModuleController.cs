using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Common;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Domain.Pricing.Model.Search;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Serialization;
using VirtoCommerce.Platform.Core.Web.Security;
using VirtoCommerce.PricingModule.Web.Security;
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
        private readonly IBlobUrlResolver _blobUrlResolver;


        public PricingModuleController(IPricingService pricingService, IItemService itemService, ICatalogService catalogService, IPricingExtensionManager extensionManager, IPricingSearchService pricingSearchService, IBlobUrlResolver blobUrlResolver)
        {
            _extensionManager = extensionManager;
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
        [ResponseType(typeof(Price[]))]
        [Route("api/pricing/evaluate")]
        public IHttpActionResult EvaluatePrices(PriceEvaluationContext evalContext)
        {
            var retVal = _pricingService.EvaluateProductPrices(evalContext).ToArray();

            return Ok(retVal);
        }


        /// <summary>
        /// Evaluate pricelists by given context
        /// </summary>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>Pricelist array</returns>
        [HttpPost]
        [ResponseType(typeof(Pricelist[]))]
        [Route("api/pricing/pricelists/evaluate")]
        public IHttpActionResult EvaluatePriceLists(PriceEvaluationContext evalContext)
        {
            var retVal = _pricingService.EvaluatePriceLists(evalContext).ToArray();
            return Ok(retVal);
        }
        /// <summary>
        /// Get pricelist assignment
        /// </summary>
        /// <param name="id">Pricelist assignment id</param>
        [HttpGet]
        [ResponseType(typeof(PricelistAssignment))]
        [Route("api/pricing/assignments/{id}")]
        public IHttpActionResult GetPricelistAssignmentById(string id)
        {
            var assignment = _pricingService.GetPricelistAssignmentsById(new[] { id }).FirstOrDefault();
            return Ok(assignment);
        }

        /// <summary>
        /// Get a new pricelist assignment
        /// </summary>
        /// <remarks>Get a new pricelist assignment object. Create new pricelist assignment, but does not save one.</remarks>
        [HttpGet]
        [ResponseType(typeof(PricelistAssignment))]
        [Route("api/pricing/assignments/new")]
        public IHttpActionResult GetNewPricelistAssignments()
        {
            var result = AbstractTypeFactory<PricelistAssignment>.TryCreateInstance();
            result.Priority = 1;
            result.DynamicExpression = _extensionManager.ConditionExpressionTree;          
            return Ok(result);
        }

        /// <summary>
        /// Get pricelists
        /// </summary>
        /// <remarks>Get all pricelists for all catalogs.</remarks>
        [HttpGet]
        [ResponseType(typeof(PricingSearchResult<Pricelist>))]
        [Route("api/pricing/pricelists")]
        public IHttpActionResult SearchPricelists([FromUri] PricelistSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new PricelistSearchCriteria();
            }
            var result = _pricingSearchService.SearchPricelists(criteria);         
            return Ok(result);
        }

        /// <summary>
        /// Search pricelist assignments
        /// </summary>
        /// <remarks>Search price list assignments by given criteria</remarks>
        [HttpGet]
        [ResponseType(typeof(PricingSearchResult<PricelistAssignment>))]
        [Route("api/pricing/assignments")]
        public IHttpActionResult SearchPricelistAssignments([FromUri]PricelistAssignmentsSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new PricelistAssignmentsSearchCriteria();
            }
            var result = _pricingSearchService.SearchPricelistAssignments(criteria);  
            return Ok(result);
        }

        /// <summary>
        /// Search product prices 
        /// </summary>
        /// <remarks>Search product prices</remarks>
        [HttpGet]
        [ResponseType(typeof(PricingSearchResult<webModel.ProductPrice>))]
        [Route("api/catalog/products/prices/search")]
        public IHttpActionResult SearchProductPrices([FromUri]PricesSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new PricesSearchCriteria();
            }
            var result = _pricingSearchService.SearchPrices(criteria);
            var retVal = new PricingSearchResult<webModel.ProductPrice>
            {
                TotalCount = result.TotalCount,
                Results = new List<webModel.ProductPrice>()
            };

            var products = _itemService.GetByIds(result.Results.Select(x=>x.ProductId).Distinct().ToArray(), Domain.Catalog.Model.ItemResponseGroup.ItemInfo);
            foreach (var productPricesGroup in result.Results.GroupBy(x => x.ProductId))
            {
                var productPrice = new webModel.ProductPrice
                {
                    ProductId = productPricesGroup.Key,
                    Prices = productPricesGroup.ToList()
                };
                var product = products.FirstOrDefault(x => x.Id == productPricesGroup.Key);
                if (product != null)
                {
                    productPrice.Product = product.ToWebModel(_blobUrlResolver);
                }
                retVal.Results.Add(productPrice);
            }

            return Ok(retVal);
        }

        /// <summary>
        /// Evaluate  product prices
        /// </summary>
        /// <remarks>Get an array of valid product prices for each currency.</remarks>
        /// <param name="productId">Product id</param>
        [HttpGet]
        [ResponseType(typeof(Price[]))]
        [Route("api/products/{productId}/prices")]
        public IHttpActionResult EvaluateProductPrices(string productId)
        {
            var priceEvalContext = new PriceEvaluationContext { ProductIds = new[] { productId } };
            var product = _itemService.GetByIds(new[] { productId }, Domain.Catalog.Model.ItemResponseGroup.ItemInfo).FirstOrDefault();
            if (product != null)
            {
                priceEvalContext.CatalogId = product.CatalogId;
            }
            return EvaluatePrices(priceEvalContext);
        }

        /// <summary>
        /// Evaluate product prices for demand catalog
        /// </summary>
        /// <remarks>Get an array of valid product prices for each currency.</remarks>
        /// <param name="productId">Product id</param>
        /// <param name="catalogId">Catalog id</param>
        [HttpGet]
        [ResponseType(typeof(Price[]))]
        [Route("api/products/{productId}/{catalogId}/pricesWidget")]
        public IHttpActionResult EvaluateProductPrices(string productId, string catalogId)
        {
            var priceEvalContext = new PriceEvaluationContext
            {
                ProductIds = new[] { productId },
                CatalogId = catalogId
            };
            return EvaluatePrices(priceEvalContext);
        }

        /// <summary>
        /// Create pricelist assignment
        /// </summary>
        /// <param name="assignment">PricelistAssignment</param>
        [HttpPost]
        [ResponseType(typeof(PricelistAssignment))]
        [Route("api/pricing/assignments")]
        [CheckPermission(Permission = PricingPredefinedPermissions.Create)]
        public IHttpActionResult CreatePricelistAssignment(PricelistAssignment assignment)
        {
            _pricingService.SavePricelistAssignments(new[] { assignment });
            return Ok(assignment);
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
        public IHttpActionResult UpdatePriceListAssignment(PricelistAssignment assignment)
        {
            _pricingService.SavePricelistAssignments(new[] { assignment });
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
            var sourcePricesGroups = productPrices.SelectMany(x => x.Prices).GroupBy(x => x.PricelistId);

            var changedPrices = new List<Price>();
            var deletedPrices = new List<Price>();

            foreach (var sourcePricesGroup in sourcePricesGroups)
            {
                var targetPricesGroup = targetPricesGroups.FirstOrDefault(x => x.Key == sourcePricesGroup.Key);
                if (targetPricesGroup != null)
                {
                    sourcePricesGroup.ToArray().CompareTo(targetPricesGroup.ToArray(), EqualityComparer<Price>.Default, (state, x, y) =>
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
                _pricingService.DeletePrices(deletedPrices.Select(x => x.Id).ToArray());
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
        [ResponseType(typeof(Pricelist[]))]
        [Route("api/catalog/products/{productId}/pricelists")]
        public IHttpActionResult GetProductPriceLists(string productId)
        {
            var productPrices = _pricingSearchService.SearchPrices(new PricesSearchCriteria { Take = int.MaxValue, ProductId = productId }).Results;
            var priceLists = _pricingSearchService.SearchPricelists(new PricelistSearchCriteria { Take = int.MaxValue }).Results;
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
        [ResponseType(typeof(Pricelist))]
        [Route("api/pricing/pricelists/{id}")]
        public IHttpActionResult GetPriceListById(string id)
        {
            var pricelist = _pricingService.GetPricelistsById(new[] { id }).FirstOrDefault();           
            return Ok(pricelist);
        }


        /// <summary>
        /// Create pricelist
        /// </summary>
        [HttpPost]
        [ResponseType(typeof(Pricelist))]
        [Route("api/pricing/pricelists")]
        [CheckPermission(Permission = PricingPredefinedPermissions.Create)]
        public IHttpActionResult CreatePriceList(Pricelist priceList)
        {         
            _pricingService.SavePricelists(new[] { priceList });
            return Ok(priceList);
        }

        /// <summary>
        /// Update pricelist
        /// </summary>
        [HttpPut]
        [ResponseType(typeof(void))]
        [Route("api/pricing/pricelists")]
        [CheckPermission(Permission = PricingPredefinedPermissions.Update)]
        public IHttpActionResult UpdatePriceList(Pricelist priceList)
        {
            _pricingService.SavePricelists(new[] { priceList });
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
            var result = _pricingSearchService.SearchPrices(new PricesSearchCriteria { PriceListId = pricelistId, ProductIds = productIds, Take = int.MaxValue });
            _pricingService.DeletePrices(result.Results.Select(x => x.Id).ToArray());
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Delete price by ids
        /// </summary>
        /// <param name="priceIds"></param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseType(typeof(void))]
        [Route("api/pricing/products/prices")]
        [CheckPermission(Permission = PricingPredefinedPermissions.Update)]
        public IHttpActionResult DeleteProductPrice([FromUri]string[] priceIds)
        {
            _pricingService.DeletePrices(priceIds);
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
