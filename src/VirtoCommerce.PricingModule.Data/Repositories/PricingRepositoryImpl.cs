using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient; //https://github.com/dotnet/efcore/issues/16812
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Core.Model;
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

        public virtual async Task<ICollection<PriceEntity>> GetPricesByIdsAsync(IEnumerable<string> priceIds)
        {
            // TODO: replace Include with separate query
            var result = await Prices.Include(x => x.Pricelist).Where(x => priceIds.Contains(x.Id)).ToListAsync();
            return result;
        }

        public virtual async Task<ICollection<PricelistEntity>> GetPricelistByIdsAsync(IEnumerable<string> pricelistIds, string responseGroup)
        {
            var pricelistResponseGroup = EnumUtility.SafeParseFlags(responseGroup, PriceListResponseGroup.Full);

            var query = Pricelists;
            if (pricelistResponseGroup == PriceListResponseGroup.Full)
            {
                // TODO: replace Include with separate query
                query = query.Include(x => x.Assignments);
            }

            var result = await query
                                         .Where(x => pricelistIds.Contains(x.Id))
                                         .ToListAsync();
            return result;
        }

        public virtual async Task<ICollection<PricelistAssignmentEntity>> GetPricelistAssignmentsByIdAsync(IEnumerable<string> assignmentsId)
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

        /// <summary>
        /// Readonly only DBSet, do not try to insert/update/delete anything here
        /// </summary>
        public IQueryable<MergedPriceEntity> GetMergedPrices(string basePriceListId, string priorityPriceListId)
        {
            var command = GetSearchMergedPricesCommand(basePriceListId, priorityPriceListId);
            var query = DbContext.Set<MergedPriceEntity>().FromSqlRaw(command.Text, command.Parameters.ToArray());
            return query;
        }

        #region Raw queries
        private Command GetSearchMergedPricesCommand(string basePriceListId, string priorityPriceListId)
        {
            var template = @"
            select a.* from
            (select p.Id, p.ProductId, p.List, p.Sale, p.MinQuantity, p.PricelistId, 2 as [State]
		        FROM Price AS p
		        JOIN Price AS b
		        on p.ProductId = b.ProductId and p.MinQuantity = b.MinQuantity
		        WHERE b.PricelistId = @basePriceListId AND p.PricelistId = @priorityPriceListId
	        union 
	        select Id, ProductId, List, Sale, MinQuantity, PricelistId, 0 as [State]
		        from Price c 
		        where c.PricelistId = @basePriceListId and NOT EXISTS 
		        (
		        select p.ProductId, p.MinQuantity
		        FROM Price AS p
		        JOIN Price AS b
		        on p.ProductId = c.ProductId and p.MinQuantity = c.MinQuantity
		        WHERE b.PricelistId = @basePriceListId AND p.PricelistId = @priorityPriceListId
		        )
	        union
		    select Id, ProductId, List, Sale, MinQuantity, PricelistId, 1 as [State]
		        from Price s
		        where s.PricelistId = @priorityPriceListId and NOT EXISTS 
		        (
		        select p.ProductId, p.MinQuantity
		        FROM Price AS p
		        JOIN Price AS b
		        on b.ProductId = s.ProductId and b.MinQuantity = s.MinQuantity
		        WHERE b.PricelistId = @basePriceListId AND p.PricelistId = @priorityPriceListId
		        )) a";

            var basePriceListIdParam = new SqlParameter("@basePriceListId", basePriceListId);
            var priorityPriceListIdParam = new SqlParameter("@priorityPriceListId", priorityPriceListId);

            return new Command
            {
                Text = template,
                Parameters = new List<object> { basePriceListIdParam, priorityPriceListIdParam }
            };
        }
        #endregion

        #region Commands
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
        #endregion
    }
}
