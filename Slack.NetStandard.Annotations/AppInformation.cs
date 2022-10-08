using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Slack.NetStandard.Annotations.Markers;

namespace Slack.NetStandard.Annotations;

public class AppInformation
{
    public IEnumerable<IGrouping<string, ClassDeclarationSyntax>> Handlers { get; }


    private AppInformation(IEnumerable<IGrouping<string, ClassDeclarationSyntax>> eventHandlers)
    {
        Handlers = eventHandlers;
    }

    public static AppInformation GenerateFrom(ClassDeclarationSyntax cls, Action<Diagnostic> reportDiagnostic)
    {
        var groupsOfHandlers = cls.Members.OfType<MethodDeclarationSyntax>().ConvertTagged(cls, reportDiagnostic).Where(c => c.Cls != null).ToArray().GroupBy(t => t.Marker,t => t.Cls);
        return new AppInformation(groupsOfHandlers);
    }

    public ArgumentSyntax PipelineHandlerArray()
    {
        var arrayType = SF.ArrayType(SF.GenericName(
                    SF.Identifier(Strings.Types.RequestHandlerInterface))
                .WithTypeArgumentList(SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.Object)))))
            .WithRankSpecifiers(SF.SingletonList(SF.ArrayRankSpecifier(SF.SingletonSeparatedList<ExpressionSyntax>(SF.OmittedArraySizeExpression()))));

        var argumentList = new List<ExpressionSyntax>();
        foreach (var grouping in Handlers)
        {
            if (grouping.Key == nameof(RespondsToEventAttribute).NameOnly())
            {
                argumentList.Add(GroupedIfMultiple(SCCheck(Strings.Names.EventProperty),
                    grouping.Select(h =>
                        SF.ObjectCreationExpression(SF.IdentifierName(h.Identifier.Text)).WithArgumentList(
                            SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.ThisExpression()))))).ToArray()));
            }
        }

        return SF.Argument(
            SF.ArrayCreationExpression(arrayType,
                SF.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                    SF.SeparatedList(argumentList))));
    }

    private ObjectCreationExpressionSyntax GroupedIfMultiple(ParenthesizedLambdaExpressionSyntax check, ObjectCreationExpressionSyntax[] handlerCreation)
    {
        if (handlerCreation.Length == 1)
        {
            return handlerCreation[0];
        }

        return GroupedHandlers(check, handlerCreation);
    }

    private ParenthesizedLambdaExpressionSyntax SCCheck(string propertyName) =>SF.ParenthesizedLambdaExpression()
            .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(SF.Parameter(SF.Identifier(Strings.Names.SlackContextAbbreviation)))))
            .WithExpressionBody(SF.BinaryExpression(SyntaxKind.NotEqualsExpression,
                SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName(Strings.Names.SlackContextAbbreviation), SF.IdentifierName(propertyName)),
                SF.LiteralExpression(SyntaxKind.NullLiteralExpression)));

    private ObjectCreationExpressionSyntax GroupedHandlers(ParenthesizedLambdaExpressionSyntax check, ObjectCreationExpressionSyntax[] handlerCreation)
    {
        var handlerType = SF.GenericName(Strings.Types.GroupedHandler)
            .WithTypeArgumentList(
                SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.Object))));

        return SF.ObjectCreationExpression(handlerType).WithArgumentList(SF.ArgumentList(SF.SeparatedList(new[]
        {
            SF.Argument(check)
        }.Concat(handlerCreation.Select(SF.Argument)))));
    }
}
