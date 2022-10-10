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
            var markerBuildInfo = GetMarkerBuildInfo(marker, method);
                yield return (Convert(method, markerBuildInfo,originalClass, reportDiagnostic), marker!.MarkerName()!);
        }
    }

    private static MarkerBuildInfo GetMarkerBuildInfo(AttributeSyntax marker, MethodDeclarationSyntax method)
    {
        MarkerBuildInfo buildInfo = new MarkerBuildInfo();
        if (marker!.MarkerName() == nameof(RespondsToEventAttribute).NameOnly())
        {
            buildInfo.BaseType =
                SF.SimpleBaseType(
                    Strings.Types.SlackEventHandler(method.ParameterList.Parameters.First().Type!));
            buildInfo.ExecuteMethod = EventHandlerExecute;
        }
        else if (marker!.MarkerName() == nameof(RespondsToSlashCommandAttribute).NameOnly())
        {
            buildInfo.BaseType =
                SF.SimpleBaseType(Strings.Types.SlashCommandHandler());
            buildInfo.BaseInitializer = SF.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                SF.ArgumentList(
                    SF.SingletonSeparatedList(SF.Argument(marker.ArgumentList!.Arguments.First().Expression))));
            buildInfo.ExecuteMethod = SlashCommandExecute;
        }

        return buildInfo;
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
                return info.ExecuteMethod(handlerClass, method);
    }

    private static ClassDeclarationSyntax SlashCommandExecute(ClassDeclarationSyntax handlerClass,
        MethodDeclarationSyntax method)
    {
        var returnType = SF.GenericName(Strings.Types.Task).WithTypeArgumentList(
            SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.Object))));

        var newMethod = SF.MethodDeclaration(returnType, Strings.Names.HandleCommandMethodName)
            .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.OverrideKeyword)))
            .WithParameterList(
                SF.ParameterList(SF.SeparatedList(new []{
                    SF.Parameter(SF.Identifier(Strings.Names.CommandParameter)).WithType(SF.IdentifierName(Strings.Types.SlashCommand)),
                    SF.Parameter(SF.Identifier(Strings.Names.ContextParameter)).WithType(SF.IdentifierName(Strings.Types.SlackContext))})));

        var mapper = ArgumentMapper.ForEventHandler(method);

        return handlerClass.AddMembers(AddWrapperCall(newMethod, mapper, method.Identifier.Text));
    }

    private static ClassDeclarationSyntax EventHandlerExecute(ClassDeclarationSyntax handlerClass,
        MethodDeclarationSyntax method)
    {
        var returnType = SF.GenericName(Strings.Types.Task).WithTypeArgumentList(
            SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.Object))));

        var newMethod = SF.MethodDeclaration(returnType, Strings.Names.HandleMethodName)
            .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.OverrideKeyword)))
            .WithParameterList(
                SF.ParameterList(SF.SingletonSeparatedList(SF.Parameter(SF.Identifier(Strings.Names.ContextParameter)).WithType(SF.IdentifierName(Strings.Types.SlackContext)))));

        var mapper = ArgumentMapper.ForEventHandler(method);

        return handlerClass.AddMembers(AddWrapperCall(newMethod, mapper, method.Identifier.Text));
    }

    private static MemberDeclarationSyntax AddWrapperCall(MethodDeclarationSyntax newMethod, ArgumentMapper mapper, string methodName)
    {
        var runWrapper = SF.InvocationExpression(SF.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName(Strings.Names.WrapperPropertyName),
                SF.IdentifierName(methodName)),
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

        return newMethod;
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