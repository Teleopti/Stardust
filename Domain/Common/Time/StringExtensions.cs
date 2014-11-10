using System;
using System.Globalization;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public static class StringExtensions
	{
		public static DateTime Utc(this string dateTimeString)
		{
			return DateTime.SpecifyKind(DateTime.Parse(dateTimeString, CultureInfo.GetCultureInfo("sv-SE")), DateTimeKind.Utc);
		}
	}
}