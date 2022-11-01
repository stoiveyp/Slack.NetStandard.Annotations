using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slack.NetStandard.Annotations;

public static class PipelineBuilder
{
    public static CompilationUnitSyntax BuildPipelineClasses(ClassDeclarationSyntax cls, Action<Diagnostic> reportDiagnostic)
    {
        var appClass = SF.ClassDeclaration(cls.Identifier.Text)
            .WithModifiers(SF.TokenList(
                SF.Token(SyntaxKind.PublicKeyword),
                SF.Token(SyntaxKind.PartialKeyword)));

        appClass = appClass.BuildApp(cls, reportDiagnostic);

        var usings = SF.List(new[]
        {
            Strings.Usings.System(),
            Strings.Usings.SlackNetstandard(),
            Strings.Usings.SlackRequestHandler(),
            Strings.Usings.Handlers(),
            Strings.Usings.Tasks(),
        }.Select(SF.UsingDirective!));

        var nsName = NamespaceHelper.Find(cls);
        var initialSetup = SF.CompilationUnit().WithUsings(usings);

        if (nsName != null)
        {
            return initialSetup.AddMembers(SF.NamespaceDeclaration(nsName).AddMembers(appClass));
        }

        return initialSetup.AddMembers(appClass);
    }

    public static ClassDeclarationSyntax BuildApp(this ClassDeclarationSyntax appClass,
        ClassDeclarationSyntax originalClass, Action<Diagnostic> reportDiagnostic)
    {
        var info = AppInformation.GenerateFrom(originalClass, reportDiagnostic);

        return appClass
            .AddPipelineField(info.ReturnType)
            .PipelineInitialization(info)
            .AddExecuteMethods(info.ReturnType);
    }

    public static ClassDeclarationSyntax AddExecuteMethods(this ClassDeclarationSyntax appClass, TypeSyntax returnType)
    {
        MethodDeclarationSyntax ExecuteBase(string parameterName, string parameterType) => SF
            .MethodDeclaration(Strings.Types.TaskOf(returnType), Strings.Names.ExecuteMethod)
            .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)))
            .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(SF
                .Parameter(SF.Identifier(parameterName))
                .WithType(SF.IdentifierName(parameterType)))));

        MethodDeclarationSyntax ContextFromSingleParameter(MethodDeclarationSyntax method)
        {
            var informationFromEvent = SF.ObjectCreationExpression(SF.IdentifierName(Strings.Types.SlackInformation))
                .WithArgumentList(SF.ArgumentList(
                    SF.SingletonSeparatedList(SF.Argument(SF.IdentifierName(method.ParameterList.Parameters.First().Identifier)))));

            var contextFromInformation = SF.ObjectCreationExpression(SF.IdentifierName(Strings.Types.SlackContext))
                .WithArgumentList(SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(informationFromEvent))));

            return method.WithExpressionBody(SF.ArrowExpressionClause(
                SF.InvocationExpression(SF.IdentifierName(Strings.Names.ExecuteMethod))
                    .WithArgumentList(SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(contextFromInformation))))))
                .WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));
        }

        var executeMethod = ExecuteBase(Strings.Names.ContextParameter, Strings.Types.SlackContext)
            .WithExpressionBody(SF.ArrowExpressionClause(
                SF.InvocationExpression(
                        SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            SF.IdentifierName(Strings.Names.PipelineField),
                            SF.IdentifierName(Strings.Names.ProcessMethodName)))
                    .WithArgumentList(SF.ArgumentList(
                        SF.SingletonSeparatedList(SF.Argument(SF.IdentifierName(Strings.Names.ContextParameter)))))))
            .WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));

        var envelopeMethod = ExecuteBase(Strings.Names.EnvelopeParameter, Strings.Types.Envelope)
            .WithExpressionBody(SF.ArrowExpressionClause(
                SF.InvocationExpression(
                        SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            SF.IdentifierName(Strings.Names.PipelineField),
                            SF.IdentifierName(Strings.Names.ProcessMethodName)))
                    .WithArgumentList(SF.ArgumentList(
                        SF.SingletonSeparatedList(SF.Argument(SF.IdentifierName(Strings.Names.EnvelopeParameter)))))))
            .WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));

        var executeFromEvent = ContextFromSingleParameter(ExecuteBase(Strings.Names.EventParameter, Strings.Types.SlackEvent));
        var executeFromInteraction = ContextFromSingleParameter(ExecuteBase(Strings.Names.PayloadParameter, Strings.Types.InteractionPayload));
        var executeFromSlashCommand =
            ContextFromSingleParameter(ExecuteBase(Strings.Names.CommandParameter, Strings.Types.SlashCommand));


        return appClass.AddMembers(executeMethod, envelopeMethod, executeFromEvent, executeFromInteraction, executeFromSlashCommand);
    }

    public static ClassDeclarationSyntax PipelineInitialization(this ClassDeclarationSyntax appClass,
        AppInformation information)
    {
        var argumentList = new List<ArgumentSyntax> { information.PipelineHandlerArray(information.ReturnType) };

        var newPipeline = SF.ObjectCreationExpression(PipelineType(information.ReturnType))
            .WithArgumentList(SF.ArgumentList(SF.SeparatedList(argumentList)));

        var initializeMethod =
            SF.MethodDeclaration(SF.PredefinedType(SF.Token(SyntaxKind.VoidKeyword)), Strings.Names.InitializeMethod)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)))
                .AddBodyStatements(
                    SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SF.IdentifierName(Strings.Names.PipelineField), newPipeline)));

        return appClass.AddMembers(initializeMethod).AddMembers(information.Handlers.SelectMany(g => g).ToArray());
    }

    public static ClassDeclarationSyntax AddPipelineField(this ClassDeclarationSyntax appClass, TypeSyntax returnType)
    {
        var field = SF.FieldDeclaration(SF.VariableDeclaration(PipelineType(returnType)))
            .AddDeclarationVariables(SF.VariableDeclarator(Strings.Names.PipelineField))
            .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)));
        return appClass.AddMembers(field);
    }

    private static TypeSyntax PipelineType(TypeSyntax requestType)
    {
        return SF
            .GenericName(SF.Identifier(Strings.Types.PipelineClass)).WithTypeArgumentList(
                SF.TypeArgumentList(SF.SingletonSeparatedList(requestType)));
    }
}