namespace ConsoleApp;

public interface IDomainRegistrar
{
   Task<string> GetCurrentAddressAsync(string domain, string name);
   Task UpdateAddressAsync(string domain, string name, string address);
}
