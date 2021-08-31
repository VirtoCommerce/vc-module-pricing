using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Data.Model;

namespace VirtoCommerce.PricingModule.Data.Repositories
{
    public interface IPriceRepository : IRepository
    {
        IQueryable<PriceEntity> Prices { get; }
        Task<IEnumerable<PriceEntity>> GetByIdsAsync(IEnumerable<string> ids);
    }
}
