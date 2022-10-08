using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Slack.NetStandard.Annotations.Markers;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.RequestHandler.Handlers;

namespace Slack.NetStandard.Annotations;

public static class SlackEvtBuilder
{
    public static IEnumerable<MethodDeclarationSyntax> Filter(this IEnumerable<MethodDeclarationSyntax> methods)
    {
        foreach (var method in methods)
        {
            if (method.EventAttribute() != null)
            {
                yield return method;
            }
        }
    }

    internal static AttributeSyntax? EventAttribute(this MethodDeclarationSyntax method)
    {
        return method.AttributeLists.SelectMany(a => a.Attributes)
            .FirstOrDefault(a => a.MarkerName() == nameof(RespondsToEventAttribute).NameOnly());
    }

    public static IEnumerable<ClassDeclarationSyntax?> Convert(this IEnumerable<MethodDeclarationSyntax> methods, ClassDeclarationSyntax originalClass, 
        Action<Diagnostic> reportDiagnostic)
    {
        foreach (var method in methods)
        {
            yield return SF.ClassDeclaration(method.Identifier.Text + Strings.Names.HandlerSuffix)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)))
                .WithBaseList(SF.BaseList(
                    SF.SingletonSeparatedList<BaseTypeSyntax>(SF.SimpleBaseType(Strings.Types.SlackEventHandler(method.ParameterList.Parameters.First().Type!)))))
                .AddWrapperField(originalClass)
                .AddWrapperConstructor(originalClass, null)
                .AddExecuteMethod(method);
        }
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