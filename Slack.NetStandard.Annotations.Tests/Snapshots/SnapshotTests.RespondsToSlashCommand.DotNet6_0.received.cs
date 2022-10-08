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

            public override Task<object> Handle(SlackContext context) => _wrapper.SlashCommand((SlashCommand)context.Event);
        }

        private class SlashCommand2Handler : SlashCommandHandler<object>
        {
            private Example _wrapper { get; }

            internal SlashCommand2Handler(Example wrapper) : base("test2")
            {
                _wrapper = wrapper;
            }

            public override Task<object> Handle(SlackContext context) => _wrapper.SlashCommand2((SlashCommand)context.Event);
        }
    }
}