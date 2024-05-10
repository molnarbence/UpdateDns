using ConsoleApp.Cloudflare;
using ConsoleApp.Cloudflare.DnsRecords;
using MbUtils.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Refit;
using Serilog;

namespace ConsoleApp;

[Command("update-dns", Description = "Update the DNS record for a domain.")]
class Program
{
   static async Task<int> Main(string[] args)
   {
      var wrapper = new CommandLineApplicationWrapper<Program>(args);

      wrapper.HostBuilder.ConfigureServices((hostBuilderContext, services) =>
      {
         services
            .AddRefitClient<IPublicIpAddressResolver>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://api.ipify.org"));

         var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt * 10));

         services
            .AddTransient<CloudflareAuthDelegatingHandler>()
            .AddRefitClient<ICloudflareApi>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://api.cloudflare.com/client/v4"))
            .AddHttpMessageHandler<CloudflareAuthDelegatingHandler>()
            .AddPolicyHandler(retryPolicy);

         services
            .AddOptions<CloudflareConfiguration>()
            .Bind(hostBuilderContext.Configuration.GetSection(CloudflareConfiguration.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
      });

      wrapper.HostBuilder.UseSerilog((context, services, configuration) =>
                  configuration.WriteTo.Console().WriteTo.File(context.Configuration.GetValue<string>("Serilog:LogFilePath") ?? "log.txt", rollingInterval: RollingInterval.Day)
                  .MinimumLevel.Information()
                  .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Warning)
            );

      return await wrapper.ExecuteAsync();
   }

   [Argument(0, "domain", "The domain to update.")]
   public string Domain { get; set; } = string.Empty;

   [Argument(1, "name", "The name of the record to update.")]
   public string Name { get; set; } = string.Empty;

   public async Task<int> OnExecuteAsync(ILogger<Program> logger, IPublicIpAddressResolver publicIpAddressResolver, ICloudflareApi cloudflareApi, IOptions<CloudflareConfiguration> options)
   {
      try
      {
         // Get the public IP address
         logger.LogInformation("Getting public IP address...");
         var publicIpAddress = await publicIpAddressResolver.GetPublicIpAddressAsync();
         logger.LogInformation("Public IP address is '{IpAddress}'", publicIpAddress);

         // Get the current alias
         logger.LogInformation("Getting current address of the DNS record...");
         var getDnsRecordResponse = await cloudflareApi.GetDnsRecordAsync(options.Value.ZoneId, options.Value.RecordId);
         var currentDomainIpAddress = getDnsRecordResponse.Result.Content;
         logger.LogInformation("Current address is '{IpAddress}'", currentDomainIpAddress);

         // If the alias is already set to the IP address, then we're done
         if (currentDomainIpAddress == publicIpAddress)
         {
            logger.LogInformation("Address is already set to the current IP address");
            return 0;
         }

         // Call the domain registrar
         logger.LogInformation("Updating address...");
         await cloudflareApi.UpdateDnsRecordAsync(options.Value.ZoneId, options.Value.RecordId, new DnsRecordInfo
         {
            Name = Name,
            Content = publicIpAddress,
            Type = "A"
         });
         logger.LogInformation("Address updated.");
         return 0;
      }
      catch (Exception ex)
      {
         logger.LogError(ex, "An error occurred");
         return -1;
      }
      
   }
}
