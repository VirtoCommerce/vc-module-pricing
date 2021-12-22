using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Services;
using Xunit;

namespace VirtoCommerce.PricingModule.Test
{
    public class PricingEvaluatorServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPricingRepository> _repositoryMock;
        private readonly Mock<IItemService> _productServiceMock;
        private readonly Mock<ILogger<PricingEvaluatorService>> _loggerMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly Mock<IPricingPriorityFilterPolicy> _pricingPriorityFilterPolicyMock;

        public PricingEvaluatorServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _repositoryMock = new Mock<IPricingRepository>();
            _productServiceMock = new Mock<IItemService>();
            _loggerMock = new Mock<ILogger<PricingEvaluatorService>>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _pricingPriorityFilterPolicyMock = new Mock<IPricingPriorityFilterPolicy>();
        }

        private List<PricelistAssignmentEntity> _assignments = new List<PricelistAssignmentEntity>
        {
            new PricelistAssignmentEntity
            {
                CatalogId = "TestCatalog_1",
            },
            new PricelistAssignmentEntity
            {
                CatalogId = "TestCatalog_2",
            },
            new PricelistAssignmentEntity
            {
                StoreId = "TestStore_1",
            },
            new PricelistAssignmentEntity
            {
                StoreId = "TestStore_2",
            }
        };

        [Fact]
        public async Task PriceListAssignmentAsync_EmptyContext_AllAssignmentsReturned()
        {
            // Arrange
            var target = GetPricingEvaluatorService(_assignments);

            var context = new PriceEvaluationContext();

            // Act
            var result = await target.PriceListAssignmentAsync(context);

            // Assert
            var assignments = result.ToList();
            assignments.Should().HaveCount(_assignments.Count);
            assignments.Should().ContainSingle(x => x.StoreId == "TestStore_1");
            assignments.Should().ContainSingle(x => x.StoreId == "TestStore_2");
            assignments.Should().ContainSingle(x => x.CatalogId == "TestCatalog_1");
            assignments.Should().ContainSingle(x => x.CatalogId == "TestCatalog_2");
        }

        [Fact]
        public async Task PriceListAssignmentAsync_HasStoreInContext_StoreAssignmentsReturned()
        {
            // Arrange
            var target = GetPricingEvaluatorService(_assignments);

            var context = new PriceEvaluationContext()
            {
                StoreId = "TestStore_1",
            };

            // Act
            var result = await target.PriceListAssignmentAsync(context);

            // Assert
            var assignments = result.ToList();
            assignments.Should().HaveCount(1);
            assignments.Should().ContainSingle(x => x.StoreId == "TestStore_1");
        }

        [Fact]
        public async Task PriceListAssignmentAsync_HasCatalogInContext_CatalogAssignmentsReturned()
        {
            // Arrange
            var target = GetPricingEvaluatorService(_assignments);

            var context = new PriceEvaluationContext()
            {
                CatalogId = "TestCatalog_1",
            };

            // Act
            var result = await target.PriceListAssignmentAsync(context);

            // Assert
            var assignments = result.ToList();
            assignments.Should().HaveCount(1);
            assignments.Should().ContainSingle(x => x.CatalogId == "TestCatalog_1");
        }

        [Fact]
        public async Task PriceListAssignmentAsync_HasStoreAndCatalogInContext_SelectedAssignmentsReturned()
        {
            // Arrange
            var target = GetPricingEvaluatorService(_assignments);

            var context = new PriceEvaluationContext()
            {
                StoreId = "TestStore_1",
                CatalogId = "TestCatalog_1",
            };

            // Act
            var result = await target.PriceListAssignmentAsync(context);

            // Assert
            var assignments = result.ToList();
            assignments.Should().HaveCount(2);
            assignments.Should().ContainSingle(x => x.StoreId == "TestStore_1");
            assignments.Should().ContainSingle(x => x.CatalogId == "TestCatalog_1");
        }

        private PricingEvaluatorService GetPricingEvaluatorService(List<PricelistAssignmentEntity> assignments)
        {
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);

            // Add mock repo with assignemnts
            _repositoryMock.Setup(ss => ss.UnitOfWork).Returns(_unitOfWorkMock.Object);
            _repositoryMock.Setup(foo => foo.PricelistAssignments).Returns(new MockAsyncEnumerable<PricelistAssignmentEntity>(assignments));
            var pricingRepository = _repositoryMock.Object;

            var service = new PricingEvaluatorService(
                () => pricingRepository,
                _productServiceMock.Object,
                _loggerMock.Object,
                platformMemoryCache,
                _pricingPriorityFilterPolicyMock.Object);

            return service;
        }

        /// <summary>
        /// Mock enumerable implementation that implements both IQueryable and IAsyncEnumerable
        /// since methods such ToListAsync() will fail on regular Lists
        /// </summary>
        class MockAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            private readonly IAsyncEnumerable<T> _inner;

            public MockAsyncEnumerable(IEnumerable<T> enumerable)
                : base(enumerable)
            {
                _inner = enumerable.ToAsyncEnumerable();
            }

            public MockAsyncEnumerable(Expression expression)
                : base(expression)
            {
            }

            public IAsyncEnumerator<T> GetEnumerator()
            {
                return _inner.GetAsyncEnumerator();
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return _inner.GetAsyncEnumerator();
            }
        }
    }
}
