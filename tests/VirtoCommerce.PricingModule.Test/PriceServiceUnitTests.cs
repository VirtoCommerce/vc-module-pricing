using System;
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
    public class PriceServiceUnitTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPriceRepository> _priceRepositoryMock;
        private readonly Mock<IPricelistRepository> _pricelistRepositoryMock;
        private readonly Mock<IPricelistAssignmentRepository> _pricelistAssignmentRepositoryMock;

        private readonly Mock<IPricingRepository> _repositoryMock;
        private readonly Mock<IItemService> _productServiceMock;
        private readonly Mock<ILogger<PricelistAssignmentService>> _loggerMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly Mock<IPricingPriorityFilterPolicy> _pricingPriorityFilterPolicyMock;

        public PriceServiceUnitTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _repositoryMock = new Mock<IPricingRepository>();
            _priceRepositoryMock = new Mock<IPriceRepository>();
            _pricelistRepositoryMock = new Mock<IPricelistRepository>();
            _pricelistAssignmentRepositoryMock = new Mock<IPricelistAssignmentRepository>();
            _productServiceMock = new Mock<IItemService>();
            _loggerMock = new Mock<ILogger<PricelistAssignmentService>>();
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
            var service = GetPriceServiceWithPlatformMemoryCache();
            _priceRepositoryMock.Setup(x => x.Add(newPriceEntity))
                .Callback(() =>
                {
                    _priceRepositoryMock.Setup(o => o.GetByIdsAsync(new[] { id }))
                        .ReturnsAsync(new[] { newPriceEntity });
                });

            //Act
            var nullPrice = await service.GetByIdsAsync(new[] { id });
            await service.SaveChangesAsync(new[] { newPrice });
            var price = await service.GetByIdsAsync(new[] { id });

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
            var service = GetPricelistServiceWithPlatformMemoryCache();
            _pricelistRepositoryMock.Setup(x => x.Add(newPricelistEntity))
                .Callback(() =>
                {
                    _pricelistRepositoryMock.Setup(o => o.GetByIdsAsync(new[] { id }))
                        .ReturnsAsync(new[] { newPricelistEntity });
                });

            //Act
            var nullPricelist = await service.GetByIdsAsync(new[] { id });
            await service.SaveChangesAsync(new[] { newPricelist });
            var pricelist = await service.GetByIdsAsync(new[] { id });

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
            var service = GetPricelistAssignmentServiceWithPlatformMemoryCache();
            _pricelistAssignmentRepositoryMock.Setup(x => x.Add(newPricelistAssignmentEntity))
                .Callback(() =>
                {
                    _pricelistAssignmentRepositoryMock.Setup(o => o.GetByIdsAsync(new[] { id }))
                        .ReturnsAsync(new[] { newPricelistAssignmentEntity });
                });

            //Act
            var nullPricelistAssignment = await service.GetByIdsAsync(new[] { id });
            await service.SaveChangesAsync(new[] { newPricelistAssignment });
            var PricelistAssignment = await service.GetByIdsAsync(new[] { id });

            //Assert
            Assert.NotEqual(nullPricelistAssignment, PricelistAssignment);
        }

        private PriceService GetPriceServiceWithPlatformMemoryCache()
        {
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);
            _priceRepositoryMock.Setup(ss => ss.UnitOfWork).Returns(_unitOfWorkMock.Object);
            _priceRepositoryMock.Setup(ss => ss.UnitOfWork).Returns(_unitOfWorkMock.Object);

            return GetPriceService(platformMemoryCache, _priceRepositoryMock.Object);
        }


        private PricelistService GetPricelistServiceWithPlatformMemoryCache()
        {
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);
            _pricelistRepositoryMock.Setup(ss => ss.UnitOfWork).Returns(_unitOfWorkMock.Object);
            _pricelistRepositoryMock.Setup(ss => ss.UnitOfWork).Returns(_unitOfWorkMock.Object);

            return GetPricelistService(platformMemoryCache, _pricelistRepositoryMock.Object);
        }


        private PricelistAssignmentService GetPricelistAssignmentServiceWithPlatformMemoryCache()
        {
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);
            _pricelistAssignmentRepositoryMock.Setup(ss => ss.UnitOfWork).Returns(_unitOfWorkMock.Object);
            _pricelistAssignmentRepositoryMock.Setup(ss => ss.UnitOfWork).Returns(_unitOfWorkMock.Object);

            return GetPricelistAssignmentService(platformMemoryCache, _pricelistAssignmentRepositoryMock.Object);
        }

        private PriceService GetPriceService(IPlatformMemoryCache platformMemoryCache, IPriceRepository priceRepository)
        {
            var pricelistService = GetPricelistService(platformMemoryCache, _pricelistRepositoryMock.Object);
            var pricelistAssignmentService = GetPricelistAssignmentService(platformMemoryCache, _pricelistAssignmentRepositoryMock.Object);
            return new PriceService(
                () => priceRepository,
                platformMemoryCache,
                _eventPublisherMock.Object,
                _pricingPriorityFilterPolicyMock.Object,
                pricelistAssignmentService,
                pricelistService,
                _productServiceMock.Object
                );
        }

        private PricelistService GetPricelistService(IPlatformMemoryCache platformMemoryCache, IPricelistRepository pricelistRepository)
        {

            return new PricelistService(
                () => pricelistRepository,
                platformMemoryCache,
                _eventPublisherMock.Object
                );
        }

        private PricelistAssignmentService GetPricelistAssignmentService(IPlatformMemoryCache platformMemoryCache, IPricelistAssignmentRepository pricelistAssignmentRepository)
        {

            return new PricelistAssignmentService(
                () => pricelistAssignmentRepository,
                platformMemoryCache,
                _eventPublisherMock.Object
                , _loggerMock.Object
                );
        }
    }
}
