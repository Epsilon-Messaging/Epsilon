using Serilog;
using static Epsilon.EnvironmentVariables;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{ASPNETCORE_ENVIRONMENT}.json", true, true);

builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

await app.RunAsync();