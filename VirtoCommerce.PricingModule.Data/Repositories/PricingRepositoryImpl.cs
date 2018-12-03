using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
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

        public IQueryable<PricelistEntity> Pricelists => GetAsQueryable<PricelistEntity>();
        public IQueryable<PriceEntity> Prices => GetAsQueryable<PriceEntity>();
        public IQueryable<PricelistAssignmentEntity> PricelistAssignments => GetAsQueryable<PricelistAssignmentEntity>();

        public virtual PriceEntity[] GetPricesByIds(string[] priceIds)
        {
            var retVal = Prices.Include(x => x.Pricelist).Where(x => priceIds.Contains(x.Id)).ToArray();
            return retVal;
        }

        public virtual PricelistEntity[] GetPricelistByIds(string[] priceListIds)
        {
            var retVal = Pricelists.Include(x => x.Assignments)
                                  .Where(x => priceListIds.Contains(x.Id))
                                  .ToArray();
            return retVal;
        }

        public virtual PricelistAssignmentEntity[] GetPricelistAssignmentsById(string[] assignmentsIds)
        {
            var retVal = PricelistAssignments.Include(x => x.Pricelist).Where(x => assignmentsIds.Contains(x.Id)).ToArray();
            return retVal;
        }

        public void DeletePrices(string[] ids)
        {
            ExecuteStoreCommand("DELETE FROM Price WHERE Id IN ({0})", ids);
        }

        public void DeletePricelists(string[] ids)
        {
            ExecuteStoreCommand("DELETE FROM Pricelist WHERE Id IN ({0})", ids);
        }

        public void DeletePricelistAssignments(string[] ids)
        {
            ExecuteStoreCommand("DELETE FROM PricelistAssignment WHERE Id IN ({0})", ids);
        }


        protected virtual void ExecuteStoreCommand(string commandTemplate, IEnumerable<string> parameterValues)
        {
            var command = CreateCommand(commandTemplate, parameterValues);
            ObjectContext.ExecuteStoreCommand(command.Text, command.Parameters);
        }

        protected virtual Command CreateCommand(string commandTemplate, IEnumerable<string> parameterValues)
        {
            var parameters = parameterValues.Select((v, i) => new SqlParameter($"@p{i}", v)).ToArray();
            var parameterNames = string.Join(",", parameters.Select(p => p.ParameterName));

            return new Command
            {
                Text = string.Format(commandTemplate, parameterNames),
                Parameters = parameters.OfType<object>().ToArray(),
            };
        }

        protected class Command
        {
            public string Text { get; set; }
            public object[] Parameters { get; set; }
        }
    }
}
