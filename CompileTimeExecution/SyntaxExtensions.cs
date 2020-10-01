using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CompileTimeExecution
{
    internal static class SyntaxExtensions
    {
        public static SyntaxTree ReParse(this SyntaxTree tree, CSharpParseOptions parseOptions)
        {
            return CSharpSyntaxTree.ParseText(tree.GetText(), parseOptions, tree.FilePath);
        }

        public static string GetLocalName(this NameSyntax name)
        {
            if (name is QualifiedNameSyntax qualifiedName)
            {
                return GetLocalName(qualifiedName.Right);
            }
            return name.ToString();
        }
    }
}
