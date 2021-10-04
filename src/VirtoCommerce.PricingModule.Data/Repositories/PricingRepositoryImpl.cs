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

        public virtual async Task<IList<PriceEntity>> GetPricesByIdsAsync(IEnumerable<string> priceIds)
        {
            // TODO: replace Include with separate query
            var result = await Prices.Include(x => x.Pricelist).Where(x => priceIds.Contains(x.Id)).ToListAsync();
            return result;
        }

        public virtual async Task<IList<PricelistEntity>> GetPricelistByIdsAsync(IEnumerable<string> pricelistIds)
        {
            // TODO: replace Include with separate query
            var result = await Pricelists.Include(x => x.Assignments)
                                         .Where(x => pricelistIds.Contains(x.Id))
                                         .ToListAsync();
            return result;
        }

        public virtual async Task<IList<PricelistAssignmentEntity>> GetPricelistAssignmentsByIdAsync(IEnumerable<string> assignmentsId)
        {
            // TODO: replace Include with separate query
            var result = await PricelistAssignments.Include(x => x.Pricelist).Where(x => assignmentsId.Contains(x.Id)).ToListAsync();
            return result;
        }

        public Task DeletePricesAsync(IEnumerable<string> ids)
        {
            return ExecuteSqlCommandAsync("DELETE FROM Price WHERE Id IN ({0})", ids);
        }

        public Task DeletePricelistsAsync(IEnumerable<string> ids)
        {
            return ExecuteSqlCommandAsync("DELETE FROM Pricelist WHERE Id IN ({0})", ids);
        }

        public Task DeletePricelistAssignmentsAsync(IEnumerable<string> ids)
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
            var parameters = parameterValues.Select((v, i) => new SqlParameter($"@p{i}", v));
            var parameterNames = string.Join(",", parameters.Select(p => p.ParameterName));

            return new Command
            {
                Text = string.Format(commandTemplate, parameterNames),
                Parameters = parameters.OfType<object>(),
            };
        }

        protected class Command
        {
            public string Text { get; set; }
            public IEnumerable<object> Parameters { get; set; }
        }
    }
}
