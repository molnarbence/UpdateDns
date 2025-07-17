using Refit;

namespace ConsoleApp;

internal interface ISlackNotifications
{
   [Post("")]
   Task SendNotificationAsync([Body] SlackMessage message);
}

internal record SlackMessage(string Text);

internal static class SlackNotificationsExtensions
{
   public static async Task SendNotificationAsync(this ISlackNotifications slackNotifications, string text)
   {
      var message = new SlackMessage(text);
      await slackNotifications.SendNotificationAsync(message);
   }
}