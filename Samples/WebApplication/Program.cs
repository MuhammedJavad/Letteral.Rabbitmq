using Microsoft.AspNetCore.Mvc;
using RabbitClient;
using RabbitClient.Contracts;
using RabbitClient.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRabbit(options =>
{
    options.HostName = "posapi.snappexpress.win";
    options.Password = "P@ssw0rdN300";
    options.UserName = "pos";
    options.AutoDelete = false;
    options.Durable = true;
    options.RetryCount = 3;
    options.UseSecondaryConnectionForConsumers = true;
});

var app = builder.Build();

app.MapGet("/publish", ([FromServices] IRabbit rabbit) =>
{
    var msg = new Message();
    var amqp = AmqpModel.DefaultExchange("MyQueueName", "MyRoutingKey");
    var evt = EventDocument<Message>.New(msg, amqp);
    return rabbit.Publish(evt);
});

app.Run();


class Message
{
    public Message()
    {
        Id = Guid.NewGuid();
    }
    
    public Guid Id { get; set; }
}