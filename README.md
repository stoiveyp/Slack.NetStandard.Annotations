# Slack.NetStandard.Annotations
Library that uses method attributes to generate a Slack app 

## Creating an Slack App
To create an app, add Slack.NetStandard.Annotations as a NuGet reference and then you can tag a class with the SlackApp attribute. The big requirement is that the class has to be partial as the generator adds code to your class behind the scenes.
The code generated uses the attributes to match methods to incoming requests, and maps the method parameters for you

```csharp
using Slack.NetStandard.Annotations.Markers;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.Interaction;

namespace ExampleApp
{
    [SlackApp]
    public partial class Example
    {
        [RespondsToEvent(typeof(AppHomeOpened))]
        public async Task<object> GenerateHome(AppHomeOpened appHome)
        {
            return Task.FromResult((object)null!);
        }

        [RespondsToInteraction(typeof(BlockActionsPayload),"callbackId"]
        public object ExampleSyncCommand()
        {
            return null!;
        }

        [RespondsToSlashCommand("command2")]
        public async Task<object> ExampleAsyncCommand(SlashCommand command)
        {
            return Task.FromResult((object)null!);
        }
    }
}
```
These attributes add several `Execute` methods to your class which can be called from your code, and work with existing Slack.NetStandard objects
```csharp
        public Task<object> Execute(SlackContext context)
        public Task<object> Execute(Envelope envelope)
        public Task<object> Execute(Slack.NetStandard.EventsApi.Event @event)
        public Task<object> Execute(InteractionPayload InteractionPayload)
        public Task<object> Execute(SlashCommand slashCommand)
```

### What about the return value? Why does it only return object?

This is the default return value assumed by the generator if you just use the standard `SlackApp` attribute.

You can pass in a Type parameter if you want the return type to be something else

```csharp
[SlackApp(typeof(Message))]
public class ExampleApp {}
```

### What if I want a method to match more specific criteria?

In this case there is the `SlackMatches` attribute that you can use to point to a static method within the class.
This method must have the signature `static bool MethodName(SlackContext)` and it will be used as a secondary check on top of the standard attribute matching

# Attribute Information