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
            modelBuilder.Entity<PriceEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<PriceEntity>().HasRequired(x => x.Pricelist).WithMany(x => x.Prices).HasForeignKey(x => x.PricelistId);
            modelBuilder.Entity<PriceEntity>().ToTable("Price");

            modelBuilder.Entity<PricelistEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<PricelistEntity>().ToTable("Pricelist");

            modelBuilder.Entity<PricelistAssignmentEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<PricelistAssignmentEntity>().HasRequired(x => x.Pricelist).WithMany(x => x.Assignments).HasForeignKey(x => x.PricelistId);
            modelBuilder.Entity<PricelistAssignmentEntity>().ToTable("PricelistAssignment");

            base.OnModelCreating(modelBuilder);
        }

        #region IPricingRepository Members

        public IQueryable<PricelistEntity> Pricelists
        {
            get { return GetAsQueryable<PricelistEntity>(); }
        }

        public IQueryable<PriceEntity> Prices
        {
            get { return GetAsQueryable<PriceEntity>(); }
        }

        public IQueryable<PricelistAssignmentEntity> PricelistAssignments
        {
            get { return GetAsQueryable<PricelistAssignmentEntity>(); }
        }

        public PriceEntity[] GetPricesByIds(string[] priceIds)
        {
            var retVal = Prices.Include(x => x.Pricelist).Where(x => priceIds.Contains(x.Id)).ToArray();
            return retVal;
        }

        public PricelistEntity[] GetPricelistByIds(string[] priceListIds)
        {
            var retVal = Pricelists.Include(x => x.Assignments)
                                  .Where(x => priceListIds.Contains(x.Id))
                                  .ToArray();
            return retVal;
        }

        public PricelistAssignmentEntity[] GetPricelistAssignmentsById(string[] assignmentsIds)
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
