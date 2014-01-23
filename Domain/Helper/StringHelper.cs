using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Helper
{
    /// <summary>
    /// Various String functions to make your life easier.
    /// </summary>
    /// <remarks>
    /// Created by: henryg
    /// Created date: 2007-12-13
    /// </remarks>
    public static class StringHelper
    {
        /// <summary>
        /// Capitalizes the specified string paragraph.
        /// </summary>
        /// <param name="paragraph">The string paragraph.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2007-12-13
        /// </remarks>
        public static string Capitalize(string paragraph)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(paragraph);
        }

        /// <summary>
        /// Splits the specified to be splitted.
        /// </summary>
        /// <param name="toBeSplit">To be splitted.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2009-01-15
        /// </remarks>
        public static string[] Split(this string toBeSplit, string separator)
        {
            return toBeSplit.Split(new[] {separator}, StringSplitOptions.None);
        }

		public static string DisplayString(string input, int wordlength)
		{
			char space = ' ';
			string dotsInEnd = "...";

			var lth = input.Length;
			if (lth < wordlength)
			{
				return input.PadRight(wordlength);
			}
			if (lth > wordlength)
			{
				var trimEndSpaces = input.TrimEnd(space);
				if (trimEndSpaces != input)
				{
					return DisplayString(trimEndSpaces, wordlength);
				}
				var subStr = trimEndSpaces.Substring(0, (wordlength - 3));
				var trimEndAgain = subStr.TrimEnd(space);
				if (trimEndAgain.Length == 0)
				{
					return string.Empty.PadRight(wordlength-3) + dotsInEnd;
				}
				return string.Concat(trimEndAgain, dotsInEnd);
			}
			return input;
		}

		public static Guid GenerateGuid(string note)
		{
			using (var md5 = MD5.Create())
			{
				var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(note));
				var guid = new Guid(hash);
				return guid;
			}
		}
    }

    public static class DateExtensions
    {
        public static DateTime LimitMin(this DateTime dateTime)
        {
            if (DateHelper.MinSmallDateTime > dateTime)
                return DateHelper.MinSmallDateTime;

            return dateTime;
        }
    }
}
