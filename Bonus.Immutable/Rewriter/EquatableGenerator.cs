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
    class EquatableGenerator : CSharpSyntaxRewriter
    {
        private readonly Type _immutable;

        public EquatableGenerator(Type immutable)
        {
            _immutable = immutable;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            return base.VisitClassDeclaration(
                node.AddMembers(
                    EqualsImplementation(node.Identifier.Text),
                    EquatableImplementation(_immutable),
                    GetHashCode(_immutable)
                )
            );
        }

        private static MethodDeclarationSyntax EqualsImplementation(string type)
        {
            return MethodDeclaration(
                PredefinedType(Token(SyntaxKind.BoolKeyword)),
                Identifier("Equals")
            ).AddModifiers(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.OverrideKeyword)
            ).AddParameterListParameters(
                Parameter(Identifier("obj"))
                    .WithType(PredefinedType(Token(SyntaxKind.ObjectKeyword)))
            ).AddBodyStatements(
                ReturnStatement(
                    InvocationExpression(IdentifierName("Equals"))
                        .AddArgumentListArguments(
                            Argument(
                                BinaryExpression(
                                    SyntaxKind.AsExpression,
                                    IdentifierName("obj"),
                                    IdentifierName(type)
                                )
                            )
                        )
                )
            );
        }

        private static MethodDeclarationSyntax EquatableImplementation(Type type)
        {

            var notNullCheck = BinaryExpression(SyntaxKind.NotEqualsExpression,
                IdentifierName("other"),
                LiteralExpression(SyntaxKind.NullLiteralExpression)
            );

            var body = type.GetAllProperties()
                .Select(property =>
                    InvocationExpression(
                        IdentifierName("Equals")
                    ).AddArgumentListArguments(
                        Argument(IdentifierName(property.Name)),
                        Argument(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("other"),
                                IdentifierName(property.Name)
                            )
                        )
                    )
                )
                .Aggregate(
                    notNullCheck,
                    (prev, curr) => BinaryExpression(SyntaxKind.LogicalAndExpression, prev, curr)
                );


            return MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.BoolKeyword)),
                    Identifier("Equals")
                )
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(Parameter(Identifier("other")).WithType(type.FullName.ToNameSyntax()))
                .AddBodyStatements(ReturnStatement(body));
        }

        private static MethodDeclarationSyntax GetHashCode(Type type)
        {

            var statements = new List<StatementSyntax>() {
                LocalDeclarationStatement(
                    VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)))
                        .AddVariables(
                            VariableDeclarator(Identifier("HashingBase"))
                                .WithInitializer(EqualsValueClause(CastExpression(
                                    PredefinedType( Token(SyntaxKind.IntKeyword)),
                                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(2166136261))
                                )))
                        )
                ).AddModifiers(Token(SyntaxKind.ConstKeyword)),
                LocalDeclarationStatement(
                    VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)))
                        .AddVariables(
                            VariableDeclarator(
                                Identifier("HashingMultiplier"),
                                BracketedArgumentList(),
                                EqualsValueClause(
                                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(16777619))
                                )
                            )
                        )
                ).AddModifiers(Token(SyntaxKind.ConstKeyword)),
                LocalDeclarationStatement(
                    VariableDeclaration(IdentifierName("var"))
                        .AddVariables(
                            VariableDeclarator(
                                Identifier("hashCode"),
                                BracketedArgumentList(),
                                EqualsValueClause(IdentifierName("HashingBase"))
                            )
                        )
                ),
            };
            statements.AddRange(type.GetAllProperties().Select(HashCodeForProperty));
            statements.Add(ReturnStatement(IdentifierName("hashCode")));

            return MethodDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)), Identifier("GetHashCode"))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddModifiers(Token(SyntaxKind.OverrideKeyword))
                .AddBodyStatements(
                    CheckedStatement(
                        SyntaxKind.UncheckedStatement,
                        Block(statements.ToArray())
                    )
                );
        }

        private static ExpressionStatementSyntax HashCodeForProperty(PropertyInfo property)
        {

            return ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName("hashCode"),
                    BinaryExpression(
                        SyntaxKind.ExclusiveOrExpression,
                        ParenthesizedExpression(
                            BinaryExpression(
                                SyntaxKind.MultiplyExpression,
                                IdentifierName("hashCode"),
                                IdentifierName("HashingMultiplier")
                            )
                        ),

                        property.PropertyType.CanBeNull()
                        ? (ExpressionSyntax)ParenthesizedExpression(
                            BinaryExpression(
                                SyntaxKind.CoalesceExpression,
                                ConditionalAccessExpression(
                                    IdentifierName(property.Name),
                                    InvocationExpression(
                                        MemberBindingExpression(
                                            IdentifierName("GetHashCode")
                                        )
                                    )
                                ),
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal(0)
                                )
                            )
                        )
                        : (ExpressionSyntax)InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(property.Name),
                                IdentifierName("GetHashCode")
                            )
                        )
                    )
                )
            );
        }

    }
}