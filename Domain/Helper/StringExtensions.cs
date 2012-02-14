using System;

namespace Teleopti.Ccc.Domain.Helper
{
	public static class StringExtensions
	{
		public static bool ContainsIgnoreCase(this string str, string strToSearchFor)
		{
			if (str == null)
			{
				return strToSearchFor == null;
			}
			if (strToSearchFor == null)
				return false;
			return str.IndexOf(strToSearchFor, StringComparison.OrdinalIgnoreCase) >= 0;
		}
	}
}