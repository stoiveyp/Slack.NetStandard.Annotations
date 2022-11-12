using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slack.NetStandard.Annotations.Markers;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;

namespace Slack.NetStandard.Annotations.Tests.Examples
{
    [SlackApp]
    public partial class Example
    {
        [RespondsToEventCallback(typeof(AppHomeOpened))]
        public object RenderHomePage(AppHomeOpened evt)
        {

        }

        internal static bool Test(SlackContext context) => true;

        [RespondsToEvent(typeof(UrlVerification))]
        [SlackMatches(nameof(Test))]
        public Task<object> RespondToUrlVerification()
        {

        }
    }
}
