using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string json, string prefix, StringComparison comp)
        {
            return source?.IndexOf(json, prefix) >= 0;
        }
    }
}