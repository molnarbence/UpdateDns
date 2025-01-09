using System.Diagnostics;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace ConsoleApp;

[Command("update-dns", Description = "Update the DNS record for a domain.")]
internal class UpdateDnsCliApplication
{
   [Argument(0, "domain", "The domain to update.")]
   private string Domain { get; } = string.Empty;

   [Argument(1, "name", "The name of the record to update.")]
   private string Name { get; } = string.Empty;

   public async Task<int> OnExecuteAsync(
      ILogger<UpdateDnsCliApplication> logger,
      UpdateDnsMetrics metrics,
      IPublicIpAddressResolver publicIpAddressResolver, 
      IDnsRecordsService dnsRecordsService, 
      ISlackNotifications slackNotifications)
   {
      ActivitySource activitySource = new("UpdateDns");
      using var activity = activitySource.StartActivity();
      Stopwatch sw = Stopwatch.StartNew();
      try
      {
         // Get the public IP address
         logger.LogInformation("Getting public IP address...");
         var publicIpAddress = await publicIpAddressResolver.GetPublicIpAddressAsync();
         logger.LogInformation("Public IP address is '{IpAddress}'", publicIpAddress);

         // Get the current address of the DNS record
         logger.LogInformation("Getting current address of the DNS record...");
         var addressInDnsRecord = await dnsRecordsService.GetCurrentAddressAsync(Domain, Name);
         logger.LogInformation("Current address is '{IpAddress}'", addressInDnsRecord);

         // If the alias is already set to the IP address, then we're done
         if (addressInDnsRecord == publicIpAddress)
         {
            logger.LogInformation("Address is already set to the current IP address");
            metrics.DnsRemainsUnchanged();
            return 0;
         }

         // Call the domain registrar
         logger.LogInformation("Updating address...");
         await dnsRecordsService.UpdateRecordAsync(Domain, Name, publicIpAddress);
         logger.LogInformation("Address updated.");
         metrics.DnsUpdated();
         
         // Send a notification
         await slackNotifications.SendNotificationAsync(new SlackMessage($"DNS record for {Name}.{Domain} updated from {addressInDnsRecord} to {publicIpAddress}"));   
         
         return 0;
      }
      catch (Exception ex)
      {
         logger.LogError(ex, "An error occurred");
         metrics.DnsUpdateFailed();
         return -1;
      }
      finally
      {
         sw.Stop();
         metrics.DnsUpdateDuration(sw.ElapsedMilliseconds);
         logger.LogInformation("Execution time: {Elapsed}", sw.Elapsed);
      }
   }
}