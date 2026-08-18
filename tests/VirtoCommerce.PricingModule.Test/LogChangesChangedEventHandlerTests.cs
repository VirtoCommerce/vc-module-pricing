using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core.Events;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Data.Handlers;
using VirtoCommerce.PricingModule.Data.Jobs;
using Xunit;

namespace VirtoCommerce.PricingModule.Test
{
    // Any other test class that enqueues through the static BackgroundJob facade must join this collection:
    // the facade has no reset API (Initialize rejects null), so Dispose leaves a DISPOSED provider behind in
    // the static, and a class racing this one would see ObjectDisposedException from it.
    [Collection(nameof(LogChangesChangedEventHandlerTests))]
    [Trait("Category", "CI")]
    public class LogChangesChangedEventHandlerTests
    {
        [Fact]
        public async Task Handle_LoggingEnabled_EnqueuesOneJobWithOneLogPerChangedEntry()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var lastModifiedDateTime = new Mock<ILastModifiedDateTime>();
            var handler = new LogChangesChangedEventHandler(Mock.Of<IChangeLogService>(), lastModifiedDateTime.Object, CreateSettingsManager(logPricingChanges: true));

            var message = new PriceChangedEvent(
            [
                new GenericChangedEntry<Price>(new Price { Id = "price1" }, new Price { Id = "price1" }, EntryState.Modified),
                new GenericChangedEntry<Price>(new Price { Id = "price2" }, new Price { Id = "price2" }, EntryState.Added),
            ]);

            //Act
            await handler.Handle(message);

            //Assert
            Assert.Equal(1, capture.EnqueueCount);
            Assert.Equal(2, capture.Payload.OperationLogs.Length);
            Assert.Equal(["price1", "price2"], capture.Payload.OperationLogs.Select(x => x.ObjectId));
            Assert.Equal([EntryState.Modified, EntryState.Added], capture.Payload.OperationLogs.Select(x => x.OperationType));

            // The job's SaveChangesAsync ends in Reset(), so the handler must not reset the date itself as well.
            lastModifiedDateTime.Verify(x => x.Reset(), Times.Never);
        }

        [Fact]
        public async Task Handle_LoggingDisabled_ResetsLastModifiedInsteadOfEnqueuing()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var lastModifiedDateTime = new Mock<ILastModifiedDateTime>();
            var handler = new LogChangesChangedEventHandler(Mock.Of<IChangeLogService>(), lastModifiedDateTime.Object, CreateSettingsManager(logPricingChanges: false));

            var price = new Price { Id = "price1" };

            //Act
            await handler.Handle(new PriceChangedEvent([new GenericChangedEntry<Price>(price, price, EntryState.Modified)]));

            //Assert
            Assert.Equal(0, capture.EnqueueCount);
            lastModifiedDateTime.Verify(x => x.Reset(), Times.Once);
        }

        [Fact]
        public async Task Handle_LoggingEnabled_NoChangedEntries_StillEnqueues()
        {
            //Arrange
            // Unlike other modules, an empty batch is NOT skipped here: SaveChangesAsync ends in Reset(), and the
            // disabled branch above shows that resetting the last-modified date on every price change is intended.
            using var capture = new EnqueueCapture();
            var handler = new LogChangesChangedEventHandler(Mock.Of<IChangeLogService>(), Mock.Of<ILastModifiedDateTime>(), CreateSettingsManager(logPricingChanges: true));

            //Act
            await handler.Handle(new PriceChangedEvent([]));

            //Assert
            Assert.Equal(1, capture.EnqueueCount);
            Assert.Empty(capture.Payload.OperationLogs);
        }

        [Fact]
        public async Task LogEntityChangesJobHandler_SavesThePayloadLogs()
        {
            //Arrange
            var operationLogs = new[] { AbstractTypeFactory<OperationLog>.TryCreateInstance() };
            var changeLogServiceMock = new Mock<IChangeLogService>();

            var handler = new LogEntityChangesJobHandler(changeLogServiceMock.Object);

            //Act
            await handler.Execute(new LogEntityChangesJobPayload { OperationLogs = operationLogs }, context: null,
                TestContext.Current.CancellationToken);

            //Assert
            changeLogServiceMock.Verify(x => x.SaveChangesAsync(operationLogs), Times.Once);
        }

        [Fact]
        public void LogEntityChangesInBackground_StillSavesForLegacyHangfireJobs()
        {
            //Arrange
            // Hangfire stores a queued job as type + method name + parameter types + serialized args, so a store
            // written by an earlier version can still invoke this method. Deleting it would strand those as Failed.
            var operationLogs = new[] { AbstractTypeFactory<OperationLog>.TryCreateInstance() };
            var changeLogServiceMock = new Mock<IChangeLogService>();

            var handler = new LogChangesChangedEventHandler(changeLogServiceMock.Object, Mock.Of<ILastModifiedDateTime>(), Mock.Of<ISettingsManager>());

            //Act
#pragma warning disable VC0015
            handler.LogEntityChangesInBackground(operationLogs);
#pragma warning restore VC0015

            //Assert
            changeLogServiceMock.Verify(x => x.SaveChangesAsync(operationLogs), Times.Once);
        }

        private static ISettingsManager CreateSettingsManager(bool logPricingChanges)
        {
            var settingsManager = new Mock<ISettingsManager>();
            settingsManager
                .Setup(x => x.GetObjectSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ObjectSettingEntry { Value = logPricingChanges });

            return settingsManager.Object;
        }

        // Captures what the handler enqueued through the static BackgroundJob facade. IBackgroundJob is registered
        // Scoped here exactly as the engine module registers it, so this also proves the facade's per-call scope
        // resolves it - the handler itself is root-resolved and must never hold it.
        private sealed class EnqueueCapture : IDisposable
        {
            private readonly ServiceProvider _provider;

            public EnqueueCapture()
            {
                BackgroundJobMock
                    .Setup(x => x.Enqueue<LogEntityChangesJobHandler>(It.IsAny<object>(), It.IsAny<EnqueueOptions>(), It.IsAny<CancellationToken>()))
                    .Callback<object, EnqueueOptions, CancellationToken>((payload, _, _) =>
                    {
                        Payload = (LogEntityChangesJobPayload)payload;
                        EnqueueCount++;
                    })
                    .ReturnsAsync("job-id");

                var services = new ServiceCollection();
                services.AddScoped(_ => BackgroundJobMock.Object);
                _provider = services.BuildServiceProvider(validateScopes: true);

                BackgroundJob.Initialize(_provider);
            }

            public Mock<IBackgroundJob> BackgroundJobMock { get; } = new();

            public LogEntityChangesJobPayload Payload { get; private set; }

            public int EnqueueCount { get; private set; }

            public void Dispose()
            {
                _provider.Dispose();
            }
        }
    }
}
