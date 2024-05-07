using Refit;

namespace ConsoleApp;

public interface IPublicIpAddressResolver
{
   [Get("/")]
   Task<string> GetPublicIpAddressAsync();
}
