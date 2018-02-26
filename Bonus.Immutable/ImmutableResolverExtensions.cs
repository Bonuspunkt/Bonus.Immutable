using System;

namespace Bonus.Immutable
{
    public static class ImmutableResolverExtensions
    {

        public static Type Typed<T>(this ImmutableResolver resolver)
        {
            return resolver(typeof(T));
        }
        public static T CreateInstance<T>(this ImmutableResolver resolver)
        {
            var type = resolver.Typed<T>();
            return (T)Activator.CreateInstance(type);
        }
    }
}