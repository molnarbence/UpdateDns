namespace ConsoleApp;
internal interface IIdMappings
{
   ValueTask<string?> GetZoneId(string zoneName);
   ValueTask<string?> GetDnsRecordId(string zoneName, string recordName);
}
