//HintName: Example.app.g.cs
using System;
using Slack.NetStandard;
using Slack.NetStandard.RequestHandler;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.RequestHandler.Handlers;
using Slack.NetStandard.Socket;
using System.Threading.Tasks;

namespace Slack.NetStandard.Annotations.Tests.Examples
{
    public partial class Example
    {
        private SlackPipeline<object> _pipeline;
        public void Initialize()
        {
            _pipeline = new SlackPipeline<object>(new ISlackRequestHandler<object>[]{new GroupedRequestHandler<object>((sc) => sc.Event != null, new RenderHomePageHandler(this), new RespondToUrlVerificationHandler(this))});
        }

        private class RenderHomePageHandler : EventCallbackHandler<AppHomeOpened, object>
        {
            private Example _wrapper { get; }

            internal RenderHomePageHandler(Example wrapper)
            {
                _wrapper = wrapper;
            }

            public override Task<object> Handle(SlackContext context) => Task.FromResult(_wrapper.RenderHomePage((AppHomeOpened)((EventCallback)context.Event).Event));
        }

        private class RespondToUrlVerificationHandler : SlackEventHandler<UrlVerification, object>
        {
            private Example _wrapper { get; }

            internal RespondToUrlVerificationHandler(Example wrapper) : base(Example.Test)
            {
                _wrapper = wrapper;
            }

            public override Task<object> Handle(SlackContext context) => _wrapper.RespondToUrlVerification();
        }

        public Task<object> Execute(SlackContext context) => _pipeline.Process(context);
        public Task<object> Execute(Envelope envelope) => _pipeline.Process(envelope);
        public Task<object> Execute(Slack.NetStandard.EventsApi.Event @event) => Execute(new SlackContext(new SlackInformation(@event)));
        public Task<object> Execute(InteractionPayload InteractionPayload) => Execute(new SlackContext(new SlackInformation(InteractionPayload)));
        public Task<object> Execute(SlashCommand slashCommand) => Execute(new SlackContext(new SlackInformation(slashCommand)));
    }
}