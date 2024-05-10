using ConsoleApp.Cloudflare.DnsRecords;
using Refit;

namespace ConsoleApp.Cloudflare;


internal interface ICloudflareApi
{
   [Get("/zones/{zoneId}/dns_records/{dnsRecordId}")]
   Task<GetDnsRecordResponse> GetDnsRecordAsync(string zoneId, string dnsRecordId);

   [Patch("/zones/{zoneId}/dns_records/{dnsRecordId}")]
   Task<UpdateDnsRecordResponse> UpdateDnsRecordAsync(string zoneId, string dnsRecordId, [Body] DnsRecordInfo dnsRecordInfo);
}
