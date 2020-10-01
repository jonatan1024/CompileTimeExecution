using Microsoft.CodeAnalysis;

namespace CompileTimeExecution
{
    internal static class ContextExtensions
    {
        public static void ReportDiagnostic(this GeneratorExecutionContext context, string id, string text, string category, DiagnosticSeverity severity, Location location = null)
        {
            var descriptor = new DiagnosticDescriptor(id, text, text, category, severity, true);
            var diagnostic = Diagnostic.Create(descriptor, location);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
