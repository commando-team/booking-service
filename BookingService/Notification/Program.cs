using MassTransit;
using Notification.Hubs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddMeter("MassTransit")
        .AddMeter("System.Runtime")
        .AddPrometheusExporter()
    );

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

app.MapPrometheusScrapingEndpoint();

app.MapHub<NotificationHub>("/notification");

app.Run();