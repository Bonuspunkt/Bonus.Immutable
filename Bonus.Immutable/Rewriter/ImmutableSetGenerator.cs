using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Bonus.Immutable.Rewriter
{
    class ImmutableSetGenerator : CSharpSyntaxRewriter
    {
        private readonly Type _immutable;

        public ImmutableSetGenerator(Type immutable)
        {
            _immutable = immutable;
        }


        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            return base.VisitClassDeclaration(
                node.AddMembers(
                    ImmutableSet(_immutable, node.Identifier.Text)
                )
            );
        }

        private static MethodDeclarationSyntax ImmutableSet(Type type, string selfName)
        {
            return MethodDeclaration(
                type.ToTypeSyntax(),
                Identifier("Set")
            )
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("properties"))
                    .WithType(typeof(Dictionary<string, object>).ToTypeSyntax())
                    .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression)))
            )
            .AddBodyStatements(
                ReturnStatement(
                    ObjectCreationExpression(
                        IdentifierName(selfName),
                        ArgumentList(),
                        InitializerExpression(
                            SyntaxKind.ObjectInitializerExpression,
                            SeparatedList<ExpressionSyntax>(
                                type.GetAllProperties().Select(ImmutableSetAssignment)
                            )
                        )
                    )
                )
            );
        }

        private static AssignmentExpressionSyntax ImmutableSetAssignment(PropertyInfo property)
        {

            return AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                IdentifierName(property.Name),
                ConditionalExpression(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("properties"),
                            IdentifierName("ContainsKey")
                        )
                    )
                    .AddArgumentListArguments(
                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(property.Name)))
                    ),
                    CastExpression(
                        property.PropertyType.ToTypeSyntax(),
                        ElementAccessExpression(IdentifierName("properties"))
                            .AddArgumentListArguments(
                                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(property.Name)))
                        )
                    ),
                    IdentifierName(property.Name)
                )
            );

        }

    }
}