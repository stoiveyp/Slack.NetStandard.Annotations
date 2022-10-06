using Slack.NetStandard.Annotations.Markers;
using Slack.NetStandard.Interaction;

namespace Slack.NetStandard.Annotations.Tests.Examples
{
    [SlackApp]
    public partial class Example
    {
        [RespondsToSlashCommand("test")]
        public object SlashCommand(SlashCommand evt)
        {
            return null!;
        }

        [RespondsToSlashCommand("test2")]
        public Task<object> SlashCommand2(SlashCommand evt)
        {
            return Task.FromResult((object)null!);
        }
    }
}