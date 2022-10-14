using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Array = System.Array;

namespace Slack.NetStandard.Annotations
{
    internal class ArgumentMapper
    {
        public List<StatementSyntax> CommonStatements = new();
        public List<ArgumentDetail> Arguments = new ();
        public bool InlineOnly => !CommonStatements.Any() && Arguments.All(p => p.Inline);

        public static ArgumentMapper MapFirstHandler(MethodDeclarationSyntax method, string contextParameter)
        {
            var mapper = new ArgumentMapper();
            var eventHandler = method.ParameterList.Parameters.First();
            MapFirstHandler(mapper, eventHandler, contextParameter);
            foreach (var param in method.ParameterList.Parameters.Skip(1))
            {
                MapCommon(mapper, param);
            }
            
            return mapper;
        }

        public static ArgumentMapper ForSlashCommand(MethodDeclarationSyntax method)
        {
            var mapper = new ArgumentMapper();
            foreach (var param in method.ParameterList.Parameters)
            {
                if (TypeName(param) == Strings.Types.SlashCommand)
                {
                    mapper.AddArgument(SF.IdentifierName(Strings.Names.CommandParameter));
                }
                else
                {
                    MapCommon(mapper, param);
                }
            }
            return mapper;
        }

        private static void MapCommon(ArgumentMapper mapper, ParameterSyntax parameter)
        {
            var type = TypeName(parameter);
            if (type == Strings.Types.SlackContext)
            {
                mapper.AddArgument(SF.IdentifierName(Strings.Names.ContextParameter));
            }
        }

        private static void MapFirstHandler(ArgumentMapper mapper, ParameterSyntax eventHandler, string contextProperty)
        {
            mapper.AddArgument(SF.CastExpression(eventHandler.Type!, SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SF.IdentifierName(Strings.Names.ContextParameter), SF.IdentifierName(contextProperty))));
        }

        private void AddArgument(ExpressionSyntax expression)
        {
            Arguments.Add(new ArgumentDetail(expression));
        }

        private static string? TypeName(ParameterSyntax parameter) => parameter.Type switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            PredefinedTypeSyntax predef => predef.Keyword.Text,
            GenericNameSyntax generic => generic.Identifier.Text,
            _ => parameter.Type?.ToString()
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
