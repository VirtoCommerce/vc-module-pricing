using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Data.Model;

namespace VirtoCommerce.PricingModule.Data.Repositories
{
    public class PricelistAssignmentRepository : DbContextRepositoryBase<PricingDbContext>, IPricelistAssignmentRepository
    {
        public PricelistAssignmentRepository(PricingDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<PricelistAssignmentEntity> PricelistAssignments => DbContext.Set<PricelistAssignmentEntity>();

        public async Task<IEnumerable<PricelistAssignmentEntity>> GetByIdsAsync(IEnumerable<string> ids)
        {
            return await PricelistAssignments.Include(x => x.Pricelist)
                                             .Where(x => ids.Contains(x.Id))
                                             .ToArrayAsync();
        }
    }
}
