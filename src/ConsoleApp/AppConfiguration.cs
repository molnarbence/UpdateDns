namespace ConsoleApp;
internal class AppConfiguration
{
   public string PublicIpAddressResolverBaseUrl { get; set; } = string.Empty;
   public string OTEL_EXPORTER_OTLP_ENDPOINT { get; set; } = string.Empty;
   public string OTEL_EXPORTER_OTLP_HEADERS { get; set; } = string.Empty;
   public string AppDataFolder { get; set; } = string.Empty;
}
