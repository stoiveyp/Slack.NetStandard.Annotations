using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slack.NetStandard.Annotations;

public class AppInformation
{
    public ClassDeclarationSyntax[] EventHandlers { get; }

    private AppInformation(ClassDeclarationSyntax[] eventHandlers)
    {
        EventHandlers = eventHandlers;
    }

    public static AppInformation GenerateFrom(ClassDeclarationSyntax cls, Action<Diagnostic> reportDiagnostic)
    {
        var evtHandlers = cls.Members.OfType<MethodDeclarationSyntax>().Filter().Convert(cls, reportDiagnostic).Where(c => c != null).ToArray();
        return new AppInformation(evtHandlers!);
    }

    public ArgumentSyntax PipelineHandlerArray()
    {
        var arrayType = SF.ArrayType(SF.GenericName(
                    SF.Identifier(Strings.Types.RequestHandlerInterface))
                .WithTypeArgumentList(SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.Object)))))
            .WithRankSpecifiers(SF.SingletonList(SF.ArrayRankSpecifier(SF.SingletonSeparatedList<ExpressionSyntax>(SF.OmittedArraySizeExpression()))));

        var eventHandlers = GroupedEvents(EventHandlers.Select(h =>
            SF.ObjectCreationExpression(SF.IdentifierName(h.Identifier.Text)).WithArgumentList(
                SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.ThisExpression()))))).ToArray());

        return SF.Argument(
            SF.ArrayCreationExpression(arrayType,
                SF.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                    SF.SeparatedList<ExpressionSyntax>(new []{eventHandlers}))));
    }

    private ObjectCreationExpressionSyntax GroupedEvents(ObjectCreationExpressionSyntax[] handlerCreation)
    {
        if (handlerCreation.Length == 1)
        {
            return handlerCreation[0];
        }

        var eventCheck = SF.ParenthesizedLambdaExpression();
        return GroupedHandlers(eventCheck, handlerCreation);
    }

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
