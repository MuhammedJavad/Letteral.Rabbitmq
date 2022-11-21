using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.DependencyInjection;
using RabbitClient;

namespace RabbitMq.IntegrationTest.Common;

public abstract class TestBase
{
    protected TestLifeResolver CreateScope() => new (GetTestRabbitOption());
        
    protected async Task ShouldAnyCompletedIn<T>(TaskCompletionSource<T>[] sources, int milliseconds)
    {
        Func<Task> func = async () => await Task.WhenAny(sources.Select(x => x.Task));

        await func.Should().CompleteWithinAsync(milliseconds.Milliseconds());
    }

    protected async Task ShouldCompletedIn<T>(TaskCompletionSource<T>[] sources, int milliseconds)
    {
        Func<Task> func = async () => await Task.WhenAll(sources.Select(x => x.Task));

        await func.Should().CompleteWithinAsync(milliseconds.Milliseconds());
    }

    protected async Task ShouldCompletedIn<T>(TaskCompletionSource<T> source, int milliseconds)
    {
        Func<Task> func = async () => await source.Task;
            
        await func.Should().CompleteWithinAsync(milliseconds.Milliseconds());
    }

    protected void ShouldCompletedIn(Task[] tasks, int milliseconds)
    {
        tasks.ExecutionTimeOf(taskArray => Task.WaitAll(taskArray))
            .Should()
            .BeLessOrEqualTo(milliseconds.Milliseconds());
    }

    //protected IRabbit GetBus(IServiceScope scope)
    //{
    //    return scope.ServiceProvider.GetRequiredService<IRabbit>();
    //}

    private RabbitOptions GetTestRabbitOption()
    {
        return new()
        {
            RabbitConnection = "AMQP://guest:guest@localhost:5672",
            RetryCount = 1,
            Durable = false,
            AutoDelete = true
        };
    }
}