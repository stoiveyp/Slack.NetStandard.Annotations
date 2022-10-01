using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace Slack.NetStandard.Annotations
{
    [Generator]
    public class SlackAppGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<ClassDeclarationSyntax?> candidateMarkers = context.SyntaxProvider.CreateSyntaxProvider(
                AppMarker.AttributePredicate,
                AppMarker.AppClasses).Where(c => c != null);
            var combined = candidateMarkers.Collect();
            context.RegisterSourceOutput(combined, AppBuilder.Execute);
            context.RegisterPostInitializationOutput(AppMarker.StaticCodeGeneration);
        }
    }
}
