using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Bonus.Immutable
{
    public static class ImmutableExtensions
    {
        public static TEntity Set<TEntity, TValue>(
            this IImmutable<TEntity> immutable,
            Expression<Func<TEntity, TValue>> expression, TValue newValue)
            where TEntity : IImmutable<TEntity>
        {

            return immutable.Set(new Dictionary<string, object> {
                { ExpressionUtils.GetPropertyName(expression.Body), newValue }
            });
        }
    }
}
