using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Data.Model;

namespace VirtoCommerce.PricingModule.Data.Repositories
{
    public interface IPricelistRepository : IRepository
    {
        IQueryable<PricelistEntity> Pricelists { get; }
        Task<IEnumerable<PricelistEntity>> GetByIdsAsync(IEnumerable<string> ids);
    }
}
