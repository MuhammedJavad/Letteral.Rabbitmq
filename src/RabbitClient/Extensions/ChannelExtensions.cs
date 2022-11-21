using RabbitClient.Contracts;
using RabbitMQ.Client;

namespace RabbitClient.Extensions;

static class ChannelExtensions
{
    public static void DeclarePath(this IModel channel, RabbitOptions options, AmqpModel amqp)
    {
        channel.DeclareExchange(options, amqp);
        channel.DeclareQueue(options, amqp);
        channel.BindQueue(amqp);
    }

    public static void DeclareExchange(this IModel channel, RabbitOptions options, AmqpModel amqp)
    {
        channel.ExchangeDeclare(
            exchange: amqp.GetExchangeName(),
            type: amqp.GetExchangeType(),
            durable: options.Durable,
            autoDelete: options.AutoDelete,
            amqp.ExchangeParameters());
    }

    public static void DeclareQueue(this IModel channel, RabbitOptions options, AmqpModel amqp)
    {
        channel.QueueDeclare(
            amqp.GetQueueName(),
            durable: options.Durable,
            exclusive: options.Exclusive,
            autoDelete: options.AutoDelete,
            amqp.QueueParameters());
    }

    public static void BindQueue(this IModel channel, AmqpModel amqp)
    {
        channel.QueueBind(
            amqp.GetQueueName(),
            amqp.GetExchangeName(),
            amqp.GetRoutingKey());
    }
}