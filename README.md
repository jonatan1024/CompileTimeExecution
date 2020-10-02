# CompileTimeExecution
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/CompileTimeExecution)](https://www.nuget.org/packages/CompileTimeExecution)
[![GitHub license](https://img.shields.io/github/license/jonatan1024/CompileTimeExecution)](https://github.com/jonatan1024/CompileTimeExecution/blob/master/LICENSE.md)

CompileTimeExecution a [Source Generator](#more-info-on-source-generators) that runs your C# code at compile time and stores the result values into generated classes.

That means that upon compilation of this class:
```csharp
partial class UsefulConstants
{
#if CompileTimeExecution
    static int Factorial(int x) => x <= 1 ? 1 : Factorial(x - 1) * x;

    [CompileTimeExecution.CompileTimeExecution]
    static int Fac10 => Factorial(10);
#endif
}
```
A new class is generated:
```csharp
partial class UsefulConstants
{
    static int Fac10 => 3628800;
}
```
The attributed property was executed and the result was stored into the source code of a newly generated class.

## Features
Properties and methods tagged with `[CompileTimeExecution]` are executed at compile time.
The return values (for non-void methods) are converted into literals.

If the value cannot be converted into a literal, but can be serialized, you can use parameter `[CompileTimeExecution(deserialize: true)]`.
This causes serialization into a byte array in compile time and deserialization on run time.
```csharp
...
#if CompileTimeExecution
    [CompileTimeExecution(deserialize: true)]
    static int Fac10 => Factorial(10);
#endif
...
```
Resulting code:
```csharp
...
    static int Fac10
    {
        get
        {
            using(var ms = new MemoryStream(new byte[] { ... 0, 95, 55, 0 ... }))
            {
                return (int)(new BinaryFormatter().Deserialize(ms));
            }
        }
    }
...
```

Only static properties and methods can be executed. Methods must be parameterless. No generics either.

## Requirements and limitations
### Requirements

- [CompileTimeExecution NuGet package](https://www.nuget.org/packages/CompileTimeExecution)

And for the Source Generators, as stated in the [.NET Blogpost](#more-info-on-source-generators):
- [.NET 5 preview](https://dotnet.microsoft.com/download/dotnet/5.0)
- [Visual Studio preview](https://visualstudio.microsoft.com/vs/preview/)

### Limitations
- No other Source Generators
  * CompileTimeExecution compiles your project, so no other Source Generators can be used.
- .NET Standard 2.0 or .NET Framework
  * The compiled assembly must be compatible with the compiler. Since VS runs on .NET Framework, your project must be .NET Framework compatible.

## Inner workings
CompileTimeExecution works like this:
1. The symbol `CompileTimeExecution` is defined and all sources gets re-parsed
2. `CompileTimeExecution.dll` gets referenced, so the sources can use `CompileTimeExecutionAttribute`
3. The sources are then compiled into an assembly
4. The assembly gets loaded
5. Each attributed method gets executed
6. The result is converted into a literal and surrounding partial class is generated
7. Generated source code is added into the project

## More info on Source Generators
- [Roslyn: Source Generators](https://github.com/dotnet/roslyn/blob/master/docs/features/source-generators.md)
- [.NET Blog: Introducing C# Source Generators](https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/)
- [.NET Blog: New C# Source Generator Samples](https://devblogs.microsoft.com/dotnet/new-c-source-generator-samples/)

## Tips for Source Generator development
- Use `Debugger.Launch()` to debug the generator using VS as JIT debugger.
- Use file resources to inject whole source files into target project. This way you get syntax checking and static type checking.
- Emitting an assembly and loading it for reflection only can give out even more information than Syntax Trees and Semantic Models
- Use preprocessor `#if UndefinedSymbol` to get code-replacement effect. Partial classes and methods can get you only so far.
- Generate source files containing `#warning My message` to get quick-and-dirty diagnostics.
- Since documentation is non-existant, you can get most information by inspection when debugging.
