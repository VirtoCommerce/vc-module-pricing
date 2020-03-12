using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Data.Model;


namespace VirtoCommerce.PricingModule.Data.Repositories
{
	public interface IPricingRepository : IRepository
	{
		IQueryable<PricelistEntity> Pricelists { get; }
		IQueryable<PriceEntity> Prices { get; }
		IQueryable<PricelistAssignmentEntity> PricelistAssignments { get; }

		PriceEntity[] GetPricesByIds(string[] priceIds);
		PricelistEntity[] GetPricelistByIds(string[] pricelistIds);
		PricelistAssignmentEntity[] GetPricelistAssignmentsById(string[] assignmentsId);

        void DeletePrices(string[] ids);
        void DeletePricelists(string[] ids);
        void DeletePricelistAssignments(string[] ids);
    }
}
