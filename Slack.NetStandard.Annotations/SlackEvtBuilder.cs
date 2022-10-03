using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Slack.NetStandard.Annotations.Markers;

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
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)));
        }
    }
}