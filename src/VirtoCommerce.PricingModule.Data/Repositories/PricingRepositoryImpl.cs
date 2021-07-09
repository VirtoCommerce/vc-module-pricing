using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient; //https://github.com/dotnet/efcore/issues/16812
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Data.Model;

namespace VirtoCommerce.PricingModule.Data.Repositories
{
    public class PricingRepositoryImpl : DbContextRepositoryBase<PricingDbContext>, IPricingRepository
    {
        public PricingRepositoryImpl(PricingDbContext dbContext, IUnitOfWork unitOfWork = null)
            : base(dbContext, unitOfWork)
        {
        }

        public IQueryable<PricelistEntity> Pricelists => DbContext.Set<PricelistEntity>();
        public IQueryable<PriceEntity> Prices => DbContext.Set<PriceEntity>();
        public IQueryable<PricelistAssignmentEntity> PricelistAssignments => DbContext.Set<PricelistAssignmentEntity>();

        public virtual async Task<PriceEntity[]> GetPricesByIdsAsync(string[] priceIds)
        {
            // TODO: replace Include with separate query
            var retVal = await Prices.Include(x => x.Pricelist).Where(x => priceIds.Contains(x.Id)).ToArrayAsync();
            return retVal;
        }

        public virtual async Task<PricelistEntity[]> GetPricelistByIdsAsync(string[] pricelistIds)
        {
            // TODO: replace Include with separate query
            var retVal = await Pricelists.Include(x => x.Assignments)
                                         .Where(x => pricelistIds.Contains(x.Id))
                                         .ToArrayAsync();
            return retVal;
        }

        public virtual async Task<PricelistAssignmentEntity[]> GetPricelistAssignmentsByIdAsync(string[] assignmentsId)
        {
            // TODO: replace Include with separate query
            var retVal = await PricelistAssignments.Include(x => x.Pricelist).Where(x => assignmentsId.Contains(x.Id)).ToArrayAsync();
            return retVal;
        }

        public Task DeletePricesAsync(string[] ids)
        {
            return ExecuteSqlCommandAsync("DELETE FROM Price WHERE Id IN ({0})", ids);
        }

        public Task DeletePricelistsAsync(string[] ids)
        {
            return ExecuteSqlCommandAsync("DELETE FROM Pricelist WHERE Id IN ({0})", ids);
        }

        public Task DeletePricelistAssignmentsAsync(string[] ids)
        {
            return ExecuteSqlCommandAsync("DELETE FROM PricelistAssignment WHERE Id IN ({0})", ids);
        }


        protected virtual Task ExecuteSqlCommandAsync(string commandTemplate, IEnumerable<string> parameterValues)
        {
            if (parameterValues?.Count() > 0)
            {
                var command = CreateCommand(commandTemplate, parameterValues);
                return DbContext.Database.ExecuteSqlRawAsync(command.Text, command.Parameters);
            }
            return Task.CompletedTask;
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
