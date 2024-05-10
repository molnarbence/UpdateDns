namespace ConsoleApp.Cloudflare.DnsRecords;

internal class DnsRecordInfo
{
   public required string Name { get; set; }
   public required string Content { get; set; }
   public required string Type { get; set; }
}

internal class DnsRecordWithIdInfo : DnsRecordInfo
{
   public required string Id { get; set; }
}