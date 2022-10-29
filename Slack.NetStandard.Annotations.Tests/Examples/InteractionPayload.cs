using Slack.NetStandard.Annotations.Markers;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.RequestHandler;
using Slack.NetStandard.RequestHandler.Handlers;

namespace Slack.NetStandard.Annotations.Tests.Examples
{
    [SlackApp]
    public partial class Example
    {
        [RespondsToInteraction(typeof(ViewSubmissionPayload),"callbackID")]
        public object ViewSubmissionPayload(ViewSubmissionPayload payload)
        {
            //payload.View.CallbackId
            return null!;
        }

        [RespondsToInteraction(typeof(BlockActionsPayload), "actionId")]
        public object BlockActionPayloadResponse(BlockActionsPayload blocks)
        {
            //blocks.Actions[0].ActionId
            return null!;
        }

        public static bool Test(SlackContext context)
        {
            return context.Interaction is ShortcutPayload shortcut && shortcut.CallbackId == "shortcutCallback";
        }

        [RespondsToInteraction(typeof(ShortcutPayload))]
        [SlackMatches(nameof(Test))]
        public object ShortcutPayload()
        {
            //shortcut.CallbackId
            return null!;
        }

        [RespondsToInteraction(typeof(GlobalShortcutPayload),"globalShortcutCallback")]
        public object GlobalPayload(GlobalShortcutPayload globalPayload)
        {
            //globalPayload.CallbackId
            return null!;
        }
    }
}