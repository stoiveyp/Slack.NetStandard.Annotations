using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slack.NetStandard.Annotations;

internal class MarkerBuildInfo
{
    public BaseTypeSyntax? BaseType { get; set;}
    public ConstructorInitializerSyntax? BaseInitializer { get; set; }
    public Func<ClassDeclarationSyntax, MethodDeclarationSyntax, ClassDeclarationSyntax> ExecuteMethod { get; set; }
}