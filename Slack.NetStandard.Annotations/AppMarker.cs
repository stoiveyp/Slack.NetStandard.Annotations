using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Slack.NetStandard.Annotations.Markers;

namespace Slack.NetStandard.Annotations
{
    internal static class AppMarker
    {
        public static void StaticCodeGeneration(IncrementalGeneratorPostInitializationContext obj)
        {

        }

        public static bool AttributePredicate(SyntaxNode sn, CancellationToken _)
        {
            return sn is ClassDeclarationSyntax;
        }

        public static ClassDeclarationSyntax? AppClasses(GeneratorSyntaxContext context, CancellationToken _)
        {
            if (context.Node is not ClassDeclarationSyntax cls)
            {
                return null;
            }

            return cls.ContainsAttributeNamed(nameof(SlackAppAttribute).NameOnly()) ? cls : null;
        }

        public static bool ContainsAttributeNamed(this ClassDeclarationSyntax cls, string markerName) =>
            cls.GetAttributeNamed(markerName) != null;

        public static AttributeSyntax? GetAttributeNamed(this ClassDeclarationSyntax cls, string markerName)
            => cls.AttributeLists
                .SelectMany(al => al.Attributes)
                .FirstOrDefault(n => n.MarkerName() == markerName);

        public static string NameOnly(this string fullAttribute) => fullAttribute.Substring(0, fullAttribute.Length - 9);

        internal static string ToCodeString(this SyntaxNode token)
        {
            var sb = new StringBuilder();
            using var writer = new StringWriter(sb);
            token.NormalizeWhitespace().WriteTo(writer);
            return sb.ToString();
        }

        internal static string? MarkerName(this AttributeSyntax attribute) =>
            attribute.Name is IdentifierNameSyntax id ? id.Identifier.Text : null;
    }
}
