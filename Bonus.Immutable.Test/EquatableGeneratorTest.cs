using System.Collections.Generic;
using Xunit;

namespace Bonus.Immutable.Test
{
    public interface IEquatable : IImmutable<IEquatable>
    {
        long Number { get; }
    }

    public partial class EquatableGeneratorTest
    {
        [Fact]
        public void WithSameValuesTheyShouldBeEqual()
        {
            var resolver = TypeGenerator.Generate(new[] { typeof(IEquatable) });
            var equatable = resolver.CreateInstance<IEquatable>();

            var instance1 = equatable.Set(e => e.Number, 5);
            var instance2 = equatable.Set(e => e.Number, 5);

            Assert.Equal(instance1, instance2);
            Assert.NotEqual(equatable, instance1);
            Assert.NotEqual(equatable, instance2);
        }

        [Fact]
        public void FromDifferentGenerationButWithSameValuesTheyShouldBeEqual()
        {
            var resolver1 = TypeGenerator.Generate(new[] { typeof(IEquatable) });
            var resolver2 = TypeGenerator.Generate(new[] { typeof(IEquatable) });
            var equatable1 = resolver1.CreateInstance<IEquatable>();
            var equatable2 = resolver2.CreateInstance<IEquatable>();

            var instance1 = equatable1.Set(e => e.Number, 5);
            var instance2 = equatable2.Set(e => e.Number, 5);

            Assert.Equal(instance1, instance2);
            Assert.True(instance1.Equals((object) instance2));
        }
    }

    public interface IEnumerablePropertyEquatable : IImmutable<IEnumerablePropertyEquatable>
    {
        IEnumerable<IEquatable> Enumerable { get; }
    }

    public partial class EquatableGeneratorTest
    {
        [Fact]
        public void EnumerablesShouldBeEqual()
        {
            var resolver = TypeGenerator.Generate(new[] { typeof(IEnumerablePropertyEquatable), typeof(IEquatable) });

            var enumerableEquatable = resolver.CreateInstance<IEnumerablePropertyEquatable>();
            var equatable = resolver.CreateInstance<IEquatable>();

            var instance1 = enumerableEquatable.Set(e => e.Enumerable, new[] { equatable.Set(f => f.Number, 5) });
            var instance2 = enumerableEquatable.Set(e => e.Enumerable, new[] { equatable.Set(f => f.Number, 5) });

            Assert.Equal(instance1, instance2);
        }
    }
}