using LogicBuilder.Kendo.ExpressionExtensions.Resources;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LogicBuilder.Kendo.ExpressionExtensions.Extensions
{
    internal static class TypeExtensionsEx
    {
        internal static readonly Type[] PredefinedTypes = [
            typeof(Object),
            typeof(Boolean),
            typeof(Char),
            typeof(String),
            typeof(SByte),
            typeof(Byte),
            typeof(Int16),
            typeof(UInt16),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),
            typeof(Decimal),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(Math),
            typeof(Convert)
        ];

        internal static Type GetUnderlyingElementType(this Expression expression)
        {
            TypeInfo tInfo = expression.Type.GetTypeInfo();
            Type[] genericArguments = tInfo.IsGenericType ? tInfo.GetGenericArguments() : [];
            if (genericArguments.Length != 1)
                throw new ArgumentException("Generic argument count must be 1.", nameof(expression));

            return genericArguments[0];
        }

        internal static bool IsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        internal static string FirstSortableProperty(this Type type)
        {
            PropertyInfo? firstSortableProperty = type.GetProperties().FirstOrDefault(property => property.PropertyType.IsPredefinedType()) ?? throw new NotSupportedException(Exceptions.CannotFindPropertyToSortBy);
            return firstSortableProperty.Name;
        }

        internal static bool IsPredefinedType(this Type type)
        {
            return PredefinedTypes.Any(t => t == type);
        }
    }
}
