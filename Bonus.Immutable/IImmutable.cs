using System.Collections.Generic;

namespace Bonus.Immutable
{
    public interface IImmutable<T> where T : IImmutable<T> {
        T Set(Dictionary<string, object> properties);
    }
}
