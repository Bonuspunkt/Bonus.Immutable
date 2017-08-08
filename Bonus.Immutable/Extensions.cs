using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Bonus.Immutable
{
    static class StringExtensions {
        public static NameSyntax ToNameSyntax(this string name) {
            var split = name.Split('.');
            if (split.Length == 1) {
                return IdentifierName(split[0]);
            }

            var identifiers = split.Select(part => IdentifierName(part)).ToArray();
            var result = QualifiedName(identifiers[0], identifiers[1]);
            for (var i = 2; i < identifiers.Length; i++) {
                result = QualifiedName(result, identifiers[i]);
            }
            return result;
        }
    }

    static class TypeExtensions {
        public static bool CanBeNull(this Type type) {
            return !type.GetTypeInfo().IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        public static IEnumerable<PropertyInfo> GetAllProperties(this Type type) {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsInterface
                ? typeInfo.GetProperties().Concat(
                    typeInfo.GetInterfaces().SelectMany(@interface => @interface.GetProperties()))
                : typeInfo.GetProperties();
        }

        public static TypeSyntax ToTypeSyntax(this Type type) {
            var actualType = Nullable.GetUnderlyingType(type) ?? type;
            var nullable = type != actualType;
            if (nullable) {
                return NullableType(actualType.Name.ToNameSyntax());
            }
            return actualType.Name.ToNameSyntax();
        }
    }
}