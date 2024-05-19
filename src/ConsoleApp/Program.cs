using ConsoleApp.Cloudflare;
using MbUtils.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils;
using Microsft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
         AppConfiguration appConfig = hostBuilderContext.Configuration.Get<AppConfiguration>() ?? new AppConfiguration();

         services
            .AddRefitClient<IPublicIpAddressResolver>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(appConfig.PublicIpAddressResolverBaseUrl));

         var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt * 10));

         services
            .AddCloudflareApi(hostBuilderContext.Configuration, httpClientBuilder => httpClientBuilder.AddPolicyHandler(retryPolicy))
            .AddSingleton<IDnsRecordsService, CloudflareDnsRecordsService>()
            .AddKeyedSingleton<IIdMappings, FileCachedIdMappings>("cache")
            .AddKeyedSingleton<IIdMappings, CloudflareApiIdMappings>("api");

         services.AddOptions<AppConfiguration>()
            .Bind(hostBuilderContext.Configuration);
         
      });

      wrapper.HostBuilder.UseSerilogWithOpenTelemetry();

      return await wrapper.ExecuteAsync();
   }

   private static string GetAppDataFolder()
   {
      var homeFolder = Environment.GetEnvironmentVariable("HOME")
         ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
      return Path.Combine(homeFolder, ".update-dns");
   }

   [Argument(0, "domain", "The domain to update.")]
   public string Domain { get; set; } = string.Empty;

   [Argument(1, "name", "The name of the record to update.")]
   public string Name { get; set; } = string.Empty;

   public async Task<int> OnExecuteAsync(ILogger<Program> logger, IPublicIpAddressResolver publicIpAddressResolver, IDnsRecordsService dnsRecordsService)
   {
      try
      {
         // Get the public IP address
         logger.LogInformation("Getting public IP address...");
         var publicIpAddress = await publicIpAddressResolver.GetPublicIpAddressAsync();
         logger.LogInformation("Public IP address is '{IpAddress}'", publicIpAddress);

         // Get the current address of the DNS record
         logger.LogInformation("Getting current address of the DNS record...");
         var currentDomainIpAddress = await dnsRecordsService.GetCurrentAddressAsync(Domain, Name);
         logger.LogInformation("Current address is '{IpAddress}'", currentDomainIpAddress);

         // If the alias is already set to the IP address, then we're done
         if (currentDomainIpAddress == publicIpAddress)
         {
            logger.LogInformation("Address is already set to the current IP address");
            return 0;
         }

         // Call the domain registrar
         logger.LogInformation("Updating address...");
         await dnsRecordsService.UpdateRecordAsync(Domain, Name, publicIpAddress);
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
