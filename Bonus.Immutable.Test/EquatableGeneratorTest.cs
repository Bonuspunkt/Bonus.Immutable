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
        public void Test()
        {
            var resolver = TypeGenerator.Generate(new[] { typeof(IEquatableTest) });
            var equatable = resolver.CreateInstance<IEquatableTest>();

            var instance1 = equatable.Set(e => e.Number, 5);
            var instance2 = equatable.Set(e => e.Number, 5);

            Assert.Equal(instance1, instance2);

        }
    }
}