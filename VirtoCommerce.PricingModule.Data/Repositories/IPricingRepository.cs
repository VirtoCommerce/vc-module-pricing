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
		IQueryable<Pricelist> Pricelists { get; }
		IQueryable<Price> Prices { get; }
		IQueryable<PricelistAssignment> PricelistAssignments { get; }

		Price[] GetPricesByIds(string[] priceIds);
		Pricelist[] GetPricelistByIds(string[] pricelistIds);
		PricelistAssignment[] GetPricelistAssignmentsById(string[] assignmentsId);

        void DeletePrices(string[] ids);
        void DeletePricelists(string[] ids);
        void DeletePricelistAssignments(string[] ids);
    }
}
