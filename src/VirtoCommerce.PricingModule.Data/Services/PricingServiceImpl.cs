using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.Services
{
    [Obsolete("Implementation was decoupled to separate implementations of interfaces IPricingEvaluatorService, ICrudService<PricelistAssignment>, ICrudService<Pricelist>, ICrudService<Price>")]
    public class PricingServiceImpl : IPricingService
    {
        private readonly ICrudService<PricelistAssignment> _pricelistAssignmentService;
        private readonly ICrudService<Pricelist> _pricelistService;
        private readonly ICrudService<Price> _priceService;
        private readonly IPricingEvaluatorService _pricingEvaluatorService;


        public PricingServiceImpl(
                ICrudService<PricelistAssignment> pricelistAssignmentService,
                ICrudService<Pricelist> pricelistService,
                ICrudService<Price> priceService,
                IPricingEvaluatorService pricingEvaluatorService
            )
        {
            _pricelistAssignmentService = pricelistAssignmentService;
            _pricelistService = pricelistService;
            _priceService = priceService;
            _pricingEvaluatorService = pricingEvaluatorService;
        }

        public Task DeletePricelistsAssignmentsAsync(string[] ids)
        {
            return _pricelistAssignmentService.DeleteAsync(ids);
        }

        public Task DeletePricelistsAsync(string[] ids)
        {
            return _pricelistService.DeleteAsync(ids);
        }

        public Task DeletePricesAsync(string[] ids)
        {
            return _priceService.DeleteAsync(ids);
        }

        public Task<IEnumerable<Pricelist>> EvaluatePriceListsAsync(PriceEvaluationContext evalContext)
        {
            return _pricingEvaluatorService.EvaluatePriceListsAsync(evalContext);
        }

        public Task<IEnumerable<Price>> EvaluateProductPricesAsync(PriceEvaluationContext evalContext)
        {
            return _pricingEvaluatorService.EvaluateProductPricesAsync(evalContext);
        }

        public async Task<PricelistAssignment[]> GetPricelistAssignmentsByIdAsync(string[] ids)
        {
            return (await _pricelistAssignmentService.GetByIdsAsync(ids)).ToArray();
        }

        public async Task<Pricelist[]> GetPricelistsByIdAsync(string[] ids)
        {
            return (await _pricelistService.GetByIdsAsync(ids)).ToArray();
        }

        public async Task<Price[]> GetPricesByIdAsync(string[] ids)
        {
            return (await _priceService.GetByIdsAsync(ids)).ToArray();
        }

        public Task SavePricelistAssignmentsAsync(PricelistAssignment[] assignments)
        {
            return _pricelistAssignmentService.SaveChangesAsync(assignments);
        }

        public Task SavePricelistsAsync(Pricelist[] priceLists)
        {
            return _pricelistService.SaveChangesAsync(priceLists);
        }

        public Task SavePricesAsync(Price[] prices)
        {
            return _priceService.SaveChangesAsync(prices);
        }
    }
}
