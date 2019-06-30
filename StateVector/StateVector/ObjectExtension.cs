using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StateVector
{
    public static class ObjectExtension
    {
        public static bool IsSameInstance(this object objA, object objB)
        {
            return ReferenceEquals(objA, objB);
        }

        public static bool IsMatch(this string pattern, string input)
        {
            return Regex.IsMatch(input, pattern);
        }

        public static T To<T>(this string str)
            where T : struct
        {
            return (T)Convert.ChangeType(str, typeof(T));
        }
    }
}
