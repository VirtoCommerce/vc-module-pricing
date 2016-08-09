using System.Linq;
using System.Data.Entity;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Data.Model;

namespace VirtoCommerce.PricingModule.Data.Repositories
{
    public class PricingRepositoryImpl : EFRepositoryBase, IPricingRepository
    {
        public PricingRepositoryImpl()
        {
        }

        public PricingRepositoryImpl(string nameOrConnectionString, params IInterceptor[] interceptors)
            : base(nameOrConnectionString, null, interceptors)
        {
            Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Price>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<Price>().HasRequired(x => x.Pricelist).WithMany(x => x.Prices).HasForeignKey(x => x.PricelistId);
            modelBuilder.Entity<Price>().ToTable("Price");

            modelBuilder.Entity<Pricelist>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<Pricelist>().ToTable("Pricelist");

            modelBuilder.Entity<PricelistAssignment>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<PricelistAssignment>().HasRequired(x => x.Pricelist).WithMany(x => x.Assignments).HasForeignKey(x => x.PricelistId);
            modelBuilder.Entity<PricelistAssignment>().ToTable("PricelistAssignment");

            base.OnModelCreating(modelBuilder);
        }

        #region IPricingRepository Members

        public IQueryable<Pricelist> Pricelists
        {
            get { return GetAsQueryable<Pricelist>(); }
        }

        public IQueryable<Price> Prices
        {
            get { return GetAsQueryable<Price>(); }
        }

        public IQueryable<PricelistAssignment> PricelistAssignments
        {
            get { return GetAsQueryable<PricelistAssignment>(); }
        }

        public Price GetPriceById(string priceId)
        {
            var retVal = Prices.Include(x => x.Pricelist).FirstOrDefault(x => x.Id == priceId);
            return retVal;
        }

        public Pricelist GetPricelistById(string priceListId)
        {
            var retVal = Pricelists.Include(x => x.Assignments)
                                   .FirstOrDefault(x => x.Id == priceListId);
            return retVal;
        }

        public PricelistAssignment GetPricelistAssignmentById(string assignmentId)
        {
            var retVal = PricelistAssignments.FirstOrDefault(x => x.Id == assignmentId);
            return retVal;
        }


        public PricelistAssignment[] GetAllPricelistAssignments(string pricelistId)
        {
            var retVal = PricelistAssignments.Where(x => x.PricelistId == pricelistId);
            return retVal.ToArray();
        }

        #endregion
    }

}
