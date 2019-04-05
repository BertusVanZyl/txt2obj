using System;

namespace txt2obj.Extentions
{
    public static class StringExtentions
    {
        public static bool IsSet(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }
    }
}
