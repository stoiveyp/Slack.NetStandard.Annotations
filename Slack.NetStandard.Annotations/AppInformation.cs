using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Slack.NetStandard.Annotations.Markers;

namespace Slack.NetStandard.Annotations;

public class AppInformation
{
    public IEnumerable<IGrouping<string, ClassDeclarationSyntax>> Handlers { get; }
    public TypeSyntax ReturnType { get; }


    private AppInformation(IEnumerable<IGrouping<string, ClassDeclarationSyntax>> eventHandlers, TypeSyntax returnType)
    {
        Handlers = eventHandlers;
        ReturnType = returnType;
    }

    public static TypeSyntax AppReturnType(ClassDeclarationSyntax cls)
    {
        if (cls.AttributeLists
                .SelectMany(al => al.Attributes).First(a => a.MarkerName() == nameof(SlackAppAttribute).NameOnly())
                .ArgumentList?.Arguments.FirstOrDefault()?.Expression is TypeOfExpressionSyntax typ)
        {
            return typ.Type;
        }

        return SF.IdentifierName(Strings.Types.Object);
    }

    public static AppInformation GenerateFrom(ClassDeclarationSyntax cls, Action<Diagnostic> reportDiagnostic)
    {
        var returnType = AppReturnType(cls);
        var groupsOfHandlers = cls.Members.OfType<MethodDeclarationSyntax>().ConvertTagged(cls,returnType, reportDiagnostic).Where(c => c.Cls != null).ToArray().GroupBy(t => t.Marker,t => t.Cls!);
        return new AppInformation(groupsOfHandlers, returnType);
    }

    public ArgumentSyntax PipelineHandlerArray(TypeSyntax returnType)
    {
        var arrayType = SF.ArrayType(SF.GenericName(
                    SF.Identifier(Strings.Types.RequestHandlerInterface))
                .WithTypeArgumentList(SF.TypeArgumentList(SF.SingletonSeparatedList(returnType))))
            .WithRankSpecifiers(SF.SingletonList(SF.ArrayRankSpecifier(SF.SingletonSeparatedList<ExpressionSyntax>(SF.OmittedArraySizeExpression()))));

        var argumentList = new List<ExpressionSyntax>();
        foreach (var grouping in Handlers)
        {
            if (grouping.Key == nameof(RespondsToEventAttribute).NameOnly())
            {
                argumentList.Add(GroupedIfMultiple(SCCheck(Strings.Names.EventProperty),
                    grouping.Select(h =>
                        SF.ObjectCreationExpression(SF.IdentifierName(h.Identifier.Text)).WithArgumentList(
                            SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.ThisExpression()))))).ToArray(),returnType));
            }

            if (grouping.Key == nameof(RespondsToSlashCommandAttribute).NameOnly())
            {
                argumentList.Add(GroupedIfMultiple(SCCheck(Strings.Names.CommandProperty),
                    grouping.Select(h =>
                        SF.ObjectCreationExpression(SF.IdentifierName(h.Identifier.Text)).WithArgumentList(
                            SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.ThisExpression()))))).ToArray(), returnType));
            }

            if (grouping.Key == nameof(RespondsToInteractionAttribute).NameOnly())
            {
                argumentList.Add(GroupedIfMultiple(SCCheck(Strings.Names.InteractionProperty),
                    grouping.Select(h =>
                        SF.ObjectCreationExpression(SF.IdentifierName(h.Identifier.Text)).WithArgumentList(
                            SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.ThisExpression()))))).ToArray(), returnType));
            }
        }

        return SF.Argument(
            SF.ArrayCreationExpression(arrayType,
                SF.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                    SF.SeparatedList(argumentList))));
    }

    private ObjectCreationExpressionSyntax GroupedIfMultiple(ParenthesizedLambdaExpressionSyntax check, ObjectCreationExpressionSyntax[] handlerCreation, TypeSyntax returnType)
    {
        if (handlerCreation.Length == 1)
        {
            return handlerCreation[0];
        }

        return GroupedHandlers(check, handlerCreation, returnType);
    }

    private ParenthesizedLambdaExpressionSyntax SCCheck(string propertyName) =>SF.ParenthesizedLambdaExpression()
            .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(SF.Parameter(SF.Identifier(Strings.Names.SlackContextAbbreviation)))))
            .WithExpressionBody(SF.BinaryExpression(SyntaxKind.NotEqualsExpression,
                SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName(Strings.Names.SlackContextAbbreviation), SF.IdentifierName(propertyName)),
                SF.LiteralExpression(SyntaxKind.NullLiteralExpression)));

    private ObjectCreationExpressionSyntax GroupedHandlers(ParenthesizedLambdaExpressionSyntax check, ObjectCreationExpressionSyntax[] handlerCreation, TypeSyntax returnType)
    {
        var handlerType = SF.GenericName(Strings.Types.GroupedHandler)
            .WithTypeArgumentList(
                SF.TypeArgumentList(SF.SingletonSeparatedList(returnType)));

        return SF.ObjectCreationExpression(handlerType).WithArgumentList(SF.ArgumentList(SF.SeparatedList(new[]
        {
            SF.Argument(check)
        }.Concat(handlerCreation.Select(SF.Argument)))));
    }
}
