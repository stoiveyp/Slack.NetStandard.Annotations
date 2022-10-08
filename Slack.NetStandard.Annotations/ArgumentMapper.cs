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

        public static ArgumentMapper ForEventHandler(MethodDeclarationSyntax method)
        {
            var mapper = new ArgumentMapper();
            var eventHandler = method.ParameterList.Parameters.First();
            MapEventHandler(mapper, eventHandler);
            MapCommon(mapper, method.ParameterList.Parameters.Skip(1));
            return mapper;
        }

        private static void MapEventHandler(ArgumentMapper mapper, ParameterSyntax eventHandler)
        {
            mapper.AddArgument(SF.CastExpression(eventHandler.Type!, SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SF.IdentifierName(Strings.Names.ContextParameter), SF.IdentifierName(Strings.Names.EventProperty))));
        }

        private void AddArgument(ExpressionSyntax expression)
        {
            Arguments.Add(new ArgumentDetail(expression));
        }

        private static void MapCommon(ArgumentMapper mapper, IEnumerable<ParameterSyntax> parameters)
        {

        }
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
