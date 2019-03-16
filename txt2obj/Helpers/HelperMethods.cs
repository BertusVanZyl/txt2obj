using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace txt2obj.Helpers
{
    public class HelperMethods
    {
        public static bool IsSimple(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimple(typeInfo.GetGenericArguments()[0]);
            }
            return typeInfo.IsPrimitive 
                   || typeInfo.IsEnum
                   || type.Equals(typeof(string))
                   || type.Equals(typeof(decimal));
        }
    }
}
