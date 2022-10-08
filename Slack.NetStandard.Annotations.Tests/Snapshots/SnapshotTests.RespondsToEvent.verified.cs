//HintName: Example.app.g.cs
using System;
using Slack.NetStandard;
using Slack.NetStandard.RequestHandler;
using Slack.NetStandard.RequestHandler.Handlers;
using System.Threading.Tasks;

namespace Slack.NetStandard.Annotations.Tests.Examples
{
    public partial class Example
    {
        private SlackPipeline<object> _pipeline;
        public void Initialize()
        {
            _pipeline = new SlackPipeline<object>(new ISlackRequestHandler<object>[]{new RenderHomePageHandler(this)});
        }

        private class RenderHomePageHandler : SlackEventHandler<AppHomeOpened, object>
        {
            private Example _wrapper { get; }

            internal RenderHomePageHandler(Example wrapper)
            {
                _wrapper = wrapper;
            }

            public override Task<object> Handle(SlackContext context) => _wrapper.RenderHomePage((AppHomeOpened)context.Event);
        }
    }
}