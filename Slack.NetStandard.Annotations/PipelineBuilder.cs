using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Slack.NetStandard.RequestHandler;

namespace Slack.NetStandard.Annotations;

public static class PipelineBuilder
{
    public static CompilationUnitSyntax BuildPipelineClasses(ClassDeclarationSyntax cls, Action<Diagnostic> reportDiagnostic)
    {
        var appClass = SF.ClassDeclaration(cls.Identifier.Text)
            .WithModifiers(SF.TokenList(
                SF.Token(SyntaxKind.PublicKeyword),
                SF.Token(SyntaxKind.PartialKeyword)));

        appClass = appClass.BuildApp(cls);

        var usings = SF.List(new[]
        {
            Strings.Usings.System(),
            Strings.Usings.SlackNetstandard(),
            Strings.Usings.SlackRequestHandler(),
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
        ClassDeclarationSyntax hostingClass)
    {
        return appClass
            .AddPipelineField();
    }

    public static ClassDeclarationSyntax AddPipelineField(this ClassDeclarationSyntax skillClass)
    {
        var field = SF.FieldDeclaration(SF.VariableDeclaration(PipelineType(nameof(Object).ToLower())))
            .AddDeclarationVariables(SF.VariableDeclarator(Strings.Names.PipelineField))
            .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)));
        return skillClass.AddMembers(field);
    }

    //new SlackPipeline<object>

    private static TypeSyntax PipelineType(string requestType)
    {
        return SF
            .GenericName(SF.Identifier(Strings.Types.PipelineClass)).WithTypeArgumentList(
                SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(requestType))));
    }
}