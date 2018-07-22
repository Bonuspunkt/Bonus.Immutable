using System;
using Bonus.Immutable.Rewriter;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bonus.Immutable
{
    public static class Implement
    {
        public static Rewrite Equatable(Type type)
        {
            return compilationUnit => (CompilationUnitSyntax)new EquatableGenerator(type).Visit(compilationUnit);
        }
        public static Rewrite ImmutableSet(Type type)
        {
            return compilationUnit => (CompilationUnitSyntax)new ImmutableSetGenerator(type).Visit(compilationUnit);
        }
        public static Rewrite Properties(Type type)
        {
            return compilationUnit => (CompilationUnitSyntax)new PropertyGenerator(type).Visit(compilationUnit);
        }
    }
}