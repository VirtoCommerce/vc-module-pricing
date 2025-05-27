using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Core;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Conditions;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PricingModule.Web.Controllers.Api
{
    [Route("")]
    [Authorize(ModuleConstants.Security.Permissions.Read)]
    public class PricingModuleController(
        IPriceSearchService priceSearchService,
        IPriceService priceService,
        IPricelistSearchService pricelistSearchService,
        IPricelistService pricelistService,
        IPricelistAssignmentSearchService pricelistAssignmentSearchService,
        IPricelistAssignmentService pricelistAssignmentService,
        IPricingEvaluatorService pricingEvaluatorService,
        IItemService itemService,
        IMergedPriceSearchService mergedPriceSearchService,
        IBlobUrlResolver blobUrlResolver,
        AbstractValidator<Pricelist> priceListValidator
        ) : Controller
    {
        /// <summary>
        /// Evaluate prices by given context
        /// </summary>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>Prices array</returns>
        [HttpPost]
        [Route("api/pricing/evaluate")]
        public async Task<ActionResult<IList<Price>>> EvaluatePrices([FromBody] PriceEvaluationContext evalContext)
        {
            var result = await pricingEvaluatorService.EvaluateProductPricesAsync(evalContext);

            return Ok(result);
        }

        /// <summary>
        /// Evaluate pricelists by given context
        /// </summary>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>Pricelist array</returns>
        [HttpPost]
        [Route("api/pricing/pricelists/evaluate")]
        public async Task<ActionResult<IList<Pricelist>>> EvaluatePriceLists([FromBody] PriceEvaluationContext evalContext)
        {
            var result = await pricingEvaluatorService.EvaluatePriceListsAsync(evalContext);
            return Ok(result);
        }

        /// <summary>
        /// Get pricelist assignment
        /// </summary>
        /// <param name="id">Pricelist assignment id</param>
        [HttpGet]
        [Route("api/pricing/assignments/{id}")]
        public async Task<ActionResult<PricelistAssignment>> GetPricelistAssignmentById(string id)
        {
            var assignment = await pricelistAssignmentService.GetByIdAsync(id);
            if (assignment != null)
            {
                assignment.DynamicExpression?.MergeFromPrototype(AbstractTypeFactory<PriceConditionTreePrototype>.TryCreateInstance());
            }
            return Ok(assignment);
        }

        /// <summary>
        /// Get pricelist assignment by outer id
        /// </summary>
        /// <param name="outerId">Pricelist assignment outer id</param>
        [HttpGet]
        [Route("api/pricing/assignments/outer/{outerId}")]
        public async Task<ActionResult<PricelistAssignment>> GetPricelistAssignmentByOuterId(string outerId)
        {
            var assignment = await pricelistAssignmentService.GetByOuterIdAsync(outerId);
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
            var result = await pricelistSearchService.SearchNoCloneAsync(criteria);
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
            var result = await pricelistAssignmentSearchService.SearchNoCloneAsync(criteria);
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

        /// <summary>
        /// Evaluate  product prices
        /// </summary>
        /// <remarks>Get an array of valid product prices for each currency.</remarks>
        /// <param name="productId">Product id</param>
        [HttpGet]
        [Route("api/products/{productId}/prices")]
        public async Task<ActionResult<IList<Price>>> EvaluateProductPrices(string productId)
        {
            var priceEvalContext = AbstractTypeFactory<PriceEvaluationContext>.TryCreateInstance();
            priceEvalContext.ProductIds = [productId];

            var product = await itemService.GetNoCloneAsync(productId, nameof(ItemResponseGroup.ItemInfo));
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
        public Task<ActionResult<IList<Price>>> EvaluateProductPricesForCatalog(string productId, string catalogId)
        {
            var priceEvalContext = AbstractTypeFactory<PriceEvaluationContext>.TryCreateInstance();
            priceEvalContext.ProductIds = [productId];
            priceEvalContext.CatalogId = catalogId;

            return EvaluatePrices(priceEvalContext);
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
            await pricelistAssignmentService.SaveChangesAsync([assignment]);
            return Ok(assignment);
        }

        /// <summary>
        /// Update pricelist assignment
        /// </summary>
        /// <param name="assignment">PricelistAssignment</param>
        [HttpPut]
        [Route("api/pricing/assignments")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdatePriceListAssignment([FromBody] PricelistAssignment assignment)
        {
            await pricelistAssignmentService.SaveChangesAsync([assignment]);
            return NoContent();
        }

        /// <summary>
        /// Update product prices
        /// </summary>
        /// <param name="productPrices">List of ProductPrice</param>
        [HttpPut]
        [Route("api/products/prices")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateProductsPrices([FromBody] ProductPrice[] productPrices)
        {
            var result = await priceSearchService.SearchAsync(new PricesSearchCriteria
            {
                ProductIds = productPrices.Select(x => x.ProductId).ToList(),
                Take = int.MaxValue,
            });
            var targetPricesGroups = result.Results.GroupBy(x => x.PricelistId).ToDictionary(g => g.Key);
            var sourcePricesGroups = productPrices.SelectMany(x => x.Prices).GroupBy(x => x.PricelistId);

            var changedPrices = new List<Price>();
            var deletedPrices = new List<Price>();

            foreach (var sourcePricesGroup in sourcePricesGroups)
            {
                if (targetPricesGroups.TryGetValue(sourcePricesGroup.Key, out var targetPricesGroup))
                {
                    sourcePricesGroup.ToArray().CompareTo(targetPricesGroup.ToArray(), EqualityComparer<Price>.Default, (state, x, _) =>
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
            await priceService.SaveChangesAsync(changedPrices);
            if (!deletedPrices.IsNullOrEmpty())
            {
                await priceService.DeleteAsync(deletedPrices.Select(x => x.Id).ToList());
            }
            return NoContent();
        }

        /// <summary>
        /// Update product prices
        /// </summary>
        /// <param name="productPrice">ProductPrice</param>
        [HttpPut]
        [Route("api/products/{productId}/prices")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public Task<ActionResult> UpdateProductPrices([FromBody] ProductPrice productPrice)
        {
            return UpdateProductsPrices([productPrice]);
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
            var productPrices = (await priceSearchService.SearchNoCloneAsync(new PricesSearchCriteria { Take = int.MaxValue, ProductId = productId })).Results;
            var priceLists = (await pricelistSearchService.SearchAsync(new PricelistSearchCriteria { Take = int.MaxValue })).Results;
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
            var pricelist = await pricelistService.GetNoCloneAsync(id);
            return Ok(pricelist);
        }

        /// <summary>
        /// Get pricelist by outer id
        /// </summary>
        /// <param name="outerId">Pricelist outer id</param>
        [HttpGet]
        [Route("api/pricing/pricelists/outer/{outerId}")]
        public async Task<ActionResult<Pricelist>> GetPricelistByOuterId(string outerId)
        {
            var pricelist = await pricelistService.GetByOuterIdNoCloneAsync(outerId);
            return Ok(pricelist);
        }

        /// <summary>
        /// Get pricelist in short mode (without assignments to avoid redundant assignments read)
        /// </summary>
        /// <param name="id">Pricelist id</param>
        [HttpGet]
        [Route("api/pricing/pricelistsshort/{id}")]
        public async Task<ActionResult<Pricelist>> GetPriceListInShortById(string id)
        {
            var pricelist = await pricelistService.GetNoCloneAsync(id, nameof(PriceListResponseGroup.NoDetails));
            return Ok(pricelist);
        }

        /// <summary>
        /// Get pricelist by outer id in short mode (without assignments)
        /// </summary>
        /// <param name="outerId">Pricelist outer id</param>
        [HttpGet]
        [Route("api/pricing/pricelistsshort/outer/{outerId}")]
        public async Task<ActionResult<Pricelist>> GetPricelistInShortByOuterId(string outerId)
        {
            var pricelist = await pricelistService.GetByOuterIdNoCloneAsync(outerId, nameof(PriceListResponseGroup.NoDetails));
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
            var validationResult = await priceListValidator.ValidateAsync(priceList);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(x => x.ErrorMessage));
            }

            await pricelistService.SaveChangesAsync([priceList]);
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
            await pricelistService.SaveChangesAsync([priceList]);
            return NoContent();
        }

        /// <summary>
        /// Delete pricelist assignments
        /// </summary>
        /// <remarks>Delete pricelist assignment by given array of ids.</remarks>
        /// <param name="ids">An array of pricelist assignment ids</param>
        [HttpDelete]
        [Route("api/pricing/assignments")]
        [Authorize(ModuleConstants.Security.Permissions.Delete)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAssignments(string[] ids)
        {
            await pricelistAssignmentService.DeleteAsync(ids);
            return NoContent();
        }

        /// <summary>
        /// Delete pricelist assignments
        /// </summary>
        /// <remarks>Delete pricelist assignments by given criteria.</remarks>
        /// <param name="criteria">Filter criteria</param>
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

            var result = await pricelistAssignmentSearchService.SearchNoCloneAsync(criteria);

            var pricelistAssignmentsIds = result.Results.Select(x => x.Id).ToList();

            const int batchSize = 20;
            for (var skip = 0; skip < pricelistAssignmentsIds.Count; skip += batchSize)
            {
                var idsBatch = pricelistAssignmentsIds.Skip(skip).Take(batchSize).ToList();
                await pricelistAssignmentService.DeleteAsync(idsBatch);
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
            var searchCriteria = new PricesSearchCriteria
            {
                PriceListId = pricelistId,
                ProductIds = productIds,
                Take = int.MaxValue,
            };

            var searchResult = await priceSearchService.SearchNoCloneAsync(searchCriteria);
            await priceService.DeleteAsync(searchResult.Results.Select(x => x.Id).ToList());

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
            await priceService.DeleteAsync(priceIds);
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
            await pricelistService.DeleteAsync(ids);
            return NoContent();
        }

        /// <summary>
        /// Merge base and priority price list and returns MergedPriceGroup.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/pricing/mergedpricegroups")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<MergedPriceGroupSearchResult>> SearchGroups([FromBody] MergedPriceSearchCriteria criteria)
        {
            var result = await mergedPriceSearchService.SearchGroupsAsync(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Merge base and priority price list and returns MergedPrice.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/pricing/mergedprices")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<MergedPriceSearchResult>> SearchGroupPrices([FromBody] MergedPriceSearchCriteria criteria)
        {
            var result = await mergedPriceSearchService.SearchGroupPricesAsync(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Partial update for the specified ProductPrice by id
        /// </summary>
        /// <param name="id">ProductPrice id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        [HttpPatch]
        [Route("api/products/prices/{id}")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> PatchProductPrice(string id, [FromBody] JsonPatchDocument<Price> patchDocument)
        {
            var price = await priceService.GetByIdAsync(id);
            if (price == null)
            {
                return NotFound();
            }

            patchDocument.ApplyTo(price, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var productPrice = AbstractTypeFactory<ProductPrice>.TryCreateInstance();
            productPrice.Prices = [price];

            await UpdateProductPrices(productPrice);

            return NoContent();
        }

        /// <summary>
        /// Partial update for the specified Pricelist by id
        /// </summary>
        /// <param name="id">Pricelist id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        [HttpPatch]
        [Route("api/pricing/pricelists/{id}")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> PatchPriceList(string id, [FromBody] JsonPatchDocument<Pricelist> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var priceList = await pricelistService.GetByIdAsync(id);
            if (priceList == null)
            {
                return NotFound();
            }

            patchDocument.ApplyTo(priceList, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await pricelistService.SaveChangesAsync([priceList]);

            return NoContent();
        }

        /// <summary>
        /// Partial update for the specified pricelist assignment by id
        /// </summary>
        /// <param name="id">PricelistAssignment id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        [HttpPatch]
        [Route("api/pricing/assignments/{id}")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> PatchPriceListAssignment(string id, [FromBody] JsonPatchDocument<PricelistAssignment> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var pricelistAssignment = await pricelistAssignmentService.GetByIdAsync(id);
            if (pricelistAssignment == null)
            {
                return NotFound();
            }

            patchDocument.ApplyTo(pricelistAssignment, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await pricelistAssignmentService.SaveChangesAsync([pricelistAssignment]);

            return NoContent();
        }

        private async Task<ProductPriceSearchResult> InnerSearchProductPrices(PricesSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new PricesSearchCriteria();
            }

            var result = AbstractTypeFactory<ProductPriceSearchResult>.TryCreateInstance();
            var searchResult = await priceSearchService.SearchNoCloneAsync(criteria);
            result.TotalCount = searchResult.TotalCount;
            result.Results = new List<ProductPrice>();

            var productIds = searchResult.Results.Select(x => x.ProductId).Distinct().ToList();
            var products = await itemService.GetNoCloneAsync(productIds, ItemResponseGroup.ItemInfo.ToString());
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
                            image.Url = blobUrlResolver.GetAbsoluteUrl(image.Url);
                        }
                    }

                    productPrice.Product = product;
                }
                result.Results.Add(productPrice);
            }

            return result;
        }
    }
}
