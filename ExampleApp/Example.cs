﻿using Slack.NetStandard.Annotations.Markers;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.RequestHandler.Handlers;

namespace ExampleApp
{
    [SlackApp]
    public class Example
    {
        [RespondsToEvent]
        public async Task<object> GenerateHome(AppHomeOpened appHome)
        {
            return Task.FromResult((object)null!);
        }

        [RespondsToSlashCommand("command")]
        public object ExampleSyncCommand(SlashCommand command)
        {
            return null!;
        }

        [RespondsToSlashCommand("command")]
        public async Task<object> ExampleAsyncCommand(SlashCommand command)
        {
            return Task.FromResult((object)null!);
        }
    }
}