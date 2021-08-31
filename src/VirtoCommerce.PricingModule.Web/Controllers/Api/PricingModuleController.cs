using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PricingModule.Core;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Conditions;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PricingModule.Web.Controllers.Api
{
    [Route("")]
    [Authorize(ModuleConstants.Security.Permissions.Read)]
    public class PricingModuleController : Controller
    {
        private readonly IItemService _itemService;
        private readonly IBlobUrlResolver _blobUrlResolver;
        private readonly IPriceService _priceService;
        private readonly IPricelistService _pricelistService;
        private readonly IPricelistAssignmentService _pricelistAssignmentService;
        private readonly ISearchService<PricesSearchCriteria, PriceSearchResult, Price> _priceSearchService;
        private readonly ISearchService<PricelistSearchCriteria, PricelistSearchResult, Pricelist> _pricelistSearchService;
        private readonly ISearchService<PricelistAssignmentsSearchCriteria, PricelistAssignmentSearchResult, PricelistAssignment> _pricelistAssignmentSearchService;

        public PricingModuleController(
            IItemService itemService
            , IBlobUrlResolver blobUrlResolver
            , IPriceService priceService
            , IPriceSearchService priceSearchService
            , IPricelistService pricelistService
            , IPricelistSearchService pricelistSearchService
            , IPricelistAssignmentService pricelistAssignmentService
            , IPricelistAssignmentSearchService pricelistAssignmentSearchService)
        {
            _itemService = itemService;
            _blobUrlResolver = blobUrlResolver;
            _priceSearchService = (ISearchService<PricesSearchCriteria, PriceSearchResult, Price>)priceSearchService;
            _pricelistSearchService = (ISearchService<PricelistSearchCriteria, PricelistSearchResult, Pricelist>)pricelistSearchService;
            _pricelistAssignmentSearchService = (ISearchService<PricelistAssignmentsSearchCriteria, PricelistAssignmentSearchResult, PricelistAssignment>)pricelistAssignmentSearchService;
            _priceService = priceService;
            _pricelistService = pricelistService;
            _pricelistAssignmentService = pricelistAssignmentService;
        }
        
        /// <summary>
        /// Evaluate prices by given context
        /// </summary>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>Prices array</returns>
        [HttpPost]
        [Route("api/pricing/evaluate")]
        public async Task<ActionResult<Price[]>> EvaluatePrices([FromBody] PriceEvaluationContext evalContext)
        {
            var retVal = (await _priceService.EvaluateProductPricesAsync(evalContext)).ToArray();

            return Ok(retVal);
        }


        /// <summary>
        /// Evaluate pricelists by given context
        /// </summary>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>Pricelist array</returns>
        [HttpPost]
        [Route("api/pricing/pricelists/evaluate")]
        public async Task<ActionResult<Pricelist[]>> EvaluatePriceLists([FromBody] PriceEvaluationContext evalContext)
        {
            var retVal = (await _pricelistAssignmentService.EvaluatePriceListsAsync(evalContext)).ToArray();
            return Ok(retVal);
        }
        /// <summary>
        /// Get pricelist assignment
        /// </summary>
        /// <param name="id">Pricelist assignment id</param>
        [HttpGet]
        [Route("api/pricing/assignments/{id}")]
        public async Task<ActionResult<PricelistAssignment>> GetPricelistAssignmentById(string id)
        {
            var assignment = (await ((ICrudService<PricelistAssignment>)_pricelistAssignmentService).GetByIdsAsync(new[] { id })).FirstOrDefault();
            if (assignment != null)
            {
                assignment.DynamicExpression?.MergeFromPrototype(AbstractTypeFactory<PriceConditionTreePrototype>.TryCreateInstance());
            }
            return Ok(assignment);
        }

        /// <summary>
        /// Get a new pricelist assignment
        /// </summary>
        /// <remarks>Get a new pricelist assignment object. Create new pricelist assignment, but does not save one.</remarks>
        [HttpGet]
        [Route("api/pricing/assignments/new")]
        public ActionResult<PricelistAssignment> GetNewPricelistAssignments()
        {
            var result = AbstractTypeFactory<PricelistAssignment>.TryCreateInstance();
            result.Priority = 1;
            //Required for UI
            result.DynamicExpression = AbstractTypeFactory<PriceConditionTree>.TryCreateInstance();
            result.DynamicExpression.MergeFromPrototype(AbstractTypeFactory<PriceConditionTreePrototype>.TryCreateInstance());
            return Ok(result);
        }

        /// <summary>
        /// Get pricelists
        /// </summary>
        /// <remarks>Get all pricelists for all catalogs.</remarks>
        [HttpGet]
        [Route("api/pricing/pricelists")]
        public async Task<ActionResult<PricelistSearchResult>> SearchPricelists(PricelistSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new PricelistSearchCriteria();
            }
            var result = await _pricelistSearchService.SearchAsync(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Search pricelist assignments
        /// </summary>
        /// <remarks>Search price list assignments by given criteria</remarks>
        [HttpGet]
        [Route("api/pricing/assignments")]
        public async Task<ActionResult<PricelistAssignmentSearchResult>> SearchPricelistAssignments(PricelistAssignmentsSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new PricelistAssignmentsSearchCriteria();
            }
            var result = await _pricelistAssignmentSearchService.SearchAsync(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Search product prices 
        /// </summary>
        /// <remarks>Search product prices</remarks>
        [HttpGet]
        [Route("api/catalog/products/prices/search")]
        public async Task<ActionResult<ProductPriceSearchResult>> SearchProductPricesGet([FromQuery] PricesSearchCriteria criteria)
        {
            return Ok(await InnerSearchProductPrices(criteria));
        }

        /// <summary>
        /// Search product prices 
        /// </summary>
        /// <remarks>Search product prices</remarks>
        [HttpPost]
        [Route("api/catalog/products/prices/search")]
        public async Task<ActionResult<ProductPriceSearchResult>> SearchProductPricesPost([FromBody] PricesSearchCriteria criteria)
        {
            return Ok(await InnerSearchProductPrices(criteria));
        }

        private async Task<ProductPriceSearchResult> InnerSearchProductPrices(PricesSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new PricesSearchCriteria();
            }

            var result = AbstractTypeFactory<ProductPriceSearchResult>.TryCreateInstance();
            var searchResult = await _priceSearchService.SearchAsync(criteria);
            result.TotalCount = searchResult.TotalCount;
            result.Results = new List<ProductPrice>();

            var products = await _itemService.GetByIdsAsync(searchResult.Results.Select(x => x.ProductId).Distinct().ToArray(), ItemResponseGroup.ItemInfo.ToString());
            foreach (var productPricesGroup in searchResult.Results.GroupBy(x => x.ProductId))
            {
                var productPrice = new ProductPrice
                {
                    ProductId = productPricesGroup.Key,
                    Prices = productPricesGroup.ToList()
                };
                var product = products.FirstOrDefault(x => x.Id == productPricesGroup.Key);
                if (product != null)
                {
                    if (!product.Images.IsNullOrEmpty())
                    {
                        foreach (var image in product.Images)
                        {
                            image.RelativeUrl = image.Url;
                            image.Url = _blobUrlResolver.GetAbsoluteUrl(image.Url);
                        }
                    }

                    productPrice.Product = product;
                }
                result.Results.Add(productPrice);
            }

            return result;
        }

        /// <summary>
        /// Evaluate  product prices
        /// </summary>
        /// <remarks>Get an array of valid product prices for each currency.</remarks>
        /// <param name="productId">Product id</param>
        [HttpGet]
        [Route("api/products/{productId}/prices")]
        public async Task<ActionResult<Price[]>> EvaluateProductPrices(string productId)
        {
            var priceEvalContext = AbstractTypeFactory<PriceEvaluationContext>.TryCreateInstance();
            priceEvalContext.ProductIds = new[] { productId };

            var product = (await _itemService.GetByIdsAsync(new[] { productId }, ItemResponseGroup.ItemInfo.ToString())).FirstOrDefault();
            if (product != null)
            {
                priceEvalContext.CatalogId = product.CatalogId;
            }
            return await EvaluatePrices(priceEvalContext);
        }

        /// <summary>
        /// Evaluate product prices for demand catalog
        /// </summary>
        /// <remarks>Get an array of valid product prices for each currency.</remarks>
        /// <param name="productId">Product id</param>
        /// <param name="catalogId">Catalog id</param>
        [HttpGet]
        [Route("api/products/{productId}/{catalogId}/pricesWidget")]
        public async Task<ActionResult<Price[]>> EvaluateProductPricesForCatalog(string productId, string catalogId)
        {
            var priceEvalContext = AbstractTypeFactory<PriceEvaluationContext>.TryCreateInstance();
            priceEvalContext.ProductIds = new[] { productId };
            priceEvalContext.CatalogId = catalogId;

            return await EvaluatePrices(priceEvalContext);
        }

        /// <summary>
        /// Create pricelist assignment
        /// </summary>
        /// <param name="assignment">PricelistAssignment</param>
        [HttpPost]
        [Route("api/pricing/assignments")]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public async Task<ActionResult<PricelistAssignment>> CreatePricelistAssignment([FromBody] PricelistAssignment assignment)
        {
            await ((ICrudService<PricelistAssignment>)_pricelistAssignmentService).SaveChangesAsync(new[] { assignment });
            return Ok(assignment);
        }

        /// <summary>
        /// Update pricelist assignment
        /// </summary>
        /// <param name="assignment">PricelistAssignment</param>
        /// <todo>Return no any reason if can't update</todo>
        [HttpPut]
        [Route("api/pricing/assignments")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdatePriceListAssignment([FromBody] PricelistAssignment assignment)
        {
            await ((ICrudService<PricelistAssignment>)_pricelistAssignmentService).SaveChangesAsync(new[] { assignment });
            return NoContent();
        }

        [HttpPut]
        [Route("api/products/prices")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateProductsPrices([FromBody] ProductPrice[] productPrices)
        {
            var result = await _priceSearchService.SearchAsync(new PricesSearchCriteria
            {
                Take = int.MaxValue,
                ProductIds = productPrices.Select(x => x.ProductId).ToArray()
            });
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
            await ((ICrudService<Price>)_priceService).SaveChangesAsync(changedPrices.ToArray());
            if (!deletedPrices.IsNullOrEmpty())
            {
                await ((ICrudService<Price>)_priceService).DeleteAsync(deletedPrices.Select(x => x.Id).ToArray());
            }
            return NoContent();
        }

        [HttpPut]
        [Route("api/products/{productId}/prices")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult> UpdateProductPrices([FromBody] ProductPrice productPrice)
        {
            return await UpdateProductsPrices(new[] { productPrice });
        }

        /// <summary>
        /// Get all price lists for product
        /// </summary>
        /// <remarks>Get all price lists for given product.</remarks>
        /// <param name="productId">Product id</param>
        [HttpGet]
        [Route("api/catalog/products/{productId}/pricelists")]
        public async Task<ActionResult<Pricelist[]>> GetProductPriceLists(string productId)
        {
            var productPrices = (await _priceSearchService.SearchAsync(new PricesSearchCriteria { Take = int.MaxValue, ProductId = productId })).Results;
            var priceLists = (await _pricelistSearchService.SearchAsync(new PricelistSearchCriteria { Take = int.MaxValue })).Results;
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
        [Route("api/pricing/pricelists/{id}")]
        public async Task<ActionResult<Pricelist>> GetPriceListById(string id)
        {
            var pricelist = (await ((ICrudService<Pricelist>)_pricelistService).GetByIdsAsync(new[] { id })).FirstOrDefault();
            return Ok(pricelist);
        }


        /// <summary>
        /// Create pricelist
        /// </summary>
        [HttpPost]
        [Route("api/pricing/pricelists")]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public async Task<ActionResult<Pricelist>> CreatePriceList([FromBody] Pricelist priceList)
        {
            await ((ICrudService<Pricelist>)_pricelistService).SaveChangesAsync(new[] { priceList });
            return Ok(priceList);
        }

        /// <summary>
        /// Update pricelist
        /// </summary>
        [HttpPut]
        [Route("api/pricing/pricelists")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdatePriceList([FromBody] Pricelist priceList)
        {
            await ((ICrudService<Pricelist>)_pricelistService).SaveChangesAsync(new[] { priceList });
            return NoContent();
        }

        /// <summary>
        /// Delete pricelist assignments
        /// </summary>
        /// <remarks>Delete pricelist assignment by given array of ids.</remarks>
        /// <param name="ids">An array of pricelist assignment ids</param>
        /// <todo>Return no any reason if can't update</todo>
        [HttpDelete]
        [Route("api/pricing/assignments")]
        [Authorize(ModuleConstants.Security.Permissions.Delete)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAssignments(string[] ids)
        {
            await ((ICrudService<PricelistAssignment>)_pricelistAssignmentService).DeleteAsync(ids);
            return NoContent();
        }

        /// <summary>
        /// Delete pricelist assignments
        /// </summary>
        /// <remarks>Delete pricelist assignments by given criteria.</remarks>
        /// <param name="criteria">Filter criteria</param>
        /// <todo>Return no any reason if can't update</todo>
        [HttpDelete]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [Route("api/pricing/filteredAssignments")]
        [Authorize(ModuleConstants.Security.Permissions.Delete)]
        public async Task<ActionResult> DeleteFilteredAssignments([FromQuery] PricelistAssignmentsSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new PricelistAssignmentsSearchCriteria();
            }

            var result = await _pricelistAssignmentSearchService.SearchAsync(criteria);

            var pricelistAssignmentsIds = result.Results.Select(x => x.Id);

            const int BATCH_SIZE = 20;
            for (var skip = 0; skip < pricelistAssignmentsIds.Count(); skip += BATCH_SIZE)
            {
                var batch = pricelistAssignmentsIds.Skip(skip).Take(BATCH_SIZE);
                await ((ICrudService<PricelistAssignment>)_pricelistAssignmentService).DeleteAsync(batch.ToArray());
            }

            return NoContent();
        }

        /// <summary>
        /// Delete all prices for specified product in specified price list
        /// </summary>
        /// <param name="pricelistId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("api/pricing/pricelists/{pricelistId}/products/prices")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteProductPrices(string pricelistId, string[] productIds)
        {
            var result = await _priceSearchService.SearchAsync(new PricesSearchCriteria { PriceListId = pricelistId, ProductIds = productIds, Take = int.MaxValue });
            await ((ICrudService<Price>)_priceService).DeleteAsync(result.Results.Select(x => x.Id).ToArray());
            return NoContent();
        }

        /// <summary>
        /// Delete price by ids
        /// </summary>
        /// <param name="priceIds"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("api/pricing/products/prices")]
        [Authorize(ModuleConstants.Security.Permissions.Delete)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteProductPrice(string[] priceIds)
        {
            await ((ICrudService<Price>)_priceService).DeleteAsync(priceIds);
            return NoContent();
        }

        /// <summary>
        /// Delete pricelists  
        /// </summary>
        /// <remarks>Delete pricelists by given array of pricelist ids.</remarks>
        /// <param name="ids">An array of pricelist ids</param>
        [HttpDelete]
        [Route("api/pricing/pricelists")]
        [Authorize(ModuleConstants.Security.Permissions.Delete)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeletePricelists(string[] ids)
        {
            await ((ICrudService<Pricelist>)_pricelistService).DeleteAsync(ids);
            return NoContent();
        }
    }
}
