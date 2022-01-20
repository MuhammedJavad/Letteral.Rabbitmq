using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Letteral.Rabbitmq.Contracts;
using Letteral.Rabbitmq.IntegrationTest.Common;
using Letteral.Rabbitmq.IntegrationTest.TestMessages;
using Xunit;

namespace Letteral.Rabbitmq.IntegrationTest.PublishAndSubscribe
{
    public class MultipleOperationTest : TestBase
    {
        [Theory]
        [InlineData(20)]
        public async Task ConcurrentPublish_WithSameSubscribers_ShouldCountsAllHandlers(int count)
        {
            //On
            using var scope = CreateScope();
            var tcs = new TaskCompletionSource<int>();
            scope.AddService<CounterMessageHandler>(tcs, new HandlerHitCounter());
            var bus = scope.GetService<IRabbit>();
            //When
            var metadata = AmqpModel.FanOut("fanout-test", nameof(CounterMessage));
            bus.Subscribe<CounterMessage, CounterMessageHandler>(metadata);
            var tasks = Enumerable.Range(0, count)
                .Select(i => Task.Run(() =>
                {
                    var evt = EventDocument<CounterMessage>.New(new CounterMessage(count),metadata);
                    bus.Publish(evt);
                }))
                .ToArray();
            //Assert
            ShouldCompletedIn(tasks, 6000);
            await ShouldCompletedIn(tcs, 6000);
            tcs.Task.Result.Should().Be(count);
        }
    }
}