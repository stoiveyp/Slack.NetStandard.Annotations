using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Slack.NetStandard.Annotations
{
    public static class Strings
    {
        public static class Names
        {
            public const string PipelineField = "_pipeline";
            public const string HandlerSuffix = "Handler";
            public const string InitializeMethod = "Initialize";
        }

        public static class Usings
        {
            public static NameSyntax System() => NamespaceHelper.Build("System")!;
            public static NameSyntax SlackNetstandard() => NamespaceHelper.Build("Slack", "NetStandard")!;
            public static NameSyntax Tasks() => NamespaceHelper.Build("System", "Threading", "Tasks")!;
            public static NameSyntax SlackRequestHandler() => NamespaceHelper.Build("Slack", "NetStandard", "RequestHandler")!;
        }

        public static class Types
        {
            public const string PipelineClass = "SlackPipeline";
            public const string RequestHandlerInterface = "ISlackRequestHandler";
            public const string Object = "object";
        }
    }
}
