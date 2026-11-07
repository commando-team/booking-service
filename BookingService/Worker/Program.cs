using MassTransit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Worker.Services;
using Worker.Workers;

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Mongo") 
                       ?? builder.Configuration["ConnectionStrings:Mongo"] 
                       ?? "mongodb://localhost:27017";

builder.Services.AddSingleton<IMongoDatabase>(_ => new MongoClient(connectionString).GetDatabase("booking-service"));

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.UseMessageRetry(r => r.Immediate(2));
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddSingleton<ILeaseLockService, MongoLeaseLockService>();
builder.Services.AddHostedService<ExpiredBookingsWorker>();

var app = builder.Build();

app.MapGet("/", () => "Booking Worker is running");

app.Run();