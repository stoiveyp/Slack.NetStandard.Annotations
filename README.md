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
        [RespondsToEventCallback(typeof(AppHomeOpened))]
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
[SlackApp(typeof(ApiGatewayProxyResponse))]
public class ExampleApp {
        [RespondsToEventCallback(typeof(AppHomeOpened))]
        public async Task<ApiGatewayProxyResponse> GenerateHome(AppHomeOpened appHome)
        {
            return Task.FromResult((object)null!);
        }
        ...
```

### What if I want a method to match more specific criteria?

In this case there is the `SlackMatches` attribute that you can use to point to a static method within the class.
This method must have the signature `static bool MethodName(SlackContext)` and it will be used as a secondary check on top of the standard attribute matching
For example

```csharp
internal static TestCmd(SlackContext cxt) => true;

[RespondsToSlashCommand()]
[SlackMatches(nameof(TestCmd))]
public Task<object> SlashCommand(SlashCommand evt, SlackContext context)
{
    return Task.FromResult((object)null!);
}
```

# Attribute Information

There are several attributes available right now. The method name you attach these two doesn't matter and can be called anything, they're just examples.

## RespondsToEventCallback

This attribute triggers the method if a particular type of callback event (So a high level type of `EventCallback` and it's Event property is the type you passed in)
The first parameter is the type of callback event

```csharp
[RespondsToEventCallback(typeof(AppHomeOpened))]
public async Task<object> GenerateHome(AppHomeOpened appHome)

```

The callback event can then be passed in to your method as a parameter (as well as the usual `SlackContext` object)

## RespondsToEvent

This attribute indicates that the method is to be triggered in response to a particular high level event type, the event type name is indicated by the parameter passed in.

```csharp
[RespondsToEvent(typeof(UrlVerification))]
public async Task<ApiGatewayProxyResponse> VerifyUrl(UrlVerification appHome)

```

The event type can then be passed in to your method as a parameter (as well as the usual `SlackContext` object)

## RespondsToInteraction

Use this attribute when you want a particular interaction payload.
The first parameter is the type of payload. 
The second optional parameter is the payload `CallbackId` property (or in the case of the BlockActionsPayload it's the first `ActionId`)

```csharp
[RespondsToInteraction(typeof(BlockActionsPayload), "actionId")]
public async Task<object> BlockActionPayloadResponse(BlockActionsPayload blocks)
```

The interaction payload can be used by your method as a passed in parameter (as well as the usual `SlackContext` object)

## RespondsToCommand

This attribute ensures your method is triggered by a command
The command name is passed in as the first attribute parameter

```csharp
[RespondsToSlashCommand("command2")]
public async Task<object> ExampleAsyncCommand(SlashCommand command)
```

The command can then be passed in as a method parameter (as well as the usual `SlackContext` object)