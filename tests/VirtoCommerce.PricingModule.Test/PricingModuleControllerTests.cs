using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Validators;
using VirtoCommerce.PricingModule.Web.Controllers.Api;
using Xunit;

namespace VirtoCommerce.PricingModule.Test
{
    public class PricingModuleControllerTests
    {
        private const string ProductId1 = "TestProduct_1";
        private const string ProductId2 = "TestProduct_2";
        private const string PricelistId = "TestPricelist_1";

        private readonly List<Price> _existingPrices = new List<Price>();
        private readonly List<PricesSearchCriteria> _searchCriteria = new List<PricesSearchCriteria>();
        private readonly List<Price> _savedPrices = new List<Price>();
        private readonly PricingModuleController _controller;

        public PricingModuleControllerTests()
        {
            var priceSearchServiceMock = new Mock<IPriceSearchService>();
            priceSearchServiceMock
                .Setup(x => x.SearchAsync(It.IsAny<PricesSearchCriteria>(), It.IsAny<bool>()))
                .Callback<PricesSearchCriteria, bool>((criteria, _) => _searchCriteria.Add(criteria))
                .ReturnsAsync(() => new PriceSearchResult
                {
                    Results = _existingPrices.ToList(),
                    TotalCount = _existingPrices.Count,
                });

            var priceServiceMock = new Mock<IPriceService>();
            priceServiceMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<IList<Price>>()))
                .Callback<IList<Price>>(prices => _savedPrices.AddRange(prices))
                .Returns(Task.CompletedTask);

            _controller = new PricingModuleController(
                priceSearchServiceMock.Object,
                priceServiceMock.Object,
                new Mock<IPricelistSearchService>().Object,
                new Mock<IPricelistService>().Object,
                new Mock<IPricelistAssignmentSearchService>().Object,
                new Mock<IPricelistAssignmentService>().Object,
                new Mock<IPricingEvaluatorService>().Object,
                new Mock<IItemService>().Object,
                new Mock<IMergedPriceSearchService>().Object,
                new Mock<IBlobUrlResolver>().Object,
                new Mock<AbstractValidator<Pricelist>>().Object,
                new ProductPriceValidator(new PriceValidator()));
        }

        [Fact]
        public async Task UpdateProductPrices_NestedPriceProductIdOmitted_TakesProductIdFromWrapper()
        {
            //Arrange
            var newPrice = CreatePrice(id: null, productId: null, list: 10m);
            var productPrice = CreateProductPrice(ProductId1, newPrice);

            //Act
            await _controller.UpdateProductPrices(productPrice);

            //Assert
            _savedPrices.Should().HaveCount(1);
            _savedPrices[0].ProductId.Should().Be(ProductId1);
        }

        [Fact]
        public async Task UpdateProductPrices_ExistingPriceWithNestedProductIdOmitted_KeepsProductId()
        {
            //Arrange
            var existingPrice = CreatePrice(id: "TestPrice_1", productId: ProductId1, list: 10m);
            _existingPrices.Add(existingPrice);

            var updatedPrice = CreatePrice(id: existingPrice.Id, productId: null, list: 20m);
            var productPrice = CreateProductPrice(ProductId1, updatedPrice);

            //Act
            await _controller.UpdateProductPrices(productPrice);

            //Assert
            _savedPrices.Should().HaveCount(1);
            _savedPrices[0].ProductId.Should().Be(ProductId1);
        }

        [Fact]
        public async Task UpdateProductsPrices_SeveralProducts_TakesProductIdFromOwnWrapper()
        {
            //Arrange
            var firstProductPrice = CreateProductPrice(ProductId1, CreatePrice(id: null, productId: null, list: 10m));
            var secondProductPrice = CreateProductPrice(ProductId2, CreatePrice(id: null, productId: null, list: 20m));

            //Act
            await _controller.UpdateProductsPrices([firstProductPrice, secondProductPrice]);

            //Assert
            _savedPrices.Should().HaveCount(2);
            _savedPrices.Single(x => x.List == 10m).ProductId.Should().Be(ProductId1);
            _savedPrices.Single(x => x.List == 20m).ProductId.Should().Be(ProductId2);

            _searchCriteria.Should().HaveCount(1);
            _searchCriteria[0].ProductIds.Should().BeEquivalentTo([ProductId1, ProductId2]);
        }

        [Fact]
        public async Task UpdateProductPrices_NestedPriceProductIdSet_KeepsNestedProductId()
        {
            //Arrange
            var newPrice = CreatePrice(id: null, productId: ProductId2, list: 10m);
            var productPrice = CreateProductPrice(ProductId1, newPrice);

            //Act
            await _controller.UpdateProductPrices(productPrice);

            //Assert
            _savedPrices.Should().HaveCount(1);
            _savedPrices[0].ProductId.Should().Be(ProductId2);
        }

        [Fact]
        public async Task UpdateProductPrices_WrapperProductIdOmitted_DoesNotFillWrapperFromNestedPrice()
        {
            //Arrange
            var newPrice = CreatePrice(id: null, productId: ProductId1, list: 10m);
            var productPrice = CreateProductPrice(productId: null, newPrice);

            //Act
            await _controller.UpdateProductPrices(productPrice);

            //Assert
            _savedPrices.Should().HaveCount(1);
            _savedPrices[0].ProductId.Should().Be(ProductId1);

            productPrice.ProductId.Should().BeNull();
            _searchCriteria.Should().HaveCount(1);
            _searchCriteria[0].ProductIds.Should().BeEquivalentTo([(string)null]);
        }

        [Theory]
        [InlineData(-10, null, 1, "[0].Prices[0].List")]
        [InlineData(0, null, 1, "[0].Prices[0].List")]
        [InlineData(10, -5, 1, "[0].Prices[0].Sale")]
        [InlineData(10, 20, 1, "[0].Prices[0].Sale")]
        [InlineData(10, null, 0, "[0].Prices[0].MinQuantity")]
        public async Task UpdateProductPrices_InvalidPrice_ReturnsBadRequestAndDoesNotSave(int list, int? sale, int minQuantity, string expectedErrorKey)
        {
            //Arrange
            var price = CreatePrice(id: null, productId: ProductId1, list: list, sale: sale, minQuantity: minQuantity);
            var productPrice = CreateProductPrice(ProductId1, price);

            //Act
            var result = await _controller.UpdateProductPrices(productPrice);

            //Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().BeOfType<SerializableError>()
                .Which.Keys.Should().BeEquivalentTo([expectedErrorKey]);
            _savedPrices.Should().BeEmpty();
            _searchCriteria.Should().BeEmpty();
        }

        [Fact]
        public async Task UpdateProductsPrices_InvalidPriceInSecondProduct_ReportsItsIndex()
        {
            //Arrange
            var validProductPrice = CreateProductPrice(ProductId1, CreatePrice(id: null, productId: null, list: 10m));
            var invalidProductPrice = CreateProductPrice(ProductId2,
                CreatePrice(id: null, productId: null, list: 10m, minQuantity: 5),
                CreatePrice(id: null, productId: null, list: -1m));

            //Act
            var result = await _controller.UpdateProductsPrices([validProductPrice, invalidProductPrice]);

            //Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().BeOfType<SerializableError>()
                .Which.Keys.Should().BeEquivalentTo(["[1].Prices[1].List"]);
            _savedPrices.Should().BeEmpty();
        }

        [Theory]
        [InlineData(10, null)]
        [InlineData(10, 0)]
        [InlineData(10, 10)]
        [InlineData(0.01, 0)]
        public async Task UpdateProductPrices_ValidPrice_Saves(double list, int? sale)
        {
            //Arrange
            var price = CreatePrice(id: null, productId: ProductId1, list: (decimal)list, sale: sale);
            var productPrice = CreateProductPrice(ProductId1, price);

            //Act
            var result = await _controller.UpdateProductPrices(productPrice);

            //Assert
            result.Should().BeOfType<NoContentResult>();
            _savedPrices.Should().ContainSingle();
        }

        private static Price CreatePrice(string id, string productId, decimal list, decimal? sale = null, int minQuantity = 1)
        {
            var price = AbstractTypeFactory<Price>.TryCreateInstance();
            price.Id = id;
            price.ProductId = productId;
            price.PricelistId = PricelistId;
            price.List = list;
            price.Sale = sale;
            price.MinQuantity = minQuantity;

            return price;
        }

        private static ProductPrice CreateProductPrice(string productId, params Price[] prices)
        {
            var productPrice = AbstractTypeFactory<ProductPrice>.TryCreateInstance();
            productPrice.ProductId = productId;
            productPrice.Prices = prices.ToList();

            return productPrice;
        }
    }
}
