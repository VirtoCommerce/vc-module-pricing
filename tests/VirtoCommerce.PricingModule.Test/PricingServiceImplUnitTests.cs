using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Services;
using VirtoCommerce.PricingModule.Data.Validators;
using Xunit;

namespace VirtoCommerce.PricingModule.Test
{
    public class PricingServiceImplUnitTests
    {
        private readonly Mock<IPricingRepository> _repositoryMock;
        private readonly PricelistAssignmentService _pricelistAssignmentService;
        private readonly PricelistService _pricelistService;
        private readonly PriceService _priceService;

        public PricingServiceImplUnitTests()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            _repositoryMock = new Mock<IPricingRepository>();
            _repositoryMock.Setup(ss => ss.UnitOfWork).Returns(unitOfWorkMock.Object);

            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);

            var eventPublisherMock = new Mock<IEventPublisher>();

            _pricelistAssignmentService = new PricelistAssignmentService(() => _repositoryMock.Object, platformMemoryCache, eventPublisherMock.Object, new PricelistAssignmentsValidator());
            _pricelistService = new PricelistService(() => _repositoryMock.Object, platformMemoryCache, eventPublisherMock.Object);
            _priceService = new PriceService(() => _repositoryMock.Object, platformMemoryCache, eventPublisherMock.Object, _pricelistService);
        }

        [Fact]
        public async Task GetAsync_GetThenSavePrice_ReturnCachedPrice()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var newPrice = new Price { Id = id };
            var newPriceEntity = AbstractTypeFactory<PriceEntity>.TryCreateInstance().FromModel(newPrice, new PrimaryKeyResolvingMap());
            _repositoryMock.Setup(x => x.Add(newPriceEntity))
                .Callback(() =>
                {
                    _repositoryMock.Setup(o => o.GetPricesByIdsAsync(new[] { id }))
                        .ReturnsAsync(new List<PriceEntity>(new[] { newPriceEntity }));
                });

            _repositoryMock.Setup(o => o.GetPricesByIdsAsync(new[] { id }))
                .ReturnsAsync(new List<PriceEntity>());
            _repositoryMock.Setup(o => o.GetPricelistByIdsAsync(It.IsAny<IList<string>>(), null))
                .ReturnsAsync(new List<PricelistEntity>());
            //Act
            var nullPrice = await _priceService.GetAsync(id);
            await _priceService.SaveChangesAsync(new[] { newPrice });
            var price = await _priceService.GetAsync(id);

            //Assert
            Assert.NotEqual(nullPrice, price);
        }

        [Fact]
        public async Task GetAsync_GetThenSavePricelist_ReturnCachedPricelist()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var newPricelist = new Pricelist { Id = id };
            var newPricelistEntity = AbstractTypeFactory<PricelistEntity>.TryCreateInstance().FromModel(newPricelist, new PrimaryKeyResolvingMap());
            _repositoryMock.Setup(x => x.Add(newPricelistEntity))
                .Callback(() =>
                {
                    _repositoryMock.Setup(o => o.GetPricelistByIdsAsync(new[] { id }, null))
                        .ReturnsAsync(new List<PricelistEntity>(new[] { newPricelistEntity }));
                });

            _repositoryMock.Setup(o => o.GetPricelistByIdsAsync(new[] { id }, null))
                .ReturnsAsync(new List<PricelistEntity>());

            //Act
            var nullPricelist = await _pricelistService.GetAsync(id);
            await _pricelistService.SaveChangesAsync(new[] { newPricelist });
            var pricelist = await _pricelistService.GetAsync(id);

            //Assert
            Assert.NotEqual(nullPricelist, pricelist);
        }

        [Fact]
        public async Task GetAsync_GetThenSavePricelistAssignment_ReturnCachedPricelistAssignment()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var newPricelistAssignment = new PricelistAssignment
            {
                Id = id,
                StoreId = Guid.NewGuid().ToString()
            };

            var newPricelistAssignmentEntity = AbstractTypeFactory<PricelistAssignmentEntity>.TryCreateInstance().FromModel(newPricelistAssignment, new PrimaryKeyResolvingMap());
            _repositoryMock.Setup(x => x.Add(newPricelistAssignmentEntity))
                .Callback(() =>
                {
                    _repositoryMock.Setup(o => o.GetPricelistAssignmentsByIdAsync(new[] { id }))
                        .ReturnsAsync(new List<PricelistAssignmentEntity>(new[] { newPricelistAssignmentEntity }));
                });

            _repositoryMock.Setup(o => o.GetPricelistAssignmentsByIdAsync(new[] { id }))
                .ReturnsAsync(new List<PricelistAssignmentEntity>());
            //Act
            var nullPricelistAssignment = await _pricelistAssignmentService.GetAsync(id);
            await _pricelistAssignmentService.SaveChangesAsync(new[] { newPricelistAssignment });
            var pricelistAssignment = await _pricelistAssignmentService.GetAsync(id);

            //Assert
            Assert.NotEqual(nullPricelistAssignment, pricelistAssignment);
        }

        [Fact]
        public Task SavePricelistAssignment_StoreAndCatalogNotNull_ValidationExceptionThrown()
        {
            //Arrange
            var newPricelistAssignment = new PricelistAssignment
            {
                StoreId = Guid.NewGuid().ToString(),
                CatalogId = Guid.NewGuid().ToString(),
            };

            // Assert
            return Assert.ThrowsAsync<ValidationException>(() => _pricelistAssignmentService.SaveChangesAsync(new[] { newPricelistAssignment }));
        }

        [Fact]
        public Task SavePricelistAssignment_StoreAndCatalogNull_ValidationExceptionThrown()
        {
            //Arrange
            var newPricelistAssignment = new PricelistAssignment();

            // Assert
            return Assert.ThrowsAsync<ValidationException>(() => _pricelistAssignmentService.SaveChangesAsync(new[] { newPricelistAssignment }));
        }
    }
}
