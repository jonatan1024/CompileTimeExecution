using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Reflection;

namespace CompileTimeExecution
{
    internal static class SymbolExtensions
    {
        public static string GetFullName(this ISymbol symbol)
        {
            string name = symbol.Name;
            if (symbol is INamedTypeSymbol namedType && namedType.IsGenericType)
                name = $"{name}<{string.Join(", ", namedType.TypeArguments.Select(arg => arg.GetFullName()))}>";
            if (symbol.ContainingNamespace != null && !symbol.ContainingNamespace.IsGlobalNamespace && !(symbol is ITypeParameterSymbol))
                name = $"{symbol.ContainingNamespace.GetFullName()}.{name}";
            return name;
        }

        public static BindingFlags GetBindingFlags(this ISymbol symbol)
        {
            BindingFlags bindingFlags = BindingFlags.Default;
            bindingFlags |= symbol.DeclaredAccessibility == Accessibility.Public ? BindingFlags.Public : BindingFlags.NonPublic;
            bindingFlags |= symbol.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
            return bindingFlags;
        }

        public static string GetAccessibility(this ISymbol symbol)
        {
            switch (symbol.DeclaredAccessibility)
            {
                case Accessibility.Public:
                    return "public";
                case Accessibility.Protected:
                    return "protected";
                case Accessibility.Internal:
                    return "internal";
                case Accessibility.Private:
                    return "private";
                default:
                    throw new Exception($"Unknown accesibility modifier {symbol.DeclaredAccessibility}");
            }
        }

        public static string GetReflectionName(this ISymbol symbol)
        {
            if (symbol.ContainingType != null)
                return $"{symbol.ContainingType.GetReflectionName()}+{symbol.Name}";
            return symbol.GetFullName();
        }
        public static MemberInfo GetMember(this ISymbol symbol, Assembly assembly)
        {
            Type type = assembly.GetType(symbol.ContainingType.GetReflectionName());
            if (type == null)
                return null;
            switch (symbol)
            {
                case IMethodSymbol methodSymbol:
                    return type.GetMethod(symbol.Name, symbol.GetBindingFlags());
                case IPropertySymbol propertySymbol:
                    return type.GetProperty(symbol.Name, symbol.GetBindingFlags());
                default:
                    throw new ArgumentException($"Invalid type of symbol {symbol.GetType()}! Expected IMethodSymbol or IPropertySymbol!", nameof(symbol));
            }
        }

        public static ITypeSymbol GetReturnType(this ISymbol symbol)
        {
            switch (symbol)
            {
                case IMethodSymbol methodSymbol:
                    return methodSymbol.ReturnType;
                case IPropertySymbol propertySymbol:
                    return propertySymbol.Type;
                default:
                    throw new ArgumentException($"Invalid type of symbol {symbol.GetType()}! Expected IMethodSymbol or IPropertySymbol!", nameof(symbol));
            }
        }
    }
}
