using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bonus.Immutable.Test
{

    public interface IEntity : IImmutable<IEntity> {
        int Id { get; }
        string Text { get; }
    }

    public interface INotImmutable {}

    public interface IBrokenImmutable : IImmutable<IBrokenImmutable> {
        int Number { get; set; }
    }

    public class TypeGeneratorTest
    {
        [Fact]
        public void GenerateValidInterface()
        {
            var resolver = TypeGenerator.Generate(new[]{ typeof(IEntity) });

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

        [Fact]
        public void GenerateVerifyAdditionalParameters() {
            var executed = false;

            var resolver = TypeGenerator.Generate(
                new[]{ typeof(IEntity) },
                "Name.Space",
                _ => node => { executed = true; return node; }
            );

            Assert.True(executed);

            var type = resolver(typeof(IEntity));
            Assert.Equal("Name.Space", type.Namespace);

        }

        [Fact]
        public void NotImmutableTest() {
            var ex = Assert.Throws<ArgumentException>(
                () => TypeGenerator.Generate(new[]{ typeof(INotImmutable) })
            );
            Assert.True(ex.Message.StartsWith("Bonus.Immutable.Test.INotImmutable does not implement IImmutable<INotImmutable>"));
        }

        [Fact]
        public void BrokenImmutableTest() {
            var ex = Assert.Throws<ArgumentException>(
                () => TypeGenerator.Generate(new[]{ typeof(IBrokenImmutable) })
            );

            Assert.True(ex.Message.StartsWith("Bonus.Immutable.Test.IBrokenImmutable.Number has a setter"));
        }
        }
    }
}
