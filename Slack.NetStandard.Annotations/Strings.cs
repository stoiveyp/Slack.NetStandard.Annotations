using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Slack.NetStandard.RequestHandler.Handlers;

namespace Slack.NetStandard.Annotations
{
    public static class Strings
    {
        public static class Names
        {
            public const string PipelineField = "_pipeline";
            public const string HandlerSuffix = "Handler";
            public const string InitializeMethod = "Initialize";
            public const string WrapperPropertyName = "_wrapper";
            public const string WrapperVarName = "wrapper";
            public const string HandleMethodName = "Handle";
            public const string ContextParameter = "context";
            public const string EventProperty = "Event";
            public const string SlackContextAbbreviation = "sc";
            public const string CommandProperty = "Command";
            public const string HandleCommandMethodName = "HandleCommand";
            public const string CommandParameter = "slashCommand";
            public const string InteractionProperty = "Interaction";
            public const string EventParameter = "@event";
            public const string ExecuteMethod = "Execute";
            public const string PayloadParameter = "InteractionPayload";
            public const string ProcessMethodName = "Process";
            public const string EnvelopeParameter = "envelope";
            public const string TmpProperty = "tmp";
            public const string ViewProperty = "View";
            public const string CallbackIdProperty = "CallbackId";
            public const string ActionId = "ActionId";
            public const string Actions = "Actions";
            public const string FromResultMethod = "FromResult";
        }

        public static class Usings
        {
            public static NameSyntax System() => NamespaceHelper.Build("System")!;
            public static NameSyntax SlackNetstandard() => NamespaceHelper.Build("Slack", "NetStandard")!;
            public static NameSyntax Tasks() => NamespaceHelper.Build("System", "Threading", "Tasks")!;
            public static NameSyntax SlackRequestHandler() => NamespaceHelper.Build(SlackNetstandard(),"RequestHandler")!;
            public static NameSyntax Handlers() => NamespaceHelper.Build(SlackRequestHandler(), "Handlers");
            public static NameSyntax Socket() => NamespaceHelper.Build(SlackNetstandard(), "Socket");
            public static NameSyntax Interaction() => NamespaceHelper.Build(SlackNetstandard(), "Interaction");
            public static NameSyntax Events() => NamespaceHelper.Build(SlackNetstandard(), "EventsApi");
        }

        public static class Types
        {
            public const string PipelineClass = "SlackPipeline";
            public const string RequestHandlerInterface = "ISlackRequestHandler";
            public const string EventHandler = "SlackEventHandler";
            public const string CommandHandler = "SlashCommandHandler";
            public const string PayloadHandler = "SlackPayloadHandler";
            public const string Object = "object";
            public const string Task = nameof(Task);
            public const string SlackContext = nameof(SlackContext);
            public const string GroupedHandler = "GroupedRequestHandler";
            public const string SlashCommand = "SlashCommand";
            public const string SlackEvent = "Slack.NetStandard.EventsApi.Event";
            public const string SlackInformation = "SlackInformation";
            public const string InteractionPayload = nameof(InteractionPayload);
            public const string Envelope = nameof(Socket.Envelope);
            public const string ViewSubmissionPayload = nameof(ViewSubmissionPayload);
            public const string BlockActionsPayload = nameof(BlockActionsPayload);
            public const string ShortcutPayload = nameof(ShortcutPayload);
            public const string GlobalShortcutPayload = nameof(GlobalShortcutPayload);

            public static GenericNameSyntax RequestHandlerWith(TypeSyntax returnType) => SF
                .GenericName(RequestHandlerInterface).WithTypeArgumentList(
                    SF.TypeArgumentList(SF.SingletonSeparatedList(returnType)));

            public static GenericNameSyntax SlackEventHandler(TypeSyntax type, TypeSyntax returnType) => SF.GenericName(EventHandler)
                .WithTypeArgumentList(SF.TypeArgumentList(SF.SeparatedList(new []{type, returnType})));

            public static GenericNameSyntax SlashCommandHandler(TypeSyntax returnType) => SF.GenericName(CommandHandler)
            .WithTypeArgumentList(SF.TypeArgumentList(SF.SingletonSeparatedList(returnType)));

            public static GenericNameSyntax SlackPayloadHandler(TypeSyntax type, TypeSyntax returnType) => SF.GenericName(PayloadHandler)
                .WithTypeArgumentList(SF.TypeArgumentList(SF.SeparatedList(new[] { type, returnType })));

            public static GenericNameSyntax TaskOf(string type)
            {
                return TaskOf(SF.IdentifierName(type));
            }

            public static GenericNameSyntax TaskOf(TypeSyntax type)
            {
                return SF.GenericName(Task).WithTypeArgumentList(
                    SF.TypeArgumentList(SF.SingletonSeparatedList(type)));
            }
        }
    }
}
