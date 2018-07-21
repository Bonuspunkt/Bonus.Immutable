using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Bonus.Immutable.Test
{
    public interface IEmptyInterface :IImmutable<IEmptyInterface> { }

    public partial class TypeGeneratorTest
    {
        [Fact]
        public void GenerateToSuppliedNamespace()
        {
            var resolver = TypeGenerator.Generate(
                new[] { typeof(IEmptyInterface) },
                "Name.Space"
            );

            var type = resolver(typeof(IEmptyInterface));
            Assert.Equal("Name.Space", type.Namespace);
        }
    }

    public interface IReturnOne
    {
        int ReturnOne();
    }
    public interface IExtendendWithInterfaceWithMethod : IReturnOne, IImmutable<IExtendendWithInterfaceWithMethod> { }

    public partial class TypeGeneratorTest
    {
        class ReturnOneGenerator : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                return base.VisitClassDeclaration(node.AddMembers(
                    SyntaxFactory.MethodDeclaration(
                            SyntaxFactory.PredefinedType(
                                SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                            SyntaxFactory.Identifier("ReturnOne"))
                        .WithModifiers(
                            SyntaxFactory.TokenList(
                                SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                        .WithBody(
                            SyntaxFactory.Block(
                                SyntaxFactory.SingletonList<StatementSyntax>(
                                    SyntaxFactory.ReturnStatement(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.NumericLiteralExpression,
                                            SyntaxFactory.Literal(1))))))
                ));
            }
        }

        [Fact]
        public void GenerateWithCustomRewrite()
        {
            var returnOneGenerator = new ReturnOneGenerator();

            var resolver = TypeGenerator.Generate(
                new[] { typeof(IExtendendWithInterfaceWithMethod) },
                getRewrite: interfaceType => node => (CompilationUnitSyntax)returnOneGenerator.Visit(node)
            );

            var instance = resolver.CreateInstance<IExtendendWithInterfaceWithMethod>();
            Assert.Equal(1, instance.ReturnOne());
        }
    }

    public interface INotImmutable { }

    public partial class TypeGeneratorTest
    {
        [Fact]
        public void NotImmutableTest()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => TypeGenerator.Generate(new[] { typeof(INotImmutable) })
            );
            Assert.StartsWith("Bonus.Immutable.Test.INotImmutable does not implement IImmutable<INotImmutable>", ex.Message);
        }
    }

    public interface IBrokenImmutable : IImmutable<IBrokenImmutable>
    {
        int Number { get; set; }
    }

    public partial class TypeGeneratorTest
    {
        [Fact]
        public void BrokenImmutableTest()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => TypeGenerator.Generate(new[] { typeof(IBrokenImmutable) })
            );
            Assert.StartsWith("Bonus.Immutable.Test.IBrokenImmutable.Number has a setter", ex.Message);
        }
    }

    public interface Entity : IImmutable<Entity>
    {
        int Number { get; }
    }

    public partial class TypeGeneratorTest
    {
        [Fact]
        public void ImmutableNameWithout_I_PrefixTest()
        {
            var resolver = TypeGenerator.Generate(new[] { typeof(Entity) });
            var entity = resolver.CreateInstance<Entity>();
        }
    }

    public interface IId<T>
    {
        T Id { get; }
    }
    public interface IEntityWithInterface : IImmutable<IEntityWithInterface>, IId<int> { }

    public partial class TypeGeneratorTest
    {
        [Fact]
        public void ImmutableWithMultipleInterfacesTest()
        {
            var resolver = TypeGenerator.Generate(new[] { typeof(IEntityWithInterface) });
            var entityWithInterface = resolver.CreateInstance<IEntityWithInterface>();
        }
    }


    public interface NullableEntity : IImmutable<NullableEntity>
    {
        bool? NullableBoolean { get; }
    }

    public partial class TypeGeneratorTest
    {
        [Fact]
        public void NullableTest()
        {
            var resolver = TypeGenerator.Generate(new[] { typeof(NullableEntity) });
            var nullableEntity = resolver.CreateInstance<NullableEntity>();
        }
    }


    public interface GenericTypeProperty : IImmutable<GenericTypeProperty>
    {
        IEnumerable<Regex> Patterns { get; }
    }
    public interface MoreComplexGenericType : IImmutable<MoreComplexGenericType>
    {
        (Task, Regex) OddTuple { get; }
    }

    public partial class TypeGeneratorTest
    {
        [Fact]
        public void Test()
        {
            var resolver = TypeGenerator.Generate(new[] {
                typeof(GenericTypeProperty),
                typeof(MoreComplexGenericType)
            });

            var genericTypeProperty = resolver.CreateInstance<GenericTypeProperty>();
            var moreComplexGenericType = resolver.CreateInstance<MoreComplexGenericType>();
        }
    }
}
