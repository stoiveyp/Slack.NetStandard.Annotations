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
            _pipeline = new SlackPipeline<object>(new ISlackRequestHandler<object>[]{new GroupedRequestHandler<object>((sc) => sc.Interaction != null, new ViewSubmissionPayloadHandler(this), new BlockActionPayloadResponseHandler(this), new ShortcutPayloadHandler(this), new GlobalPayloadHandler(this))});
        }

        private class ViewSubmissionPayloadHandler : SlackPayloadHandler<ViewSubmissionPayload, object>
        {
            private Example _wrapper { get; }

            internal ViewSubmissionPayloadHandler(Example wrapper)
            {
                _wrapper = wrapper;
            }

            public override Task<object> Handle(SlackContext context) => _wrapper.ViewSubmissionPayload((ViewSubmissionPayload)context.Interaction);
        }

        private class BlockActionPayloadResponseHandler : SlackPayloadHandler<BlockActionsPayload, object>
        {
            private Example _wrapper { get; }

            internal BlockActionPayloadResponseHandler(Example wrapper)
            {
                _wrapper = wrapper;
            }

            public override Task<object> Handle(SlackContext context) => _wrapper.BlockActionPayloadResponse((BlockActionsPayload)context.Interaction);
        }

        private class ShortcutPayloadHandler : SlackPayloadHandler<ShortcutPayload, object>
        {
            private Example _wrapper { get; }

            internal ShortcutPayloadHandler(Example wrapper)
            {
                _wrapper = wrapper;
            }

            public override Task<object> Handle(SlackContext context) => _wrapper.ShortcutPayload((ShortcutPayload)context.Interaction);
        }

        private class GlobalPayloadHandler : SlackPayloadHandler<GlobalShortcutPayload, object>
        {
            private Example _wrapper { get; }

            internal GlobalPayloadHandler(Example wrapper)
            {
                _wrapper = wrapper;
            }

            public override Task<object> Handle(SlackContext context) => _wrapper.GlobalPayload((GlobalShortcutPayload)context.Interaction);
        }
    }
}