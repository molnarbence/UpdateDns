using Refit;

namespace ConsoleApp;

internal interface ISlackNotifications
{
   [Post("")]
   Task SendNotificationAsync([Body] SlackMessage message);
}

internal record SlackMessage(string Text);