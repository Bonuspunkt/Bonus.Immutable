using System;
using System.Linq.Expressions;

namespace Bonus.Immutable
{
    internal static class ExpressionUtils {
        public static string GetPropertyName(Expression expression) {
            switch (expression) {
                case UnaryExpression unary:
                    return GetPropertyName(unary.Operand);
                case MemberExpression property:
                    if (!(property.Expression is ParameterExpression)) {
                        throw new Exception();
                    }
                    return property.Member.Name;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}