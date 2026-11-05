using MassTransit;
using Notification.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        cfg.Host(rabbitHost, "/", h => {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.UseMessageRetry(r => r.Immediate(2));
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.UseRouting();

app.MapHub<NotificationHub>("/notification");

app.Run();