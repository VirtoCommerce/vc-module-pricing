using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Pricing.Services;
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


        public PricingModuleController(IPricingService pricingService, IItemService itemService, ICatalogService catalogService, IPricingExtensionManager extensionManager, IExpressionSerializer expressionSerializer, IPricingSearchService pricingSearchService)
        {
            _extensionManager = extensionManager;
            _expressionSerializer = expressionSerializer;
            _pricingService = pricingService;
            _itemService = itemService;
            _catalogService = catalogService;
            _pricingSearchService = pricingSearchService;
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
        /// <response code="200"></response>
        /// <response code="404">Pricelist assignment not found.</response>
        [HttpGet]
        [ResponseType(typeof(webModel.PricelistAssignment))]
        [Route("api/pricing/assignments/{id}")]
        public IHttpActionResult GetPricelistAssignmentById(string id)
        {
            var assignment = _pricingService.GetPricelistAssignmentById(id);
            var result = assignment != null
                ? assignment.ToWebModel(_extensionManager.ConditionExpressionTree)
                : null;

            if (result != null)
            {
                result.Catalog = _catalogService.GetById(result.CatalogId);               
            }
            return result != null ? Ok(result) : (IHttpActionResult)NotFound();
        }

        /// <summary>
        /// Get pricelist assignments
        /// </summary>
        /// <remarks>Get array of all pricelist assignments for all catalogs.</remarks>
        /// <todo>Do we need return for all catalogs?</todo>
        [HttpGet]
        [ResponseType(typeof(webModel.PricelistAssignment[]))]
        [Route("api/pricing/assignments")]
        public IHttpActionResult GetPricelistAssignments()
        {
            var assignments = _pricingService.GetPriceListAssignments();
            var result = assignments.Select(x => x.ToWebModel()).ToArray();
            if (!result.IsNullOrEmpty())
            {
                var catalogs = _catalogService.GetCatalogsList();
                foreach(var assignment in result)
                {
                    assignment.Catalog = catalogs.FirstOrDefault(x => x.Id == assignment.CatalogId);
                }
            }

            return Ok(result);
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
        /// <response code="204">Operation completed.</response>
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

        /// <summary>
        /// Delete pricelist assignments
        /// </summary>
        /// <remarks>Delete pricelist assignment by given array of ids.</remarks>
        /// <param name="ids">An array of pricelist assignment ids</param>
        /// <response code="204">Operation completed.</response>
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
        /// Get pricelists
        /// </summary>
        /// <remarks>Get all pricelists for all catalogs.</remarks>
        [HttpGet]
        [ResponseType(typeof(webModel.Pricelist[]))]
        [Route("api/pricing/pricelists")]
        public IHttpActionResult GetPriceLists()
        {
            var priceLists = _pricingService.GetPriceLists();
            var result = priceLists.Select(x => x.ToWebModel()).ToArray();
            return Ok(result);
        }

        /// <summary>
        /// Get array of product prices
        /// </summary>
        /// <remarks>Get an array of valid product prices for each currency.</remarks>
        /// <param name="productId">Product id</param>
        /// <response code="200"></response>
        /// <response code="404">Prices not found.</response>
        [HttpGet]
        [ResponseType(typeof(webModel.Price[]))]
        [Route("api/products/{productId}/prices")]
        public IHttpActionResult GetProductPrices(string productId)
        {
            var result = _pricingSearchService.Search(new Domain.Pricing.Model.Search.SearchCriteria { Take = int.MaxValue, ProductId = productId });
            if (result != null)
            {
                var retVal = result.Prices.GroupBy(x => x.Currency)
                    .Select(x => x.First().ToWebModel())
                    .ToArray();

                return Ok(result);
            }

            return Ok();
        }

        /// <summary>
        /// Search product prices 
        /// </summary>
        /// <remarks>Search product prices</remarks>
        [HttpGet]
        [ResponseType(typeof(webModel.ProductPricesSearchResult))]
        [Route("api/catalog/products/prices/search")]
        public IHttpActionResult SearchProductPrices([FromUri]coreModel.Search.SearchCriteria criteria)
        {
            var result = _pricingSearchService.Search(criteria);
            var retVal = new webModel.ProductPricesSearchResult
            {
                TotalCount = result.TotalCount,
                ProductPrices = result.Prices.GroupBy(x => x.ProductId).Select(x => new webModel.ProductPrice { ProductId = x.Key, Prices = x.Select(y => y.ToWebModel()).ToList() }).ToList()
            };

            if(retVal.ProductPrices != null)
            {
                var products = _itemService.GetByIds(retVal.ProductPrices.Select(x => x.ProductId).ToArray(), Domain.Catalog.Model.ItemResponseGroup.ItemInfo);
                foreach(var productPrice in retVal.ProductPrices)
                {
                    productPrice.Product = products.FirstOrDefault(x => x.Id == productPrice.ProductId);
                }
            }
            return Ok(retVal);
        }

        [HttpPut]
        [ResponseType(typeof(void))]
        [Route("api/products/{productId}/prices")]
        [CheckPermission(Permission = PricingPredefinedPermissions.Update)]
        public IHttpActionResult UpdateProductPrices(webModel.ProductPrice productPrice)
        {
            var result = _pricingSearchService.Search(new Domain.Pricing.Model.Search.SearchCriteria { Take = int.MaxValue, ProductId = productPrice.ProductId });
            var targetPrices = result.Prices;
            var sourcePrices = productPrice.Prices.Select(x => x.ToCoreModel()).ToList();

            var changedPrices = new List<coreModel.Price>();
            var deletedPrices = new List<coreModel.Price>();

            sourcePrices.CompareTo(targetPrices, EqualityComparer<coreModel.Price>.Default, (state, x, y) =>
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
            _pricingService.SavePrices(changedPrices.ToArray());
            if(!deletedPrices.IsNullOrEmpty())
            {
                _pricingService.DeletePrices(changedPrices.Select(x => x.Id).ToArray());
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Get pricelist
        /// </summary>
        /// <param name="id">Pricelist id</param>
        /// <response code="200"></response>
        /// <response code="404">Pricelist not found.</response>
        [HttpGet]
        [ResponseType(typeof(webModel.Pricelist))]
        [Route("api/pricing/pricelists/{id}")]
        public IHttpActionResult GetPriceListById(string id)
        {
            webModel.Pricelist result = null;
            var pricelist = _pricingService.GetPricelistById(id);
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
            var result = corePriceList.ToWebModel();
            return Ok(result);
        }

        /// <summary>
        /// Update pricelist
        /// </summary>
        /// <response code="204">Operation completed.</response>
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
        /// Delete pricelists  
        /// </summary>
        /// <remarks>Delete pricelists by given array of pricelist ids.</remarks>
        /// <param name="ids">An array of pricelist ids</param>
        /// <response code="204">Operation completed.</response>
        [HttpDelete]
        [ResponseType(typeof(void))]
        [Route("api/pricing/pricelists")]
        [CheckPermission(Permission = PricingPredefinedPermissions.Delete)]
        public IHttpActionResult DeletePriceLists([FromUri] string[] ids)
        {
            _pricingService.DeletePricelists(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }        
    }
}
