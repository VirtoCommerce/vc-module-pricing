using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Data.Model;

namespace VirtoCommerce.PricingModule.Data.Repositories
{
    public interface IPricelistAssignmentRepository : IRepository
    {
        IQueryable<PricelistAssignmentEntity> PricelistAssignments { get; }
        Task<IEnumerable<PricelistAssignmentEntity>> GetByIdsAsync(IEnumerable<string> ids);
    }
}
