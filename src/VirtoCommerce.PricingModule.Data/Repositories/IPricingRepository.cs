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

        Task<IList<PriceEntity>> GetPricesByIdsAsync(IEnumerable<string> priceIds);
        Task<IList<PricelistEntity>> GetPricelistByIdsAsync(IEnumerable<string> pricelistIds);
        Task<IList<PricelistAssignmentEntity>> GetPricelistAssignmentsByIdAsync(IEnumerable<string> assignmentsId);

        Task DeletePricesAsync(IEnumerable<string> ids);
        Task DeletePricelistsAsync(IEnumerable<string> ids);
        Task DeletePricelistAssignmentsAsync(IEnumerable<string> ids);
    }
}
