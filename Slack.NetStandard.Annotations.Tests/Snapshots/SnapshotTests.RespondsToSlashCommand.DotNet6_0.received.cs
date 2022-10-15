﻿//HintName: Example.app.g.cs
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
            _pipeline = new SlackPipeline<object>(new ISlackRequestHandler<object>[]{new GroupedRequestHandler<object>((sc) => sc.Command != null, new SlashCommandHandler(this), new SlashCommand2Handler(this))});
        }

        private class SlashCommandHandler : SlashCommandHandler<object>
        {
            private Example _wrapper { get; }

            internal SlashCommandHandler(Example wrapper) : base("test")
            {
                _wrapper = wrapper;
            }

            public override Task<object> HandleCommand(SlashCommand slashCommand, SlackContext context) => _wrapper.SlashCommand(slashCommand);
        }

        private class SlashCommand2Handler : SlashCommandHandler<object>
        {
            private Example _wrapper { get; }

            internal SlashCommand2Handler(Example wrapper) : base("test2")
            {
                _wrapper = wrapper;
            }

            public override Task<object> HandleCommand(SlashCommand slashCommand, SlackContext context) => _wrapper.SlashCommand2(slashCommand, context);
        }

        public Task<object> Execute(SlackContext context) => _pipeline.Process(context);
        public Task<object> Execute(Envelope envelope) => _pipeline.Process(envelope);
        public Task<object> Execute(Slack.NetStandard.EventsApi.Event @event) => Execute(new SlackContext(new SlackInformation(@event)));
        public Task<object> Execute(InteractionPayload InteractionPayload) => Execute(new SlackContext(new SlackInformation(InteractionPayload)));
        public Task<object> Execute(SlashCommand slashCommand) => Execute(new SlackContext(new SlackInformation(slashCommand)));
    }
}