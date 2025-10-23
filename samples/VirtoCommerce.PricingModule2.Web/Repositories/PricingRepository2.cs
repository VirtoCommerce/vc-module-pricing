using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule2.Web.Repositories
{
    public class PricingRepository2 : PricingRepositoryImpl
    {
        public PricingRepository2(Pricing2DbContext dbContext)
            : base(dbContext)
        {
        }
    }
}
