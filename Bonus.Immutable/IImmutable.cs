using System;
using System.Collections.Generic;

namespace Bonus.Immutable
{
    public interface IImmutable<T> : IEquatable<T> where T : IImmutable<T>
    {
        T Set(Dictionary<string, object> properties);
    }
}
