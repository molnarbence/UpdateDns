namespace ConsoleApp.Caching;
internal class FileCacheZoneEntry
{
   public required string Id { get; set; }
   public IDictionary<string, string> DnsRecords { get; set; } = new Dictionary<string, string>();
}
