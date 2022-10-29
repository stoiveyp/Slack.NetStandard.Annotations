using Slack.NetStandard.Annotations.Markers;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.RequestHandler;
using Slack.NetStandard.RequestHandler.Handlers;

namespace Slack.NetStandard.Annotations.Tests.Examples
{
    [SlackApp]
    public partial class Example
    {
        [RespondsToInteraction("callbackID")]
        public object ViewSubmissionPayload(ViewSubmissionPayload payload)
        {
            //payload.View.CallbackId
            return null!;
        }

        [RespondsToInteraction("actionId")]
        public object BlockActionPayloadResponse(BlockActionsPayload blocks)
        {
            //blocks.Actions[0].ActionId
            return null!;
        }

        public static bool Test(InteractionPayload payload)
        {
            return payload is ShortcutPayload shortcut && shortcut.CallbackId == "shortcutCallback";
        }

        [RespondsToInteraction(nameof(Test))]
        public object ShortcutPayload()
        {
            //shortcut.CallbackId
            return null!;
        }

        [RespondsToInteraction("globalShortcutCallback")]
        public object GlobalPayload(GlobalShortcutPayload globalPayload)
        {
            //globalPayload.CallbackId
            return null!;
        }
    }
}