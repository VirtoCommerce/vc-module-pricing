using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Data.Model;

namespace VirtoCommerce.PricingModule.Data.Repositories
{
    public class PricelistRepository : DbContextRepositoryBase<PricingDbContext>, IPricelistRepository
    {
        public PricelistRepository(PricingDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<PricelistEntity> Pricelists => DbContext.Set<PricelistEntity>();

        public async Task<IEnumerable<PricelistEntity>> GetByIdsAsync(IEnumerable<string> ids)
        {
            return await Pricelists.Include(x => x.Assignments)
                                         .Where(x => ids.Contains(x.Id))
                                         .ToArrayAsync();
        }
    }
}
