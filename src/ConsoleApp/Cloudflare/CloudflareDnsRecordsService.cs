using CloudflareApi.Client;
using CloudflareApi.Client.DnsRecords;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp.Cloudflare;
internal class CloudflareDnsRecordsService(IZonesApi zonesApi, [FromKeyedServices("cache")] IIdMappings idMappings) : IDnsRecordsService
{
   private readonly IZonesApi _zonesApi = zonesApi;
   private readonly IIdMappings _idMappings = idMappings;

   public async Task<string?> GetCurrentAddressAsync(string zoneName, string recordName)
   {
      var zoneId = await _idMappings.GetZoneId(zoneName);
      if (zoneId is null) return null;

      var dnsRecordId = await _idMappings.GetDnsRecordId(zoneName, recordName);
      if (dnsRecordId is null) return null;

      var getDnsRecordResponse = await _zonesApi.GetDnsRecordAsync(zoneId, dnsRecordId);

      return getDnsRecordResponse.Result.Content;
   }

   public async Task UpdateRecordAsync(string zoneName, string recordName, string newAddress)
   {
      var zoneId = await _idMappings.GetZoneId(zoneName) ?? throw new InvalidOperationException($"Zone '{zoneName}' not found.");
      var recordNameWithZoneName = $"{recordName}.{zoneName}";

      var dnsRecordId = await _idMappings.GetDnsRecordId(zoneName, recordName) ?? throw new InvalidOperationException($"DNS record '{recordName}' not found.");

      await _zonesApi.UpdateDnsRecordAsync(zoneId, dnsRecordId, new DnsRecordDto { Content = newAddress, Name = recordNameWithZoneName, Type = "A" });
   }
}
