using Xunit;

namespace Bonus.Immutable.Test
{
    public interface IEntity : IImmutable<IEntity>
    {
        int Id { get; }
        string Text { get; }
    }

    public class ImmutableSetGeneratorTest
    {
        [Fact]
        public void GenerateValidInterface()
        {
            var resolver = TypeGenerator.Generate(new[] { typeof(IEntity) });

            var entity = resolver.CreateInstance<IEntity>();
            Assert.Equal(default(int), entity.Id);
            Assert.Equal(default(string), entity.Text);

            var step1 = entity.Set(e => e.Id, 1);
            Assert.Equal(1, step1.Id);
            Assert.Equal(default(string), step1.Text);

            var step2 = entity.Set(e => e.Id, 2).Set(e => e.Text, "abc");
            Assert.Equal(2, step2.Id);
            Assert.Equal("abc", step2.Text);
        }
    }
}