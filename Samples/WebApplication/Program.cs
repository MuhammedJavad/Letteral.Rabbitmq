using Letteral.Rabbitmq;
using Letteral.Rabbitmq.Contracts;
using Letteral.Rabbitmq.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using WebApplication;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

builder.Services.AddRabbit(options =>
{
    options.HostName = "***";
    options.Password = "***";
    options.UserName = "***";
    options.AutoDelete = false;
    options.Durable = true;
    options.UseSecondaryConnectionForConsumers = true;
});

var app = builder.Build();

app.MapGet("/publish", ([FromServices] IRabbit rabbit) =>
{
    var msg = new Message();
    var amqp = AmqpModel.DefaultExchange("MyQueueName", "MyRoutingKey");
    var evt = EventDocument<Message>.New(msg, amqp);
    try
    {
        rabbit.Publish(evt);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
});

app.Run();


namespace WebApplication
{
    class Message
    {
        public Message()
        {
            Id = Guid.NewGuid();
        }
    
        public Guid Id { get; set; }
    }
}