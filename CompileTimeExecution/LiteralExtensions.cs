using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace CompileTimeExecution
{
    internal static class LiteralExtensions
    {
        static string GetName(this Type type)
        {
            if (!type.IsGenericType)
                return type.FullName;

            string nonGenericName = type.FullName.Substring(0, type.FullName.IndexOf('`'));
            var arguments = type.GetGenericArguments().Select(a => a.GetName());
            return $"{nonGenericName}<{string.Join(", ", arguments)}>";
        }

        static string[] GetArrayLiterals(Array array, System.Collections.IEnumerator enumerator, int dim = 0)
        {
            int maxDim = array.Rank - 1;
            var length = array.GetLength(dim);
            var literals = new string[length];
            for (int i = 0; i < length; i++)
            {
                if (dim == maxDim)
                {
                    enumerator.MoveNext();
                    literals[i] = enumerator.Current.GetLiteral();
                }
                else
                {
                    var innerLiterals = GetArrayLiterals(array, enumerator, dim + 1);
                    literals[i] = $"{{{string.Join(", ", innerLiterals)}}}";
                }
            }
            return literals;
        }

        public static string GetLiteral(this object o)
        {
            var objectType = o.GetType();
            switch (o)
            {
                case sbyte:
                case byte:
                case short:
                case ushort:
                case int:
                    return SyntaxFactory.Literal(Convert.ToInt32(o)).ToString();
                case uint ui:
                    return SyntaxFactory.Literal(ui).ToString();
                case long l:
                    return SyntaxFactory.Literal(l).ToString();
                case ulong ul:
                    return SyntaxFactory.Literal(ul).ToString();
                case float f:
                    return SyntaxFactory.Literal(f).ToString();
                case double d:
                    return SyntaxFactory.Literal(d).ToString();
                case char c:
                    return SyntaxFactory.Literal(c).ToString();
                case string s:
                    return SyntaxFactory.Literal(s).ToString();
                case bool b:
                    return SyntaxFactory.LiteralExpression(b ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression).ToString();
                case Enum e:
                    return string.Join(" | ", e.ToString().Split(',').Select(f => $"{objectType.FullName}.{f.Trim()}"));
            }

            if (o is System.Collections.IEnumerable enumerable && (o is System.Collections.ICollection || objectType.GetMethod("Add") != null))
            {
                string[] literals;
                switch (o)
                {
                    case Array array:
                        literals = GetArrayLiterals(array, array.GetEnumerator());
                        break;
                    case System.Collections.IDictionary dictionary:
                        var dictionaryLiterals = new List<string>();
                        foreach (System.Collections.DictionaryEntry item in dictionary)
                            dictionaryLiterals.Add($"{{{item.Key.GetLiteral()}, {item.Value.GetLiteral()}}}");
                        literals = dictionaryLiterals.ToArray();
                        break;
                    default:
                        var enumerableLiterals = new List<string>();
                        foreach (var item in enumerable)
                            enumerableLiterals.Add(item.GetLiteral());
                        literals = enumerableLiterals.ToArray();
                        break;
                }
                return $"new {objectType.GetName()} {{{string.Join(", ", literals)}}}";
            }
            throw new SerializationException($"Can't convert object of type {o.GetType()} to a literal!");
        }


        public static string GetDeserializer(this object o, Type returnType)
        {
            var objectType = o.GetType();
            if (!objectType.IsSerializable)
                throw new SerializationException($"Can't serialize object of type {o.GetType()}!");

            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, o);
                var bytes = ms.ToArray();
                var bytesLiteral = bytes.GetLiteral();
                return $@"{{
            using(var ms = new System.IO.MemoryStream({bytesLiteral}))
            {{
                return ({returnType.GetName()})(new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Deserialize(ms));
            }}
        }}";
            }
        }
    }
}
