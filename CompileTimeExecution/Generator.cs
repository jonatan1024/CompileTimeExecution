using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

namespace CompileTimeExecution
{
    [Generator]
    internal class Generator : ISourceGenerator
    {
        GeneratorExecutionContext context;
        Assembly cteAssembly;
        Compilation cteCompilation;
        public void Execute(GeneratorExecutionContext context)
        {
            this.context = context;

            cteCompilation = CreateCTECompilation();
            cteAssembly = CreateAssembly(cteCompilation);
            if (cteAssembly == null)
            {
                context.ReportDiagnostic("CTE0000", "Failed to compile!", "Compilation", DiagnosticSeverity.Error);
                return;
            }

            //walks over all correctly attributed members and processes (executes and bakes) them
            var walker = new AttributedMemberWalker<CompileTimeExecutionAttribute>(ProcessMember);
            foreach (var cteTree in cteCompilation.SyntaxTrees)
            {
                walker.Visit(cteTree.GetRoot());
            }
        }

        void ProcessMember(MemberDeclarationSyntax memberDeclaration)
        {
            SemanticModel model = cteCompilation.GetSemanticModel(memberDeclaration.SyntaxTree);
            var memberSymbol = model.GetDeclaredSymbol(memberDeclaration);
            var member = memberSymbol.GetMember(cteAssembly);
            if (member == null)
            {
                context.ReportDiagnostic("CTE0001", $"Couldn't find matching type '{memberSymbol.ContainingType}'!", "Reflection", DiagnosticSeverity.Error, memberSymbol.ContainingType.Locations.FirstOrDefault());
                return;
            }
            MethodInfo method = member is PropertyInfo property ? property.GetMethod : (MethodInfo)member;
            if (!method.IsStatic)
            {
                context.ReportDiagnostic("CTE0002", "Method or property must be static!", "Reflection", DiagnosticSeverity.Error, memberSymbol.Locations.FirstOrDefault());
                return;
            }
            if (method.GetParameters().Length > 0)
            {
                context.ReportDiagnostic("CTE0003", "Method or property can't have parameters!", "Reflection", DiagnosticSeverity.Error, memberSymbol.Locations.FirstOrDefault());
                return;
            }
            if (method.IsGenericMethod)
            {
                context.ReportDiagnostic("CTE0004", "Method can't be generic!", "Reflection", DiagnosticSeverity.Error, memberSymbol.Locations.FirstOrDefault());
                return;
            }

            var retVal = method.Invoke(null, new object[] { });
            if (method.ReturnType == typeof(void))
                return;

            var attribute = member.GetCustomAttribute<CompileTimeExecutionAttribute>();
            string memberBody;
            if (attribute.Deserialize)
                memberBody = GetMemberBodyDeserializer(memberSymbol, retVal, method.ReturnType);
            else
                memberBody = GetMemberBodyLiteral(memberSymbol, retVal);
            if (memberBody == null)
                return;

            string fileText = $"{memberSymbol.GetAccessibility()} static {memberSymbol.GetReturnType().ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {memberSymbol.Name}{(memberSymbol is IMethodSymbol ? "()" : "")} {memberBody}";
            var containingType = memberSymbol.ContainingType;
            while (containingType != null)
            {
                fileText = $@"
    {containingType.GetAccessibility()} partial class {containingType.Name} {{
        {fileText}
    }}
    ";
                containingType = containingType.ContainingType;
            }
            if (!memberSymbol.ContainingNamespace.IsGlobalNamespace)
                fileText = $@"
namespace {memberSymbol.ContainingNamespace.GetFullName()} {{
    {fileText}
}}
";

            context.AddSource(memberSymbol.ToString(), SourceText.From(fileText, Encoding.UTF8));
        }

        Compilation CreateCTECompilation()
        {
            var cteParseOptions = ((CSharpParseOptions)context.ParseOptions).WithPreprocessorSymbols(context.ParseOptions.PreprocessorSymbolNames.Concat(new string[] { nameof(CompileTimeExecution) }));
            var cteTrees = context.Compilation.SyntaxTrees.Select(t => t.ReParse(cteParseOptions));
            var generatorReference = MetadataReference.CreateFromFile(typeof(Generator).Assembly.Location);
            var cteCompilation = context.Compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(cteTrees).AddReferences(generatorReference);
            return cteCompilation;
        }

        Assembly CreateAssembly(Compilation compilation)
        {
            using var assemblyStream = new MemoryStream();
            using var symbolStream = new MemoryStream();
            EmitResult emitResult = compilation.Emit(assemblyStream, symbolStream);
            foreach (var diagnostic in emitResult.Diagnostics)
            {
                context.ReportDiagnostic(diagnostic);
            }
            if (!emitResult.Success)
                return null;

            return Assembly.Load(assemblyStream.ToArray(), symbolStream.ToArray());
        }

        string GetMemberBodyLiteral(ISymbol memberSymbol, object retVal)
        {
            try
            {
                return $"=> {retVal.GetLiteral()};";
            }
            catch (SerializationException ex)
            {
                var extraInfo = retVal.GetType().IsSerializable ? $"Specify {nameof(CompileTimeExecutionAttribute)}.{nameof(CompileTimeExecutionAttribute.Deserialize)} = true to use deserializer instead." : "";
                context.ReportDiagnostic("CTE0005", $"Failed to convert return value into a literal! {ex.Message} {extraInfo}", "Serialization", DiagnosticSeverity.Error, memberSymbol.Locations.FirstOrDefault());
                return null;
            }
        }

        string GetMemberBodyDeserializer(ISymbol memberSymbol, object retVal, Type returnType)
        {
            try
            {
                var memberBody = retVal.GetDeserializer(returnType);
                if (memberSymbol is IPropertySymbol)
                    memberBody = $"{{ get {memberBody} }}";
                return memberBody;
            }
            catch (SerializationException ex)
            {
                context.ReportDiagnostic("CTE0006", $"Failed to create deserializer! {ex.Message}", "Serialization", DiagnosticSeverity.Error, memberSymbol.Locations.FirstOrDefault());
                return null;
            }
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }
}
