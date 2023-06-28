using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Data.Model;


namespace VirtoCommerce.PricingModule.Data.Repositories
{
    public interface IPricingRepository : IRepository
    {
        IQueryable<PricelistEntity> Pricelists { get; }
        IQueryable<PriceEntity> Prices { get; }
        IQueryable<PricelistAssignmentEntity> PricelistAssignments { get; }

        Task<IList<PriceEntity>> GetPricesByIdsAsync(IList<string> priceIds);
        Task<IList<PricelistEntity>> GetPricelistByIdsAsync(IList<string> pricelistIds, string responseGroup);
        Task<IList<PricelistAssignmentEntity>> GetPricelistAssignmentsByIdAsync(IList<string> assignmentsId);

        Task DeletePricesAsync(IList<string> ids);
        Task DeletePricelistsAsync(IList<string> ids);
        Task DeletePricelistAssignmentsAsync(IList<string> ids);

        IQueryable<MergedPriceEntity> GetMergedPrices(string basePriceListId, string priorityPriceListId);
    }
}
