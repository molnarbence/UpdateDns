namespace ConsoleApp;
internal interface IDnsRecordsService
{
   Task<string?> GetCurrentAddressAsync(string zoneName, string recordName);
   Task UpdateRecordAsync(string zoneName, string recordName, string newAddress);
}
