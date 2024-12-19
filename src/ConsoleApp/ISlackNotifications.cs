using Refit;

namespace ConsoleApp;

internal interface ISlackNotifications
{
   [Post("")]
   Task SendNotificationAsync([Body] SlackMessage message);
}

internal class SlackMessage
{
   public string Text { get; set; } = string.Empty;
}