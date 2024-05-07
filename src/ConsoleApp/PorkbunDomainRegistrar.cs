using Microsoft.Extensions.Options;
using AutoCtor;

namespace ConsoleApp;

[AutoConstruct]
public partial class PorkbunDomainRegistrar : IDomainRegistrar
{
   private readonly IPorkbunApi _porkbunApi;
   private readonly IOptions<PorkbunApiConfiguration> _options;

   public async Task<string> GetCurrentAddressAsync(string domain, string name)
   {
      var getDnsRecordsByNameAndTypeRequest = new GetDnsRecordsByNameAndTypeRequest
      {
         Apikey = _options.Value.ApiKey,
         Secretapikey = _options.Value.SecretKey
      };
      var getDnsRecordsByNameAndTypeResponse = await _porkbunApi.GetDnsRecordsByNameAndTypeAsync(getDnsRecordsByNameAndTypeRequest, domain, "A", name);
      var currentDomainIpAddress = getDnsRecordsByNameAndTypeResponse.Records.FirstOrDefault()?.Content;
      return currentDomainIpAddress ?? string.Empty;
   }

   public async Task UpdateAddressAsync(string domain, string name, string address)
   {
      var updateDnsRecordRequest = new UpdateDnsRecordRequest
      {
         ApiKey = _options.Value.ApiKey,
         SecretApiKey = _options.Value.SecretKey,
         Content = address,
         Ttl = "600"
      };
      await _porkbunApi.UpdateDnsRecordAsync(updateDnsRecordRequest, domain, "A", name);
   }
}
