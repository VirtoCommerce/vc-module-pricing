using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Services.Search.Basic;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricelistAssignmentSearchService : BasicPricelistAssignmentSearchService<PricelistAssignmentSearchResult, PricelistAssignment, PricelistAssignmentEntity>
    {
        public PricelistAssignmentSearchService(Func<IPricingRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache,
            ICrudService<PricelistAssignment> pricelistAssignmentService)
           : base(repositoryFactory, platformMemoryCache, pricelistAssignmentService)
        {
        }
    }
}
