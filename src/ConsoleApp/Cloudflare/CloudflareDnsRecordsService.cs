using CloudflareApi.Client;
using CloudflareApi.Client.DnsRecords;

namespace ConsoleApp.Cloudflare;
internal class CloudflareDnsRecordsService(IZonesApi zonesApi, IIdMappings idMappings) : IDnsRecordsService
{
   private readonly IZonesApi _zonesApi = zonesApi;
   private readonly IIdMappings _idMappings = idMappings;

   public async Task<string?> GetCurrentAddressAsync(string zoneName, string recordName)
   {
      var zoneId = await _idMappings.GetZoneId(zoneName);
      if (zoneId is null) return null;

      var dnsRecordId = await _idMappings.GetDnsRecordId(zoneId, recordName);
      if (dnsRecordId is null) return null;

      var getDnsRecordResponse = await _zonesApi.GetDnsRecordAsync(zoneId, dnsRecordId);

      return getDnsRecordResponse.Result.Content;
   }

   public async Task UpdateRecordAsync(string zoneName, string recordName, string newAddress)
   {
      var getZonesResponse = await _zonesApi.GetZonesAsync();
      var zone = getZonesResponse.Result.FirstOrDefault(z => z.Name == zoneName) ?? throw new InvalidOperationException($"Zone '{zoneName}' not found.");

      var recordNameWithZoneName = $"{recordName}.{zoneName}";

      var getDnsRecordsResponse = await _zonesApi.GetDnsRecordsAsync(zone.Id);
      var dnsRecord = getDnsRecordsResponse.Result.FirstOrDefault(r => r.Name == recordNameWithZoneName && r.Type == "A") ?? throw new InvalidOperationException($"DNS record '{recordName}.{zoneName}' not found.");

      var updateDnsRecordResponse = await _zonesApi.UpdateDnsRecordAsync(zone.Id, dnsRecord.Id, new DnsRecordDto { Content = newAddress, Name = recordNameWithZoneName, Type = "A" });
   }
}
