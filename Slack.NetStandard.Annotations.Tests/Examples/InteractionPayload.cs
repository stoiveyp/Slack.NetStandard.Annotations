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

        [RespondsToInteraction("shortcutCallback")]
        public object ShortcutPayload(ShortcutPayload shortcut)
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