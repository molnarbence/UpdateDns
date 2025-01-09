using ConsoleApp;
using MbUtils.Extensions.CommandLineUtils;
using Microsft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Extensions.Http;
using Refit;

var wrapper = new CommandLineApplicationWrapper<UpdateDnsCliApplication>(args);

var appDataFolder = GetAppDataFolder();
if (!Directory.Exists(appDataFolder)) Directory.CreateDirectory(appDataFolder);

wrapper.HostBuilder.ConfigureAppConfiguration((context, builder) => {
   builder.AddJsonFile(Path.Combine(appDataFolder, "config.json"), optional: true);
   builder.AddInMemoryCollection([
      new KeyValuePair<string, string?>("AppDataFolder", appDataFolder),
   ]);
});

wrapper.HostBuilder.ConfigureServices((hostBuilderContext, services) =>
{
   AppConfiguration appConfig =
      hostBuilderContext.Configuration.Get<AppConfiguration>() ?? new AppConfiguration();

   services
      .AddRefitClient<IPublicIpAddressResolver>()
      .ConfigureHttpClient(client => client.BaseAddress = new Uri(appConfig.PublicIpAddressResolverBaseUrl));
         
   services.AddRefitClient<ISlackNotifications>()
      .ConfigureHttpClient(client => client.BaseAddress = new Uri(appConfig.Slack.WebhookUrl));

   var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt * 10));

   services
      .AddCloudflareApi(hostBuilderContext.Configuration, httpClientBuilder => httpClientBuilder.AddPolicyHandler(retryPolicy))
      .AutoRegisterFromConsoleApp();

   services.AddOptions<AppConfiguration>()
      .Bind(hostBuilderContext.Configuration);

   services.AddOpenTelemetry()
      .ConfigureResource(builder => builder.AddService("UpdateDns"))
      .WithTracing(builder => builder.AddHttpClientInstrumentation().AddOtlpExporter())
      .WithMetrics(builder => builder
         .AddMeter("UpdateDns")
         .AddOtlpExporter()
         .AddHttpClientInstrumentation());

});

wrapper.HostBuilder.UseSerilogWithOpenTelemetry();

return await wrapper.ExecuteAsync();

static string GetAppDataFolder()
{
   var homeFolder = Environment.GetEnvironmentVariable("HOME")
                    ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
   return Path.Combine(homeFolder, ".update-dns");
}