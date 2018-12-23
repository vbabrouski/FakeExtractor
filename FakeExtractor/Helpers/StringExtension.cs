using System;
using System.Text;

namespace FakeExtractor.Helpers
{
    public static class StringExtension
    {
        public static bool EqualTo(this string source, string target)
        {
            return string.Equals(source, target, StringComparison.OrdinalIgnoreCase);
        }

        public static bool StartsWithOrdinal(this string source, string start)
        {
            if (source == null ||
                start == null)
            {
                return false;
            }

            return source.StartsWith(start, StringComparison.Ordinal);
        }

        public static bool ContainsOrdinalIgnoreCase(this string source, string value)
        {
            if (source == null ||
                value == null)
            {
                return false;
            }

            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool EndsWithOrdinalIgnoreCase(this string source, string value)
        {
            if (source == null ||
                value == null)
            {
                return false;
            }

            return source.EndsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        public static string ReplaceOrdinalIgnoreCase(this string source, string oldValue, string newValue)
        {
            if (source == null ||
                oldValue == null)
            {
                return source;
            }

            var index = source.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return source;
            }

            return new StringBuilder(source).Remove(index, oldValue.Length).Insert(index, newValue).ToString();
        }
    }
}