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
            .AddPipelineField()
            .PipelineInitialization(info);
    }

    public static ClassDeclarationSyntax PipelineInitialization(this ClassDeclarationSyntax appClass,
        AppInformation information)
    {
        var argumentList = new List<ArgumentSyntax> { information.PipelineHandlerArray() };

        var newPipeline = SF.ObjectCreationExpression(PipelineType())
            .WithArgumentList(SF.ArgumentList(SF.SeparatedList(argumentList)));

        var initializeMethod =
            SF.MethodDeclaration(SF.PredefinedType(SF.Token(SyntaxKind.VoidKeyword)), Strings.Names.InitializeMethod)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)))
                .AddBodyStatements(
                    SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SF.IdentifierName(Strings.Names.PipelineField), newPipeline)));
        return appClass.AddMembers(initializeMethod).AddMembers(information.EventHandlers);
    }

    public static ClassDeclarationSyntax AddPipelineField(this ClassDeclarationSyntax appClass)
    {
        var field = SF.FieldDeclaration(SF.VariableDeclaration(PipelineType()))
            .AddDeclarationVariables(SF.VariableDeclarator(Strings.Names.PipelineField))
            .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)));
        return appClass.AddMembers(field);
    }

    //new SlackPipeline<object>

    private static TypeSyntax PipelineType(string requestType = Strings.Types.Object)
    {
        return SF
            .GenericName(SF.Identifier(Strings.Types.PipelineClass)).WithTypeArgumentList(
                SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(requestType))));
    }
}