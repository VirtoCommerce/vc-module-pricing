using Microsoft.EntityFrameworkCore;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule2.Web.Entities;

namespace VirtoCommerce.PricingModule2.Web.Repositories
{
    public class Pricing2DbContext : PricingDbContext
    {
        public Pricing2DbContext(DbContextOptions<Pricing2DbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Price2Entity>();
        }
    }
}
