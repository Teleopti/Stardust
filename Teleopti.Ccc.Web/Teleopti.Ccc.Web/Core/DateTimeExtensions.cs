using System;

namespace Teleopti.Ccc.Web.Core
{
	public static class DateTimeExtensions
	{
		private static readonly DateTime jsBaseDate = new DateTime(1970, 1, 1);

		public static TimeSpan SubtractJavascriptBaseDate(this DateTime dateTime)
		{
			return dateTime.Subtract(jsBaseDate);
		}
	}
}