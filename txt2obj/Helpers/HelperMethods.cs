using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace txt2obj.Helpers
{
    public class HelperMethods
    {
        public static bool IsSimple(Type type)
        {
            return
                type.IsValueType ||
                type.IsPrimitive ||
                new Type[] {
                    typeof(String),
                    typeof(Decimal),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid)
                }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }

        public static bool IsCollection(Type t)
        {
            
            if (t == typeof(string)) return false;
            
            return (
                t.GetInterface(typeof(ICollection<>).FullName) != null
                || t.GetInterface(typeof(IList<>).FullName) != null
                || t.GetInterface(typeof(IEnumerable<>).FullName) != null
            );
            
        }

        public static Type GetTypePropertyOrFieldType(Type t, string name)
        {
            var property = t.GetProperty(name);
            return property != null ? property.PropertyType : t.GetField(name)?.FieldType;
        }

        public static Type GetCollectionType(Type t)
        {
            if (t.IsArray)
            {
                return t.GetElementType();
            }
            return t.GetGenericArguments().FirstOrDefault();
        }
    }
}
