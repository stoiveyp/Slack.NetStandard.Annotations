﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Slack.NetStandard.Annotations.Markers;

namespace Slack.NetStandard.Annotations;

internal class MarkerBuildInfo
{
    public BaseTypeSyntax? BaseType { get; set;}
    public ConstructorInitializerSyntax? BaseInitializer { get; set; }
    public Func<ClassDeclarationSyntax, MethodDeclarationSyntax, MarkerBuildInfo, ClassDeclarationSyntax> ExecuteMethod { get; set; }
    public TypeSyntax? HandlerType { get; set; }
    public ArgumentMapper Arguments { get; set; }

    public static MarkerBuildInfo? BuildFrom(ClassDeclarationSyntax cls,AttributeSyntax marker, MethodDeclarationSyntax method, Action<Diagnostic> reportDiagnostic)
    {
        MarkerBuildInfo buildInfo = new MarkerBuildInfo();
        if (!SetBaseType(cls,method, marker, buildInfo))
        {
            return null;
        }

        buildInfo.Arguments = ArgumentMapper.For(method, marker, buildInfo, reportDiagnostic);
        buildInfo.ExecuteMethod = SelectExecute(marker);
        return buildInfo;
    }

    private static Func<ClassDeclarationSyntax, MethodDeclarationSyntax, MarkerBuildInfo, ClassDeclarationSyntax>
        SelectExecute(AttributeSyntax marker)
        => marker.IsSlashCommandMarker() ? SlashCommandExecute : SlackTypeHandlerExecute;

    private static bool SetBaseType(ClassDeclarationSyntax cls,MethodDeclarationSyntax method, AttributeSyntax marker, MarkerBuildInfo buildInfo)
    {
        var typeCheck = method.AttributeLists.SelectMany(a => a.Attributes)
            .FirstOrDefault(a => a.MarkerName() == nameof(SlackMatchesAttribute).NameOnly());

        var typeCheckMethod = GetMethodNameFromTypeCheck(typeCheck);
        
        if (marker.IsEventMarker() || marker.IsInteractionMarker())
        {
            buildInfo.HandlerType = GetHandlerType(method, marker);
            if (buildInfo.HandlerType == null)
            {
                return false;
            }

            buildInfo.BaseType = SF.SimpleBaseType(marker.IsEventMarker() ? Strings.Types.SlackEventHandler(buildInfo.HandlerType!) : Strings.Types.SlackPayloadHandler(buildInfo.HandlerType!));

            if (typeCheckMethod != null)
            {
                buildInfo.BaseInitializer = SF.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                    SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.QualifiedName(SF.IdentifierName(cls.Identifier.Text),SF.IdentifierName(typeCheckMethod))))));
            }
        }
        else if (marker.IsSlashCommandMarker())
        {
            buildInfo.BaseType =
                SF.SimpleBaseType(Strings.Types.SlashCommandHandler());

            var markers = new List<ArgumentSyntax>();

            if (marker.ArgumentList?.Arguments.Any() ?? false)
            {
                markers.Add(SF.Argument(marker.ArgumentList.Arguments.First().Expression));
            }

            if (typeCheckMethod != null)
            {
                markers.Add(SF.Argument(SF.QualifiedName(SF.IdentifierName(cls.Identifier.Text), SF.IdentifierName(typeCheckMethod))));
            }

            if (markers.Any())
            {
                buildInfo.BaseInitializer = SF.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                    SF.ArgumentList(SF.SeparatedList(markers)));
            }
        }

        return true;
    }

    private static string? GetMethodNameFromTypeCheck(AttributeSyntax? typeCheck)
    {
        var expr = typeCheck?.ArgumentList?.Arguments.FirstOrDefault()?.Expression;

        if (expr == null)
        {
            return null;
        }

        if (expr is LiteralExpressionSyntax lit)
        {
            return lit.Token.Text.Trim('"');
        }

        if (expr is InvocationExpressionSyntax invoc && invoc.ArgumentList.Arguments.First().Expression is IdentifierNameSyntax idn)
        {
            return idn.Identifier.Text;
        }


        return null;

    }

    private static TypeSyntax? GetHandlerType(MethodDeclarationSyntax method, AttributeSyntax marker)
    {
        if ((marker.ArgumentList?.Arguments.Any() ?? false) && marker.ArgumentList.Arguments.First().Expression is TypeOfExpressionSyntax typ)
        {
            return typ.Type;
        }
        
        if (method.ParameterList.Parameters.Any())
        {
            return method.ParameterList.Parameters.First().Type;
        }

        return null;
    }

    private static ClassDeclarationSyntax SlashCommandExecute(ClassDeclarationSyntax handlerClass,
        MethodDeclarationSyntax method, MarkerBuildInfo info)
    {
        var returnType = SF.GenericName(Strings.Types.Task).WithTypeArgumentList(
            SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.Object))));

        var newMethod = SF.MethodDeclaration(returnType, Strings.Names.HandleCommandMethodName)
            .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.OverrideKeyword)))
            .WithParameterList(
                SF.ParameterList(SF.SeparatedList(new[]{
                    SF.Parameter(SF.Identifier(Strings.Names.CommandParameter)).WithType(SF.IdentifierName(Strings.Types.SlashCommand)),
                    SF.Parameter(SF.Identifier(Strings.Names.ContextParameter)).WithType(SF.IdentifierName(Strings.Types.SlackContext))})));

        return handlerClass.AddMembers(AddWrapperCall(newMethod, info.Arguments, method.Identifier.Text));
    }

    private static ClassDeclarationSyntax SlackTypeHandlerExecute(ClassDeclarationSyntax handlerClass,
        MethodDeclarationSyntax method, MarkerBuildInfo info)
    {
        var returnType = SF.GenericName(Strings.Types.Task).WithTypeArgumentList(
            SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.Object))));

        var newMethod = SF.MethodDeclaration(returnType, Strings.Names.HandleMethodName)
            .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.OverrideKeyword)))
            .WithParameterList(
                SF.ParameterList(SF.SingletonSeparatedList(SF.Parameter(SF.Identifier(Strings.Names.ContextParameter)).WithType(SF.IdentifierName(Strings.Types.SlackContext)))));

        return handlerClass.AddMembers(AddWrapperCall(newMethod, info.Arguments, method.Identifier.Text));
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
}