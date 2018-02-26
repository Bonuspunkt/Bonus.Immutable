using System;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Bonus.Immutable.Rewriter
{
    class PropertyGenerator : CSharpSyntaxRewriter
    {
        private readonly Type _immutable;

        public PropertyGenerator(Type immutable)
        {
            _immutable = immutable;
        }

        public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var usings = _immutable.GetAllProperties()
                .SelectMany(p => new[] { p.PropertyType }.Concat(p.PropertyType.GetGenericArguments()))
                .Select(type => type.Namespace)
                .Distinct()
                .Select(name => UsingDirective(name.ToNameSyntax()))
                .ToArray();

            return base.VisitNamespaceDeclaration(
                node.AddUsings(usings)
            );
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var properties = _immutable.GetAllProperties()
                .Select(property => Property(property))
                .ToArray<MemberDeclarationSyntax>();

            return base.VisitClassDeclaration(
                node.AddMembers(properties)
            );
        }


        public static PropertyDeclarationSyntax Property(PropertyInfo property)
        {
            return PropertyDeclaration(
                property.PropertyType.ToTypeSyntax(),
                Identifier(property.Name)
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration),
                AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .AddModifiers(Token(SyntaxKind.PrivateKeyword))
            );
        }
    }
}