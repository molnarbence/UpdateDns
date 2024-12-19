using CloudflareApi.Client;

namespace ConsoleApp.Cloudflare;
internal class CloudflareApiIdMappings(IZonesApi zonesApi) : IIdMappings
{
   public async ValueTask<string?> GetZoneId(string zoneName)
   {
      var getZonesResponse = await zonesApi.GetZonesAsync();
      var zone = getZonesResponse.Result.FirstOrDefault(z => z.Name == zoneName);
      return zone?.Id;
   }

   public async ValueTask<string?> GetDnsRecordId(string zoneName, string recordName)
   {
      var zoneId = await GetZoneId(zoneName);
      if (zoneId is null) return null;

      var getDnsRecordsResponse = await zonesApi.GetDnsRecordsAsync(zoneId);
      var dnsRecord = getDnsRecordsResponse.Result.FirstOrDefault(r => r.Name.StartsWith($"{recordName}.") && r.Type == "A");
      return dnsRecord?.Id;
   }
}
