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

#pragma warning disable CS0618 // Allow to use obsoleted

namespace VirtoCommerce.PricingModule.Test
{
    public class PricingServiceImplUnitTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPricingRepository> _repositoryMock;
        private readonly Mock<IItemService> _productServiceMock;
        private readonly Mock<ILogger<PricingEvaluatorService>> _loggerMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly Mock<IPricingPriorityFilterPolicy> _pricingPriorityFilterPolicyMock;

        public PricingServiceImplUnitTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _repositoryMock = new Mock<IPricingRepository>();
            _productServiceMock = new Mock<IItemService>();
            _loggerMock = new Mock<ILogger<PricingEvaluatorService>>();
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

        [Fact]
        public async Task GetByIdsAsync_GetThenSavePricelist_ReturnCachedPricelist()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var newPricelist = new Pricelist { Id = id };
            var newPricelistEntity = AbstractTypeFactory<PricelistEntity>.TryCreateInstance().FromModel(newPricelist, new PrimaryKeyResolvingMap());
            var service = GetPricingServiceImplWithPlatformMemoryCache();
            _repositoryMock.Setup(x => x.Add(newPricelistEntity))
                .Callback(() =>
                {
                    _repositoryMock.Setup(o => o.GetPricelistByIdsAsync(new[] { id }))
                        .ReturnsAsync(new[] { newPricelistEntity });
                });

            //Act
            var nullPricelist = await service.GetPricelistsByIdAsync(new[] { id });
            await service.SavePricelistsAsync(new[] { newPricelist });
            var pricelist = await service.GetPricelistsByIdAsync(new[] { id });

            //Assert
            Assert.NotEqual(nullPricelist, pricelist);
        }

        [Fact]
        public async Task GetByIdsAsync_GetThenSavePricelistAssignment_ReturnCachedPricelistAssignment()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var newPricelistAssignment = new PricelistAssignment { Id = id };
            var newPricelistAssignmentEntity = AbstractTypeFactory<PricelistAssignmentEntity>.TryCreateInstance().FromModel(newPricelistAssignment, new PrimaryKeyResolvingMap());
            var service = GetPricingServiceImplWithPlatformMemoryCache();
            _repositoryMock.Setup(x => x.Add(newPricelistAssignmentEntity))
                .Callback(() =>
                {
                    _repositoryMock.Setup(o => o.GetPricelistAssignmentsByIdAsync(new[] { id }))
                        .ReturnsAsync(new[] { newPricelistAssignmentEntity });
                });

            //Act
            var nullPricelistAssignment = await service.GetPricelistAssignmentsByIdAsync(new[] { id });
            await service.SavePricelistAssignmentsAsync(new[] { newPricelistAssignment });
            var PricelistAssignment = await service.GetPricelistAssignmentsByIdAsync(new[] { id });

            //Assert
            Assert.NotEqual(nullPricelistAssignment, PricelistAssignment);
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
                new PricelistAssignmentService(() => pricingRepository, platformMemoryCache, _eventPublisherMock.Object),
                new PricelistService(() => pricingRepository, platformMemoryCache, _eventPublisherMock.Object),
                new PriceService(() => pricingRepository, platformMemoryCache, _eventPublisherMock.Object, new PricelistService(() => pricingRepository, platformMemoryCache, null)),
                new PricingEvaluatorService(
                        () => pricingRepository,
                        _productServiceMock.Object,
                        _loggerMock.Object,
                        platformMemoryCache,
                        _pricingPriorityFilterPolicyMock.Object
                    )
                );
        }
    }
}
