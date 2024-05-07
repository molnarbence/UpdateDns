using MbUtils.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

         services
            .AddRefitClient<IPorkbunApi>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://api-ipv4.porkbun.com/api/json/v3"));

         services.AddSingleton<IDomainRegistrar, PorkbunDomainRegistrar>();

         services
            .AddOptions<PorkbunApiConfiguration>()
            .Bind(hostBuilderContext.Configuration.GetSection(PorkbunApiConfiguration.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
      });

      wrapper.HostBuilder.UseSerilog((context, services, configuration) =>
                  configuration.ReadFrom.Configuration(context.Configuration)
            );

      return await wrapper.ExecuteAsync();
   }

   [Argument(0, "domain", "The domain to update.")]
   public string Domain { get; set; } = string.Empty;

   [Argument(1, "name", "The name of the record to update.")]
   public string Name { get; set; } = string.Empty;

   public async Task OnExecuteAsync(ILogger<Program> logger, IPublicIpAddressResolver publicIpAddressResolver, IDomainRegistrar domainRegistrar)
   {
      // Get the public IP address
      logger.LogInformation("Getting public IP address...");
      var publicIpAddress = await publicIpAddressResolver.GetPublicIpAddressAsync();
      logger.LogInformation("Public IP address is '{IpAddress}'", publicIpAddress);

      // Get the current alias
      logger.LogInformation("Getting current address of the DNS record...");
      var currentDomainIpAddress = await domainRegistrar.GetCurrentAddressAsync(Domain, Name);
      logger.LogInformation("Current address is '{IpAddress}'", currentDomainIpAddress);

      // If the alias is already set to the IP address, then we're done
      if (currentDomainIpAddress == publicIpAddress)
      {
         logger.LogInformation("Address is already set to the current IP address");
         return;
      }

      // Call the domain registrar
      logger.LogInformation("Updating address...");
      await domainRegistrar.UpdateAddressAsync(Domain, Name, publicIpAddress);
      logger.LogInformation("Alias updated.");
   }
}
