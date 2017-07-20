using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bonus.Immutable
{
    public delegate CompilationUnitSyntax Rewrite(CompilationUnitSyntax compilationUnit);
    public delegate Type ImmutableResolver(Type type);
}