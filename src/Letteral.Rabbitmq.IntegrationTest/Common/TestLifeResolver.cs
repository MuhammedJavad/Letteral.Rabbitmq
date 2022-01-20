using System;
using Letteral.Rabbitmq.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Letteral.Rabbitmq.IntegrationTest.Common
{
    public class TestLifeResolver : IDisposable
    {
        private readonly IServiceCollection _collection;

        public TestLifeResolver(RabbitOptions options)
        {
            _collection = new ServiceCollection();
            _collection.AddScoped(typeof(ILogger<>), typeof(NullLogger<>));
            _collection.AddRabbit(rabbitOptions =>
            {
                rabbitOptions.AutoDelete = options.AutoDelete;
                rabbitOptions.CircuitBreakCount = options.CircuitBreakCount;
                rabbitOptions.Durable = options.AutoDelete;
                rabbitOptions.RabbitConnection = options.RabbitConnection;
                rabbitOptions.RetryCount = options.RetryCount;
                rabbitOptions.ServiceName = options.ServiceName;
            });
        }

        public void AddService<TService>(params object[] ctor) where TService : class
        {
            _collection.TryAddTransient(_ => Instantiate<TService>(ctor));
        }

        public void AddService<TService, TImplementation>(params object[] ctor)
            where TService : class
            where TImplementation : class, TService
        {
            _collection.AddTransient<TService, TImplementation>(_ => Instantiate<TImplementation>(ctor));
        }

        public T GetService<T>() where T : class
        {
            return Build().GetRequiredService<T>();
        }

        public IServiceProvider Build()
        {
            return _collection.BuildServiceProvider();
        }

        private T Instantiate<T>(params object[] ctor) where T : class
        {
            return (T) Activator.CreateInstance(typeof(T), ctor);
        }

        public void Dispose()
        {

        }
    }
}