using Slack.NetStandard.Annotations.Markers;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.RequestHandler;
using Slack.NetStandard.RequestHandler.Handlers;

namespace Slack.NetStandard.Annotations.Tests.Examples
{
    [SlackApp(typeof(Slack.NetStandard.Messages.Message))]
    public partial class Example
    {
        [RespondsToSlashCommand("alternativeTest")]
        public async Task<Slack.NetStandard.Messages.Message> SlashCommand(SlashCommand payload)
        {
            return null;
        }

        public static bool Test(SlackContext context)
        {
            return context.Interaction is ShortcutPayload shortcut && shortcut.CallbackId == "shortcutCallback";
        }

        [RespondsToInteraction(typeof(ShortcutPayload))]
        [SlackMatches(nameof(Test))]
        public object ShortcutPayload()
        {
            return null;
        }
    }
}