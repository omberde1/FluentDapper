using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace FluentDapper.Core
{
    internal static class DapperHelpers
    {
        public static readonly ConcurrentDictionary<Type, List<PropertyInfo>> _propertyCache = new ConcurrentDictionary<Type, List<PropertyInfo>>();
        internal static List<PropertyInfo> GetCachedProperties(Type type)
        {
            return _propertyCache.GetOrAdd(type, t => t.GetProperties().ToList());
        }

        internal static object EnsureSafeValue(Type type, object value)
        {
            if (value != null) return value;

            // Check for Nullable<T>
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null) return null; // Allow null for nullable properties
            

            // Non-nullable types — provide default values
            if (type == typeof(string)) return string.Empty;
            if (type == typeof(int)) return 0;
            if (type == typeof(long)) return 0L;
            if (type == typeof(decimal)) return 0m;
            if (type == typeof(float)) return 0f;
            if (type == typeof(double)) return 0d;
            if (type == typeof(bool)) return false;
            if (type == typeof(DateTime)) return DateTime.MinValue;
            if (type == typeof(Guid)) return Guid.Empty;

            // Fallback for any other non-nullable reference or value types
            return Activator.CreateInstance(type);
        }
        internal static IEnumerable<PropertyInfo> GetNonNullProperties<T>(T obj)
        {
            var props = GetCachedProperties(typeof(T));
            return props.Where(p => p.GetValue(obj) != null && !Attribute.IsDefined(p, typeof(IgnorePropertyAttribute)));
        }
        // Optional: if want to exclude props from insert/update
        [AttributeUsage(AttributeTargets.Property)]
        private class IgnorePropertyAttribute : Attribute { }

    }
}