using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Teleopti.Ccc.Domain.Helper
{
	public static class StringExtensions
	{
		const string dotsInEnd = "...";

		public static bool ContainsIgnoreCase(this string str, string strToSearchFor)
		{
			if (str == null || strToSearchFor == null)
			{
				return str == strToSearchFor;
			}
			return str.IndexOf(strToSearchFor, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public static string Capitalize(this string paragraph)
		{
			return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(paragraph);
		}

		public static string[] Split(this string toBeSplit, string separator)
		{
			return toBeSplit.Split(new[] { separator }, StringSplitOptions.None);
		}

		public static string DisplayString(this string input, int wordlength)
		{
			var result = input.PadRight(wordlength);
			var rightPart = result.Substring(wordlength);

			return string.IsNullOrWhiteSpace(rightPart)
				? result.Substring(0, 20)
				: result.Substring(0, (wordlength - dotsInEnd.Length)) + dotsInEnd;
		}

		public static Guid GenerateGuid(this string note)
		{
			using (var md5 = MD5.Create())
			{
				var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(note));
				var guid = new Guid(hash);
				return guid;
			}
		}

		public static DateTime Utc(this string dateTimeString)
		{
			return DateTime.SpecifyKind(DateTime.Parse(dateTimeString, CultureInfo.GetCultureInfo("sv-SE")), DateTimeKind.Utc);
		}
		
		public static DateTime Time(this string dateTimeString)
		{
			return DateTime.Parse(dateTimeString, CultureInfo.GetCultureInfo("sv-SE"));
		}
		
	}
}