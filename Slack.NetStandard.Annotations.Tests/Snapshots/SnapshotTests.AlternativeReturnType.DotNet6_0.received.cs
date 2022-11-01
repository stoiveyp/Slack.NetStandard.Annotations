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
        private SlackPipeline<Slack.NetStandard.Messages.Message> _pipeline;
        public void Initialize()
        {
            _pipeline = new SlackPipeline<Slack.NetStandard.Messages.Message>(new ISlackRequestHandler<Slack.NetStandard.Messages.Message>[]{new SlashCommandHandler(this), new ShortcutPayloadHandler(this)});
        }

        private class SlashCommandHandler : SlashCommandHandler<object>
        {
            private Example _wrapper { get; }

            internal SlashCommandHandler(Example wrapper) : base("alternativeTest")
            {
                _wrapper = wrapper;
            }

            public override Task<Slack.NetStandard.Messages.Message> HandleCommand(SlashCommand slashCommand, SlackContext context) => _wrapper.SlashCommand(slashCommand);
        }

        private class ShortcutPayloadHandler : SlackPayloadHandler<ShortcutPayload, object>
        {
            private Example _wrapper { get; }

            internal ShortcutPayloadHandler(Example wrapper) : base(Example.Test)
            {
                _wrapper = wrapper;
            }

            public override Task<Slack.NetStandard.Messages.Message> Handle(SlackContext context) => Task.FromResult(_wrapper.ShortcutPayload());
        }

        public Task<Slack.NetStandard.Messages.Message> Execute(SlackContext context) => _pipeline.Process(context);
        public Task<Slack.NetStandard.Messages.Message> Execute(Envelope envelope) => _pipeline.Process(envelope);
        public Task<Slack.NetStandard.Messages.Message> Execute(Slack.NetStandard.EventsApi.Event @event) => Execute(new SlackContext(new SlackInformation(@event)));
        public Task<Slack.NetStandard.Messages.Message> Execute(InteractionPayload InteractionPayload) => Execute(new SlackContext(new SlackInformation(InteractionPayload)));
        public Task<Slack.NetStandard.Messages.Message> Execute(SlashCommand slashCommand) => Execute(new SlackContext(new SlackInformation(slashCommand)));
    }
}