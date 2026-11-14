using CeibaAssets.Api.Configuration;
using CeibaAssets.Application;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.Services.AddSerilog(
    (services, logger) => logger
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services),
    preserveStaticLogger: true);

builder.Services.AddApplication();
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseApiPipeline();
app.MapApiEndpoints();

await app.RunAsync();
