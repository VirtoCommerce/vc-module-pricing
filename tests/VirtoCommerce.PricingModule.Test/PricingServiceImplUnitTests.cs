using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Services;
using Xunit;

namespace VirtoCommerce.PricingModule.Test
{
    public class PricingServiceImplUnitTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPricingRepository> _repositoryMock;
        private readonly Mock<IItemService> _productServiceMock;
        private readonly Mock<ILogger<PricingServiceImpl>> _loggerMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly Mock<IPricingPriorityFilterPolicy> _pricingPriorityFilterPolicyMock;

        public PricingServiceImplUnitTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _repositoryMock = new Mock<IPricingRepository>();
            _productServiceMock = new Mock<IItemService>();
            _loggerMock = new Mock<ILogger<PricingServiceImpl>>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _pricingPriorityFilterPolicyMock = new Mock<IPricingPriorityFilterPolicy>();
        }

        [Fact]
        public async Task GetByIdsAsync_GetThenSavePrice_ReturnCachedPrice()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var newPrice = new Price { Id = id };
            var newPriceEntity = AbstractTypeFactory<PriceEntity>.TryCreateInstance().FromModel(newPrice, new PrimaryKeyResolvingMap());
            var service = GetPricingServiceImplWithPlatformMemoryCache();
            _repositoryMock.Setup(x => x.Add(newPriceEntity))
                .Callback(() =>
                {
                    _repositoryMock.Setup(o => o.GetPricesByIdsAsync(new[] { id }))
                        .ReturnsAsync(new[] { newPriceEntity });
                });

            //Act
            var nullPrice = await service.GetPricesByIdAsync(new []{ id });
            await service.SavePricesAsync(new[] { newPrice });
            var price = await service.GetPricesByIdAsync(new[] { id });

            //Assert
            Assert.NotEqual(nullPrice, price);
        }


        private PricingServiceImpl GetPricingServiceImplWithPlatformMemoryCache()
        {
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);
            _repositoryMock.Setup(ss => ss.UnitOfWork).Returns(_unitOfWorkMock.Object);

            return GetPricingServiceImpl(platformMemoryCache, _repositoryMock.Object);
        }

        private PricingServiceImpl GetPricingServiceImpl(IPlatformMemoryCache platformMemoryCache, IPricingRepository pricingRepository)
        {

            return new PricingServiceImpl(
                () => pricingRepository,
                _productServiceMock.Object,
                _loggerMock.Object,
                platformMemoryCache,
                _eventPublisherMock.Object,
                _pricingPriorityFilterPolicyMock.Object
                );
        }
    }
}
