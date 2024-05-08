using Refit;

namespace ConsoleApp;

public interface IPorkbunApi
{
   [Post("/ping")]
   Task<PingResponse> PingAsync([Body] PingRequest body);

   [Post("/dns/retrieveByNameType/{domain}/{type}/{subdomain}")]
   Task<GetDnsRecordsByNameAndTypeResponse> GetDnsRecordsByNameAndTypeAsync([Body] GetDnsRecordsByNameAndTypeRequest body, string domain, string type, string subdomain);

   [Post("/dns/editByNameType/{domain}/{type}/{subdomain}")]
   Task<UpdateDnsRecordResponse> UpdateDnsRecordAsync([Body] UpdateDnsRecordRequest request, string domain, string type, string subdomain);
}

public abstract class PorkbunRequestBase
{
   public required string Secretapikey { get; set; }
   public required string Apikey { get; set; }
}

public abstract class PorkbunResponseBase
{
   public string Status { get; set; } = string.Empty;
}

public class PingRequest : PorkbunRequestBase
{
}
public class PingResponse : PorkbunResponseBase
{
   public required string YourIp { get; set; }
}

public class GetDnsRecordsByNameAndTypeRequest : PorkbunRequestBase
{
}

public class GetDnsRecordsByNameAndTypeResponse : PorkbunResponseBase
{
   public IEnumerable<DnsRecord> Records { get; set; } = [];

}

public class DnsRecord
{
   public string Id { get; set; } = string.Empty;
   public string Name { get; set; } = string.Empty;
   public string Type { get; set; } = string.Empty;
   public string Content { get; set; } = string.Empty;
   public string Ttl { get; set; } = string.Empty;
   public string Prio { get; set; } = string.Empty;
   public string Notes { get; set; } = string.Empty;
}

public class UpdateDnsRecordRequest : PorkbunRequestBase
{
   public required string Content { get; set; }
   public required string Ttl { get; set; }
}

public class UpdateDnsRecordResponse : PorkbunResponseBase
{
}