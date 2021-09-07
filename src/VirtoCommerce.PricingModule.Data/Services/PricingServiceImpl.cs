using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;
namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricingServiceImpl : IPricingService
    {
        private readonly IPriceService _priceService;
        private readonly IPricelistService _pricelistService;
        private readonly IPricelistAssignmentService _pricelistAssignmentService;

        public PricingServiceImpl(
            IPriceService priceService,
            IPricelistAssignmentService pricelistAssignmentService,
            IPricelistService pricelistService)
        {
            _priceService = priceService;
            _pricelistService = pricelistService;
            _pricelistAssignmentService = pricelistAssignmentService;
        }

        public virtual Task<IEnumerable<Pricelist>> EvaluatePriceListsAsync(PriceEvaluationContext evalContext)
        {
            return _pricelistAssignmentService.EvaluatePriceListsAsync(evalContext);
        }

        public virtual Task<IEnumerable<Price>> EvaluateProductPricesAsync(PriceEvaluationContext evalContext)
        {
            return _priceService.EvaluateProductPricesAsync(evalContext);
        }

        public Task<IEnumerable<Price>> GetPricesByIdAsync(IEnumerable<string> ids)
        {
            return ((ICrudService<Price>)_priceService).GetByIdsAsync(ids);
        }

        public Task<IEnumerable<Pricelist>> GetPricelistsByIdAsync(IEnumerable<string> ids)
        {
            return ((ICrudService<Pricelist>)_pricelistService).GetByIdsAsync(ids);
        }

        public Task<IEnumerable<PricelistAssignment>> GetPricelistAssignmentsByIdAsync(IEnumerable<string> ids)
        {
            return ((ICrudService<PricelistAssignment>)_pricelistAssignmentService).GetByIdsAsync(ids);
        }

        public Task SavePricesAsync(IEnumerable<Price> prices)
        {
            return ((ICrudService<Price>)_priceService).SaveChangesAsync(prices);
        }

        public Task SavePricelistsAsync(IEnumerable<Pricelist> priceLists)
        {
            return ((ICrudService<Pricelist>)_pricelistService).SaveChangesAsync(priceLists);
        }

        public Task SavePricelistAssignmentsAsync(IEnumerable<PricelistAssignment> assignments)
        {
            return ((ICrudService<PricelistAssignment>)_pricelistAssignmentService).SaveChangesAsync(assignments);
        }

        public Task DeletePricelistsAsync(IEnumerable<string> ids)
        {
            return ((ICrudService<Pricelist>)_pricelistService).DeleteAsync(ids);
        }

        public Task DeletePricesAsync(IEnumerable<string> ids)
        {
            return ((ICrudService<Price>)_priceService).DeleteAsync(ids);
        }

        public Task DeletePricelistsAssignmentsAsync(IEnumerable<string> ids)
        {
            return ((ICrudService<PricelistAssignment>)_pricelistAssignmentService).DeleteAsync(ids);
        }
    }
}
