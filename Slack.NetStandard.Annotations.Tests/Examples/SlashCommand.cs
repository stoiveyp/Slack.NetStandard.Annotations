using Slack.NetStandard.Annotations.Markers;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.RequestHandler;

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
        public Task<object> SlashCommand2(SlashCommand evt, SlackContext context)
        {
            return Task.FromResult((object)null!);
        }

        internal static TestCmd(SlackContext cxt) => true;

        [RespondsToSlashCommand()]
        [SlackMatches("TestCmd")]
        public Task<object> SlashCommand3(SlashCommand evt, SlackContext context)
        {
            return Task.FromResult((object)null!);
        }
    }
}