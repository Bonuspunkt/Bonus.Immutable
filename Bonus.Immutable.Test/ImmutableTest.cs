using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bonus.Immutable.Test
{
    public class ImmutableTest
    {
        interface IEntity : IImmutable<IEntity> {
            int Id { get; }
            string Text { get; }
        }

        class Entity : IEntity
        {
            public int Id { get; private set; }
            public string Text { get; private set; }

            public IEntity Set(Dictionary<string, object> properties = null)
            {
                var result = new Entity {
                    Id = properties.ContainsKey("Id") ? (int)properties["Id"] : Id,
                    Text = properties.ContainsKey("Text") ? (string)properties["Text"] : Text,
                };

                return result;
            }
        }

        [Fact]
        public void Test1()
        {
            var entity = new Entity();
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
