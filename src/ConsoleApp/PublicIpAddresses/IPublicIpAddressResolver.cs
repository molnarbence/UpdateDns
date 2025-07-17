using Refit;

namespace ConsoleApp.PublicIpAddresses;

public interface IPublicIpAddressResolver
{
   [Get("/")]
   Task<string> GetPublicIpAddressAsync();
}
