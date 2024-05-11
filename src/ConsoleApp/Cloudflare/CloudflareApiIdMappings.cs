using CloudflareApi.Client;

namespace ConsoleApp.Cloudflare;
internal class CloudflareApiIdMappings(IZonesApi zonesApi) : IIdMappings
{
   private readonly IZonesApi _zonesApi = zonesApi;

   public async Task<string?> GetZoneId(string zoneName)
   {
      var getZonesResponse = await _zonesApi.GetZonesAsync();
      var zone = getZonesResponse.Result.FirstOrDefault(z => z.Name == zoneName);
      return zone?.Id;
   }

   public async Task<string?> GetDnsRecordId(string zoneId, string recordName)
   {
      var getDnsRecordsResponse = await _zonesApi.GetDnsRecordsAsync(zoneId);
      var dnsRecord = getDnsRecordsResponse.Result.FirstOrDefault(r => r.Name.StartsWith($"{recordName}.") && r.Type == "A");
      return dnsRecord?.Id;
   }
}
