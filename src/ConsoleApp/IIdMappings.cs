namespace ConsoleApp;
internal interface IIdMappings
{
   Task<string?> GetZoneId(string zoneName);
   Task<string?> GetDnsRecordId(string zoneId, string recordName);
}
