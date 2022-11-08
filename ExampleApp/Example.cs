using Slack.NetStandard.Annotations.Markers;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.Interaction;

namespace ExampleApp
{
    [SlackApp]
    public partial class Example
    {
        [RespondsToEvent(typeof(AppHomeOpened))]
        public async Task<object> GenerateHome(AppHomeOpened appHome)
        {
            return await Task.FromResult((object)null!);
        }

        [RespondsToSlashCommand("command")]
        public object ExampleSyncCommand(SlashCommand command)
        {
            return null!;
        }

        [RespondsToSlashCommand("command2")]
        public async Task<object> ExampleAsyncCommand(SlashCommand command)
        {
            return await Task.FromResult((object)null!);
        }
    }
}