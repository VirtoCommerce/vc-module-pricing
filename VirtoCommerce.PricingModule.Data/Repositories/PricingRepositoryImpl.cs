using System.Linq;
using System.Data.Entity;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Data.Model;
using System;

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

        public Price[] GetPricesByIds(string[] priceIds)
        {
            var retVal = Prices.Include(x => x.Pricelist).Where(x => priceIds.Contains(x.Id)).ToArray();
            return retVal;
        }

        public Pricelist[] GetPricelistByIds(string[] priceListIds)
        {
            var retVal = Pricelists.Include(x => x.Assignments)
                                  .Where(x => priceListIds.Contains(x.Id))
                                  .ToArray();
            return retVal;
        }

        public PricelistAssignment[] GetPricelistAssignmentsById(string[] assignmentsIds)
        {
            var retVal = PricelistAssignments.Include(x => x.Pricelist).Where(x => assignmentsIds.Contains(x.Id)).ToArray();
            return retVal;
        }     


        public void DeletePrices(string[] ids)
        {
            var queryPattern = @"DELETE FROM Price WHERE Id IN ({0})";
            var query = string.Format(queryPattern, string.Join(", ", ids.Select(x => string.Format("'{0}'", x))));
            ObjectContext.ExecuteStoreCommand(query);
        }

        public void DeletePricelists(string[] ids)
        {
            var queryPattern = @"DELETE FROM Pricelist WHERE Id IN ({0})";
            var query = string.Format(queryPattern, string.Join(", ", ids.Select(x => string.Format("'{0}'", x))));
            ObjectContext.ExecuteStoreCommand(query);
        }
        
        public void DeletePricelistAssignments(string[] ids)
        {
            var queryPattern = @"DELETE FROM PricelistAssignment WHERE Id IN ({0})";
            var query = string.Format(queryPattern, string.Join(", ", ids.Select(x => string.Format("'{0}'", x))));
            ObjectContext.ExecuteStoreCommand(query);
        }
        #endregion
    }

}
