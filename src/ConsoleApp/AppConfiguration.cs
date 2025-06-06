namespace ConsoleApp;
internal class AppConfiguration
{
   public string PublicIpAddressResolverBaseUrl { get; set; } = string.Empty;
   public string AppConfigurationFolderPath { get; set; } = string.Empty;
   public SlackConfiguration Slack { get; set; } = new();
   public string Domain { get; init; } = string.Empty;
   public string Name { get; init; } = string.Empty;
}

internal class SlackConfiguration
{
   public string WebhookUrl { get; set; } = string.Empty;
}
