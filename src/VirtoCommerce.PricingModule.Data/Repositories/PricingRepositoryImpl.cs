using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public virtual async Task<IList<PriceEntity>> GetPricesByIdsAsync(IList<string> priceIds)
        {
            var result = await Prices.Include(x => x.Pricelist).Where(x => priceIds.Contains(x.Id)).ToListAsync();
            return result;
        }

        public virtual async Task<IList<PricelistEntity>> GetPricelistByIdsAsync(IList<string> pricelistIds, string responseGroup)
        {
            var pricelistResponseGroup = EnumUtility.SafeParseFlags(responseGroup, PriceListResponseGroup.Full);

            var query = Pricelists;
            if (pricelistResponseGroup == PriceListResponseGroup.Full)
            {
                query = query.Include(x => x.Assignments);
            }

            var result = await query.Where(x => pricelistIds.Contains(x.Id))
                                         .ToListAsync();
            return result;
        }

        public virtual async Task<IList<PricelistAssignmentEntity>> GetPricelistAssignmentsByIdAsync(IList<string> assignmentsId)
        {
            var result = await PricelistAssignments.Include(x => x.Pricelist).Where(x => assignmentsId.Contains(x.Id)).ToListAsync();
            return result;
        }

        public Task DeletePricesAsync(IList<string> ids)
        {
            return ExecuteSqlCommandAsync("DELETE FROM \"Price\" WHERE Id IN ({0})", ids);
        }

        public Task DeletePricelistsAsync(IList<string> ids)
        {
            return ExecuteSqlCommandAsync("DELETE FROM \"Pricelist\" WHERE Id IN ({0})", ids);
        }

        public Task DeletePricelistAssignmentsAsync(IList<string> ids)
        {
            return ExecuteSqlCommandAsync("DELETE FROM \"PricelistAssignment\" WHERE Id IN ({0})", ids);
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
        private static Command GetSearchMergedPricesCommand(string basePriceListId, string priorityPriceListId)
        {
            var template = @"SELECT A.* FROM
(
    SELECT P.""Id"", P.""ProductId"", P.""List"", P.""Sale"", P.""MinQuantity"", P.""PricelistId"", 2 as ""State""
        FROM ""Price"" AS P
            JOIN ""Price"" AS B
            ON P.""ProductId"" = B.""ProductId"" and P.""MinQuantity"" = B.""MinQuantity""
        WHERE B.""PricelistId"" = {0} AND P.""PricelistId"" = {1}
    UNION 
    SELECT ""Id"", ""ProductId"", ""List"", ""Sale"", ""MinQuantity"", ""PricelistId"", 0 as ""State""
        FROM ""Price"" C
        WHERE C.""PricelistId"" = {0} and NOT EXISTS 
        (
            SELECT P.""ProductId"", P.""MinQuantity""
                FROM ""Price"" AS P
                    JOIN ""Price"" AS B
                    ON P.""ProductId"" = C.""ProductId"" AND P.""MinQuantity"" = C.""MinQuantity""
                WHERE B.""PricelistId"" = {0} AND P.""PricelistId"" = {1}
        )
    UNION
    SELECT  ""Id"", ""ProductId"", ""List"", ""Sale"", ""MinQuantity"", ""PricelistId"", 1 as ""State""
        FROM ""Price"" S
        WHERE S.""PricelistId"" = {1} AND NOT EXISTS 
        (
            SELECT P.""ProductId"", P.""MinQuantity""
                FROM ""Price"" AS P
                    JOIN ""Price"" AS B
                    ON B.""ProductId"" = S.""ProductId"" and B.""MinQuantity"" = S.""MinQuantity""
                WHERE B.""PricelistId"" = {0} AND P.""PricelistId"" = {1}
         )
) A";

            return new Command
            {
                Text = template,
                Parameters = new List<object> { basePriceListId, priorityPriceListId }
            };
        }
        #endregion

        #region Commands
        protected virtual Task ExecuteSqlCommandAsync(string commandTemplate, IList<string> parameterValues)
        {
            if (parameterValues?.Count > 0)
            {
                var command = CreateCommand(commandTemplate, parameterValues);
                return DbContext.Database.ExecuteSqlRawAsync(command.Text, command.Parameters);
            }
            return Task.CompletedTask;
        }

        protected virtual Command CreateCommand(string commandTemplate, IList<string> parameterValues)
        {
            var parameters = parameterValues.Select((v, i) => $"{{{i}}}").ToList<object>();
            var parameterNames = string.Join(",", parameters);

            return new Command
            {
                Text = string.Format(commandTemplate, parameterNames),
                Parameters = parameters,
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
