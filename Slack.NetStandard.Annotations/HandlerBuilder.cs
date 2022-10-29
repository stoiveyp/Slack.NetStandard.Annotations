using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Slack.NetStandard.Annotations.Markers;
using Slack.NetStandard.Interaction;

namespace Slack.NetStandard.Annotations;

public static class HandlerBuilder
{
    public static IEnumerable<(ClassDeclarationSyntax? Cls, string Marker)> ConvertTagged(this
        IEnumerable<MethodDeclarationSyntax> methods,
        ClassDeclarationSyntax originalClass,
        Action<Diagnostic> reportDiagnostic)
    {
        foreach (var method in methods)
        {
            if (!method.TryGetAttributeName(out var marker)) continue;
            var markerBuildInfo = MarkerBuildInfo.BuildFrom(originalClass, marker!, method, reportDiagnostic);
            yield return (markerBuildInfo == null)
                    ? (null, string.Empty)
                    : (Convert(method, markerBuildInfo!, originalClass, reportDiagnostic), marker!.MarkerName()!);
        }
    }

    private static readonly string[] ValidMarkers = {
        nameof(RespondsToEventAttribute).NameOnly(),
        nameof(RespondsToSlashCommandAttribute).NameOnly(),
        nameof(RespondsToInteractionAttribute).NameOnly()
    };

    internal static bool TryGetAttributeName(this MethodDeclarationSyntax method, out AttributeSyntax? marker)
    {
        var markerName = method.AttributeLists.SelectMany(a => a.Attributes)
            .FirstOrDefault(a => ValidMarkers.Contains(a.MarkerName()));

        if (markerName == null)
        {
            marker = null;
            return false;
        }

        marker = markerName;
        return true;
    }

    internal static ClassDeclarationSyntax Convert(this MethodDeclarationSyntax method,
        MarkerBuildInfo info,
        ClassDeclarationSyntax originalClass,
        Action<Diagnostic> reportDiagnostic)
    {
        var handlerClass = SF.ClassDeclaration(method.Identifier.Text + Strings.Names.HandlerSuffix)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)))
                .WithBaseList(SF.BaseList(
                    SF.SingletonSeparatedList(info.BaseType!)))
                .AddWrapperField(originalClass)
                .AddWrapperConstructor(originalClass, info.BaseInitializer);
        return info.ExecuteMethod(handlerClass, method, info);
    }

    private static ClassDeclarationSyntax AddWrapperConstructor(this ClassDeclarationSyntax handlerClass, ClassDeclarationSyntax wrapperClass, ConstructorInitializerSyntax? initializer)
    {
        var handlerParameter = SF.Parameter(SF.Identifier(Strings.Names.WrapperVarName))
            .WithType(SF.IdentifierName(wrapperClass.Identifier.Text));

        var constructor = SF.ConstructorDeclaration(handlerClass.Identifier.Text)
            .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.InternalKeyword)))
            .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(handlerParameter)))
            .WithBody(SF.Block(SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                SF.IdentifierName(Strings.Names.WrapperPropertyName), SF.IdentifierName(Strings.Names.WrapperVarName)))));

        if (initializer != null)
        {
            constructor = constructor.WithInitializer(initializer);
        }

        return handlerClass.AddMembers(constructor);
    }

    private static ClassDeclarationSyntax AddWrapperField(this ClassDeclarationSyntax handlerClass,
        ClassDeclarationSyntax wrapperClass)
    {
        var handlerField = SF
            .PropertyDeclaration(SF.IdentifierName(wrapperClass.Identifier.Text), Strings.Names.WrapperPropertyName)
            .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)))
            .WithAccessorList(SF.AccessorList(SF.SingletonList(SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken)))));
        return handlerClass.AddMembers(handlerField);
    }
}