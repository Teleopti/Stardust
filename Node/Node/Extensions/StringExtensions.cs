using System;

namespace Stardust.Node.Extensions
{
    public static class StringExtensions
    {
        public static void ThrowArgumentExceptionIfNullOrEmpty(this string stringValue)
        {
            if (stringValue.IsNullOrEmpty())
            {
                throw new ArgumentException();
            }
        }

        public static void ThrowArgumentNullExceptionIfNullOrEmpty(this string stringValue)
        {
            if (stringValue.IsNullOrEmpty())
            {
                throw new ArgumentNullException();
            }
        }

        public static bool IsNullOrEmpty(this string stringValue)
        {
            return string.IsNullOrEmpty(stringValue);
        }

        public static bool HasValue(this string stringValue)
        {
            return !stringValue.IsNullOrEmpty();
        }
    }
}