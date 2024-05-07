namespace ConsoleApp;

public class PorkbunApiConfiguration
{
   public const string SectionName = "Porkbun";
   public required string ApiKey { get; set; }
   public required string SecretKey { get; set; }
}
