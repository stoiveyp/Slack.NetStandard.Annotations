using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slack.NetStandard.Annotations;

public class AppInformation
{
    public ClassDeclarationSyntax[] Handlers { get; }

    private AppInformation(ClassDeclarationSyntax[] handlers)
    {
        Handlers = handlers;
    }

    public static AppInformation GenerateFrom(ClassDeclarationSyntax cls, Action<Diagnostic> reportDiagnostic)
    {
        var evtHandlers = cls.Members.OfType<MethodDeclarationSyntax>().Filter().Convert(cls, reportDiagnostic).Where(c => c != null).ToArray();
        return new AppInformation(evtHandlers!);
    }

    public ArgumentSyntax HandlerArray()
    {
        var arrayType = SF.ArrayType(SF.GenericName(
                    SF.Identifier(Strings.Types.RequestHandlerInterface))
                .WithTypeArgumentList(SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.Object)))))
            .WithRankSpecifiers(SF.SingletonList(SF.ArrayRankSpecifier(SF.SingletonSeparatedList<ExpressionSyntax>(SF.OmittedArraySizeExpression()))));

        return SF.Argument(
            SF.ArrayCreationExpression(arrayType,
                SF.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                    SF.SeparatedList<ExpressionSyntax>(
                        Handlers.Select(h =>
                            SF.ObjectCreationExpression(SF.IdentifierName(h.Identifier.Text)).WithArgumentList(
                                SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.ThisExpression())))))))));
    }
}