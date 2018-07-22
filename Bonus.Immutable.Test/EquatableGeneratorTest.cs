using Xunit;

namespace Bonus.Immutable.Test
{
    public interface IEquatableTest : IImmutable<IEquatableTest>
    {
        long Number { get; }
    }

    public class EquatableGeneratorTest
    {
        [Fact]
        public void WithSameValuesTheyShouldBeEqual()
        {
            var resolver = TypeGenerator.Generate(new[] { typeof(IEquatableTest) });
            var equatable = resolver.CreateInstance<IEquatableTest>();

            var instance1 = equatable.Set(e => e.Number, 5);
            var instance2 = equatable.Set(e => e.Number, 5);

            Assert.Equal(instance1, instance2);
            Assert.NotEqual(equatable, instance1);
            Assert.NotEqual(equatable, instance2);
        }

        [Fact]
        public void FromDifferentGenerationButWithSameValuesTheyShouldBeEqual()
        {
            var resolver1 = TypeGenerator.Generate(new[] { typeof(IEquatableTest) });
            var resolver2 = TypeGenerator.Generate(new[] { typeof(IEquatableTest) });
            var equatable1 = resolver1.CreateInstance<IEquatableTest>();
            var equatable2 = resolver2.CreateInstance<IEquatableTest>();

            var instance1 = equatable1.Set(e => e.Number, 5);
            var instance2 = equatable2.Set(e => e.Number, 5);

            Assert.Equal(instance1, instance2);
            Assert.True(instance1.Equals((object) instance2));
        }
    }
}