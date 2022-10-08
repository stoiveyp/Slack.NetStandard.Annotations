using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Slack.NetStandard.Annotations.Markers;
using System.Diagnostics.Tracing;

namespace Slack.NetStandard.Annotations;

public static class HandlerBuilder
{
    public static IEnumerable<(ClassDeclarationSyntax Cls, string Marker)> ConvertTagged(this 
        IEnumerable<MethodDeclarationSyntax> methods,
        ClassDeclarationSyntax originalClass,
        Action<Diagnostic> reportDiagnostic)
    {
        foreach (var method in methods)
        {
            if (!method.TryGetAttributeName(out var marker)) continue;
            if (marker!.MarkerName() == nameof(RespondsToEventAttribute).NameOnly())
            {
                var eventBase =
                    SF.SimpleBaseType(
                        Strings.Types.SlackEventHandler(method.ParameterList.Parameters.First().Type!));
                yield return (Convert(method, eventBase,null,originalClass, reportDiagnostic), marker!.MarkerName()!);
            }

            if (marker!.MarkerName() == nameof(RespondsToSlashCommandAttribute).NameOnly())
            {
                var commandBase =
                    SF.SimpleBaseType(Strings.Types.SlashCommandHandler());
                var initializer = SF.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                    SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(marker.ArgumentList!.Arguments.First().Expression))));
                yield return (Convert(method, commandBase, initializer, originalClass, reportDiagnostic), marker!.MarkerName()!);
            }
        }
    }

    private static readonly string[] ValidMarkers = {
        nameof(RespondsToEventAttribute).NameOnly(),
        nameof(RespondsToSlashCommandAttribute).NameOnly()
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

    public static ClassDeclarationSyntax Convert(this MethodDeclarationSyntax method, 
        BaseTypeSyntax baseType,
        ConstructorInitializerSyntax? initializer,
        ClassDeclarationSyntax originalClass, 
        Action<Diagnostic> reportDiagnostic)
    { 
        return SF.ClassDeclaration(method.Identifier.Text + Strings.Names.HandlerSuffix)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)))
                .WithBaseList(SF.BaseList(
                    SF.SingletonSeparatedList(baseType)))
                .AddWrapperField(originalClass)
                .AddWrapperConstructor(originalClass, initializer)
                .AddExecuteMethod(method);
    }

    private static ClassDeclarationSyntax AddExecuteMethod(this ClassDeclarationSyntax handlerClass,
        MethodDeclarationSyntax method)
    {
        var returnType = SF.GenericName(Strings.Types.Task).WithTypeArgumentList(
            SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.Object))));

        var newMethod = SF.MethodDeclaration(returnType, Strings.Names.HandleMethodName)
            .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.OverrideKeyword)))
            .WithParameterList(
                SF.ParameterList(SF.SingletonSeparatedList(SF.Parameter(SF.Identifier(Strings.Names.ContextParameter)).WithType(SF.IdentifierName(Strings.Types.SlackContext)))));

        var mapper = ArgumentMapper.ForEventHandler(method);

        var runWrapper = SF.InvocationExpression(SF.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName(Strings.Names.WrapperPropertyName),
            SF.IdentifierName(method.Identifier.Text)),
            SF.ArgumentList(SF.SeparatedList(mapper.Arguments.Select(a => a.Argument))));

        if (mapper.InlineOnly)
        {
            newMethod = newMethod.WithExpressionBody(SF.ArrowExpressionClause(runWrapper)).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));
        }
        else
        {
            newMethod = newMethod.WithBody(SF.Block(mapper.CommonStatements
                .Concat(mapper.Arguments.SelectMany(a => a.Statements)).Concat(new StatementSyntax[]
                    { SF.ExpressionStatement(runWrapper) })));
        }

        return handlerClass.AddMembers(newMethod);
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