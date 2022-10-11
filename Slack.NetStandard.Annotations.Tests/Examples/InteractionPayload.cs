using Slack.NetStandard.Annotations.Markers;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.RequestHandler;

namespace Slack.NetStandard.Annotations.Tests.Examples
{
    [SlackApp]
    public partial class Example
    {
        [RespondsToPayload("callbackID")]
        public object ViewSubmissionPayload(ViewSubmissionPayload payload)
        {
            return null!;
        }
    }
}