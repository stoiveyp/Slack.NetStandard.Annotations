//HintName: Example.app.g.cs
using System;
using Slack.NetStandard;
using Slack.NetStandard.RequestHandler;
using System.Threading.Tasks;
using Slack.NetStandard.RequestHandler.Handlers;

namespace Slack.NetStandard.Annotations.Tests.Examples
{
    public partial class Example
    {
        private SlackPipeline<object> _pipeline;
        public void Initialize()
        {
            _pipeline = new SlackPipeline<object>(new ISlackRequestHandler<object>[]{});
        }

        private class SlashCommandHandler : SlashCommandHandler<object>
        {
            private Example _wrapper { get; }

            internal RenderHomePageHandler(Example wrapper):base("test")
            {
                _wrapper = wrapper;
            }

            public override Task<object> HandleCommand(SlashCommand command, SlackContext context) =>
                _wrapper.SlashCommand(command);
        }

        private class SlashCommand2Handler : SlashCommandHandler<object>
        {
            private Example _wrapper { get; }

            internal RenderHomePageHandler(Example wrapper) : base("test2")
            {
                _wrapper = wrapper;
            }

            public override Task<object> HandleCommand(SlashCommand command, SlackContext context) =>
                _wrapper.SlashCommand2(command);
        }
    }
}