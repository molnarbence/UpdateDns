using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConsoleApp;

internal class UpdateDnsJob(ILogger<UpdateDnsJob> logger, 
   UpdateDnsMetrics metrics,
   IPublicIpAddressResolver publicIpAddressResolver, 
   IDnsRecordsService dnsRecordsService, 
   ISlackNotifications slackNotifications,
   IOptions<AppConfiguration> appConfiguration)
{
   public async Task ExecuteAsync()
   {
      ActivitySource activitySource = new("UpdateDns");
      using var activity = activitySource.StartActivity();
      Stopwatch sw = Stopwatch.StartNew();

      var domain = appConfiguration.Value.Domain;
      var name = appConfiguration.Value.Name;
      
      try
      {
         // Get the public IP address
         logger.LogInformation("Getting public IP address...");
         var publicIpAddress = await publicIpAddressResolver.GetPublicIpAddressAsync();
         logger.LogInformation("Public IP address is '{IpAddress}'", publicIpAddress);

         // Get the current address of the DNS record
         logger.LogInformation("Getting current address of the DNS record...");
         var addressInDnsRecord = await dnsRecordsService.GetCurrentAddressAsync(domain, name);
         logger.LogInformation("Current address is '{IpAddress}'", addressInDnsRecord);

         // If the alias is already set to the IP address, then we're done
         if (addressInDnsRecord == publicIpAddress)
         {
            logger.LogInformation("Address is already set to the current IP address");
            metrics.DnsRemainsUnchanged();
            return;
         }

         // Call the domain registrar
         logger.LogInformation("Updating address...");
         await dnsRecordsService.UpdateRecordAsync(domain, name, publicIpAddress);
         logger.LogInformation("Address updated.");
         metrics.DnsUpdated();
         
         // Send a notification
         await slackNotifications.SendNotificationAsync(new SlackMessage($"DNS record for {name}.{domain} updated from {addressInDnsRecord} to {publicIpAddress}"));   
      }
      catch (Exception ex)
      {
         logger.LogError(ex, "An error occurred");
         metrics.DnsUpdateFailed();
      }
      finally
      {
         sw.Stop();
         metrics.DnsUpdateDuration(sw.ElapsedMilliseconds);
         logger.LogInformation("Execution time: {Elapsed}", sw.Elapsed);
      }
   }
}