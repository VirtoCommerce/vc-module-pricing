using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Data.Model;

namespace VirtoCommerce.PricingModule.Data.Repositories
{
    public class PriceRepository : DbContextRepositoryBase<PricingDbContext>, IPriceRepository
    {
        public PriceRepository(PricingDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<PriceEntity> Prices => DbContext.Set<PriceEntity>();

        public async Task<IEnumerable<PriceEntity>> GetByIdsAsync(IEnumerable<string> ids)
        {
            return await Prices.Include(x => x.Pricelist).Where(x => ids.Contains(x.Id)).ToArrayAsync();
        }
    }
}
