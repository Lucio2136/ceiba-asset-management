using CeibaAssets.Application;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();

using var host = builder.Build();
await host.RunAsync();
