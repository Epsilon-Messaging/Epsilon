using Epsilon;
using Epsilon.Handler.WebsocketMessageHandler;
using Epsilon.Models;
using Epsilon.Services.WebsocketStateService;
using Serilog;
using static Epsilon.EnvironmentVariables;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{ASPNETCORE_ENVIRONMENT}.json", true, true)
    .AddEnvironmentVariables();

builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddTransient<IMessageHandler<string>, WebsocketMessageHandler>();
builder.Services.AddTransient<IMessageHandler<LoginRequest>, LoginRequestMessageHandler>();
builder.Services.AddTransient<IMessageHandler<MessageRequest>, MessageRequestMessageHandler>();
builder.Services.AddTransient<IMessageHandler<ChallengeRequest>, ChallengeRequestMessageHandler>();
builder.Services.AddSingleton<IWebsocketStateService, WebsocketStateService>();

builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapControllers();
app.MapHealthChecks("/health");

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30),
});

await app.RunAsync();