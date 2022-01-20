using System;
using System.Threading.Tasks;
using FluentAssertions;
using Letteral.Rabbitmq.Contracts;
using Letteral.Rabbitmq.IntegrationTest.Common;
using Letteral.Rabbitmq.IntegrationTest.TestMessages;
using Xunit;

namespace Letteral.Rabbitmq.IntegrationTest.PublishAndSubscribe
{
    public class SubscriptionTest : TestBase
    {
        [Fact]
        public async Task FanOutPublishSubscribe_ShouldCallHandler()
        {
            //Given
            using var scope = CreateScope();
            var tcs = new TaskCompletionSource<BasicMessage>();
            scope.AddService<BasicMessageHandler>(tcs);
            var bus = scope.GetService<IRabbit>();
            var metaData = AmqpModel.FanOut("fanout-exchange", Guid.NewGuid().ToString("N"));
            bus.Subscribe<BasicMessage, BasicMessageHandler>(metaData);
            //When
            var evt = EventDocument<BasicMessage>.New(BasicMessage.New(), metaData);
            bus.Publish(evt);
            //Assert
            await ShouldCompletedIn(tcs, 500);
            tcs.Task.Result.Id.Should().Be(evt.Id);
        }

        [Fact]
        public async Task DirectPublishSubscribe_ShouldCallHandler()
        {
            //Given
            using var scope = CreateScope();
            var tcs = new TaskCompletionSource<BasicMessage>();
            scope.AddService<BasicMessageHandler>(tcs);
            var bus = scope.GetService<IRabbit>();
            var metaData = AmqpModel.Direct("direct-test", Guid.NewGuid().ToString("N"));
            bus.Subscribe<BasicMessage, BasicMessageHandler>(metaData);
            //When
            var evt = EventDocument<BasicMessage>.New(BasicMessage.New(), metaData);
            bus.Publish(evt);
            //Assert
            await ShouldCompletedIn(tcs, 500);
            tcs.Task.Result.Id.Should().Be(evt.Id);
        }
    }
}