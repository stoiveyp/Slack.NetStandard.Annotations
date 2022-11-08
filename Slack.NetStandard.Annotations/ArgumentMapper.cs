using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection.Metadata;

namespace Slack.NetStandard.Annotations
{
    internal class ArgumentMapper
    {
        public List<StatementSyntax> CommonStatements = new();
        public List<ArgumentDetail> Arguments = new ();
        public bool InlineOnly => !CommonStatements.Any() && Arguments.All(p => p.Inline);

        public static ArgumentMapper For(MethodDeclarationSyntax method, AttributeSyntax marker, MarkerBuildInfo info, Action<Diagnostic> reportDiagnostic)
        {
            var mapper = new ArgumentMapper();
            foreach (var param in method.ParameterList.Parameters)
            {
                var type = TypeName(param);
                if (type == null || MapCommon(mapper, type))
                {
                    continue;
                }

                if (marker.IsEventMarker())
                {
                    if (type == TypeName(info.HandlerType))
                    {
                        MapContextProperty(mapper, param.Type,Strings.Names.EventProperty);
                    }
                }
                else if(marker.IsSlashCommandMarker())
                {
                    if (TypeName(param) == Strings.Types.SlashCommand)
                    {
                        mapper.AddArgument(SF.IdentifierName(Strings.Names.CommandParameter));
                    }
                }
                else if (marker.IsInteractionMarker())
                {
                    if (type == TypeName(info.HandlerType))
                    {
                        MapContextProperty(mapper, param.Type, Strings.Names.InteractionProperty);
                    }
                }
            }
            return mapper;
        }

        private static bool MapCommon(ArgumentMapper mapper, string parameterType)
        {
            if(parameterType == Strings.Types.SlackContext)
            {
                mapper.AddArgument(SF.IdentifierName(Strings.Names.ContextParameter));
                return true;
            }

            return false;
        }

        private static void MapContextProperty(ArgumentMapper mapper, TypeSyntax? type, string contextProperty)
        {
            if (type == null)
            {
                return;
            }
            mapper.AddArgument(SF.CastExpression(type, SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SF.IdentifierName(Strings.Names.ContextParameter), SF.IdentifierName(contextProperty))));
        }

        private void AddArgument(ExpressionSyntax expression)
        {
            Arguments.Add(new ArgumentDetail(expression));
        }

        private static string? TypeName(ParameterSyntax parameter) => TypeName(parameter.Type);

        public static string? TypeName(TypeSyntax? type) => type switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            PredefinedTypeSyntax predef => predef.Keyword.Text,
            GenericNameSyntax generic => generic.Identifier.Text,
            _ => type?.ToString()
        };
    }

    internal class ArgumentDetail
    {
        public List<StatementSyntax> Statements = new ();

        public ArgumentDetail(ExpressionSyntax expression)
        {
            Argument = SF.Argument(expression);
        }

        public ArgumentSyntax Argument { get; set; }
        public bool Inline => !Statements.Any();
    }
}
