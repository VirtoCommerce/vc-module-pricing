using System.Linq;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule2.Web.Entities;

namespace VirtoCommerce.PricingModule2.Web.Repositories
{
    public class PricingRepository2 : PricingRepositoryImpl
    {
        public PricingRepository2(Pricing2DbContext dbContext)
            : base(dbContext)
        {
        }

        public IQueryable<Price2Entity> Prices2 => DbContext.Set<Price2Entity>();
    }
}
