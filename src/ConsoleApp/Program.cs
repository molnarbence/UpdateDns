using MbUtils.Extensions.Tools;
using Microsft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Extensions.Http;
using Refit;
using Serilog;
using ConsoleApp;

var builder = Host.CreateApplicationBuilder(args);

builder.AddHomeFolderConfigurationFile(".update-dns");

var services = builder.Services;
var configuration = builder.Configuration;
var appConfig = builder.Configuration.Get<AppConfiguration>() ?? new AppConfiguration();


services.AddSerilog(loggerConfiguration => loggerConfiguration.ReadFrom.Configuration(configuration));

services
   .AddOptions<AppConfiguration>()
   .Bind(configuration)
   .ValidateOnStart();

services.AddSingleton<UpdateDnsJob>();

services
   .AddRefitClient<IPublicIpAddressResolver>()
   .ConfigureHttpClient(client => client.BaseAddress = new Uri(appConfig.PublicIpAddressResolverBaseUrl));
         
services.AddRefitClient<ISlackNotifications>()
   .ConfigureHttpClient(client => client.BaseAddress = new Uri(appConfig.Slack.WebhookUrl));

var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt * 10));

services
   .AddCloudflareApi(configuration, httpClientBuilder => httpClientBuilder.AddPolicyHandler(retryPolicy))
   .AutoRegisterFromConsoleApp();

services.AddOpenTelemetry()
   .ConfigureResource(resourceBuilder => resourceBuilder.AddService("UpdateDns"))
   .WithTracing(tracerProviderBuilder => tracerProviderBuilder
      .AddHttpClientInstrumentation()
      .AddSource("UpdateDns")
      .AddOtlpExporter())
   .WithMetrics(meterProviderBuilder => meterProviderBuilder
      .AddMeter("UpdateDns")
      .AddOtlpExporter()
      .AddHttpClientInstrumentation());

using var host = builder.Build();


try
{
   var runner = host.Services.GetRequiredService<UpdateDnsJob>();
   
   await runner.ExecuteAsync();

   return 0; // success
}
catch (Exception ex)
{
   var logger = host.Services.GetRequiredService<ILogger<Program>>();
   logger.LogError(ex, "Stopped program because of exception");

   return 1; // failure
}