using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slack.NetStandard.Annotations;

public class AppBuilder
{
    public static void Execute(SourceProductionContext context, ImmutableArray<ClassDeclarationSyntax?> args)
    {
        if (!args.Any())
        {
            return;
        }

        foreach (var cls in args.Where(a => a != null))
        {
            try
            {
                context.AddSource($"{cls!.Identifier.Text}.app.g.cs",
                    PipelineBuilder.BuildPipelineClasses(cls, context.ReportDiagnostic).ToCodeString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

public class PipelineBuilder
{
    public static CompilationUnitSyntax BuildPipelineClasses(ClassDeclarationSyntax cls, Action<Diagnostic> reportDiagnostic)
    {
        return SF.CompilationUnit();
    }
}