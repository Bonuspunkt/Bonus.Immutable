using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Bonus.Immutable.Rewriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Bonus.Immutable
{
    public class TypeGenerator
    {
        public static ImmutableResolver Generate(
            IEnumerable<Type> types, string @namespace = null, Func<Type, Rewrite> getRewrite = null
        )
        {
            @namespace = @namespace ?? GenerateNamespace();

            foreach (var type in types)
            {
                var typeInfo = type.GetTypeInfo();

                if (!typeInfo.IsInterface)
                {
                    throw new ArgumentException($"{ type.FullName } is not an interface", nameof(types));
                }

                try
                {
                    typeof(IImmutable<>).GetTypeInfo().MakeGenericType(type);
                }
                catch
                {
                    throw new ArgumentException($"{ type.FullName } does not implement IImmutable<{ type.Name }>", nameof(types));
                }

                foreach (var property in type.GetAllProperties())
                {
                    if (property.CanWrite)
                    {
                        throw new ArgumentException($"{ type.FullName }.{property.Name} has a setter", nameof(types));
                    }
                }
            }


            var compilationUnits = types.Select(type =>
                GetRewriters(type).Concat(new[]{
                    getRewrite != null ? getRewrite(type) : node => node,
                    node => node.NormalizeWhitespace(),
                }).Aggregate(
                    CompilationUnit()
                        .AddMembers(
                            NamespaceDeclaration(@namespace.ToNameSyntax())
                                .AddMembers(Class(type))
                        ),
                    (prev, curr) => curr(prev)
                )
            ).ToArray();

            var compilation = CSharpCompilation.Create(
                @namespace,
                compilationUnits.Select(c => c.SyntaxTree),
                GetReferences(types),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithOptimizationLevel(OptimizationLevel.Release)
                    .WithPlatform(Platform.AnyCpu)
                    .WithModuleName(DateTime.Now.Ticks.ToString())
            );

            using (var peStream = new MemoryStream())
            using (var pdbStream = new MemoryStream())
            {
                var result = compilation.Emit(peStream, pdbStream);
                if (!result.Success)
                {
                    var error = string.Join(
                        Environment.NewLine,
                        result.Diagnostics.Select(d => d.ToString())
                    ) + Environment.NewLine +
                    "--- Source ---" + Environment.NewLine +
                    string.Join(
                        Environment.NewLine,
                        compilationUnits.Select(cu => cu.ToFullString()));
                    throw new Exception(error);
                }
                peStream.Position = 0;
                pdbStream.Position = 0;

                var assembly = AssemblyLoadContext.Default.LoadFromStream(peStream, pdbStream);

                var translations = assembly.GetExportedTypes().ToDictionary(
                    implementedType => types.First(type => type.GetTypeInfo().IsAssignableFrom(implementedType)),
                    implementedType => implementedType
                );

                return type => translations.TryGetValue(type, out var immutableType) ? immutableType : null;
            }
        }

        private static string GenerateNamespace()
        {
            var chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvxyz".ToCharArray();

            IEnumerable<char> Convert(long number, int radix)
            {
                if (radix < 2 || radix > chars.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(radix), $"radix must be between 2 and { chars.Length }");
                }

                do
                {
                    yield return chars[number % radix];
                    number = (number / radix);
                } while (number > 0);
            }

            var timeFragment = Convert(DateTime.Now.Ticks, chars.Length).Reverse();
            return $"Gen{ string.Join("", timeFragment) }";
        }

        private static ClassDeclarationSyntax Class(Type immutable)
        {

            var className = immutable.Name;
            if (className.StartsWith("I"))
            {
                var secondChart = className.Substring(1, 1);
                if (secondChart == secondChart.ToUpperInvariant())
                {
                    className = className.Substring(1);
                }
            }

            return ClassDeclaration(className)
                .AddBaseListTypes(
                    SimpleBaseType(immutable.FullName.ToNameSyntax())
                )
                .AddModifiers(Token(SyntaxKind.PublicKeyword));
        }


        private static IEnumerable<Rewrite> GetRewriters(Type type)
        {
            yield return unit => (CompilationUnitSyntax)new PropertyGenerator(type).Visit(unit);
            yield return unit => (CompilationUnitSyntax)new EquatableGenerator(type).Visit(unit);
            yield return unit => (CompilationUnitSyntax)new ImmutableSetGenerator(type).Visit(unit);
        }

        private static IEnumerable<MetadataReference> GetReferences(IEnumerable<Type> types)
        {
            var assemblies = types.Select(type => type.GetTypeInfo().Assembly);
            var referencedAssemblies = ResolveReferences(assemblies);

            return assemblies.Concat(referencedAssemblies)
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
                .ToArray();
        }

        public static IEnumerable<Assembly> ResolveReferences(IEnumerable<Assembly> assemblies)
        {
            var assemblyNames = assemblies.SelectMany(a => a.GetReferencedAssemblies()).ToList();

            int oldCount, newCount;
            do
            {
                oldCount = assemblyNames.Count;
                var refNames = assemblyNames
                    .Select(LoadAssembly)
                    .SelectMany(assembly => assembly.GetReferencedAssemblies())
                    .ToArray();

                foreach (var refName in refNames)
                {
                    if (!assemblyNames.Any(name => refName.Name == name.Name))
                    {
                        assemblyNames.Add(refName);
                    }
                }
                newCount = assemblyNames.Count;
            } while (oldCount != newCount);

            return assemblyNames.Select(LoadAssembly).ToArray();
        }

        private static Assembly LoadAssembly(AssemblyName name)
        {
            return AssemblyLoadContext.Default.LoadFromAssemblyName(name);
        }
    }
}