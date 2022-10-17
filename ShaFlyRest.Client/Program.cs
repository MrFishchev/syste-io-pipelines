using ShaFlyRest.Core.Pipelines;
using ShaFlyRest.Core.Streams;
using ShaFlyRest.Core.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddScoped<IPipelinesService, PipelinesService>();
builder.Services.AddScoped<IStreamService, StreamService>();
builder.Services.AddScoped<IWebSocketService, WebSocketService>();

var app = builder.Build();

app.MapControllers();

app.Run();