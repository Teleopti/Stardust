using System;
using System.Globalization;
using System.Linq;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Convert date and time between current sessions time zone and UTC.
	/// </summary>
	/// <remarks>
	/// Created by: robink
	/// Created date: 2007-10-23
	/// </remarks>
	public static class TimeZoneHelper
	{
		/// <summary>
		/// Converts from UTC.
		/// </summary>
		/// <param name="utcDateTime">The UTC date time.</param>
		/// <param name="sourceTimeZone">The source time zone.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-11-26
		/// </remarks>
		public static DateTime ConvertFromUtc(DateTime utcDateTime, TimeZoneInfo sourceTimeZone)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Unspecified),
											  sourceTimeZone);
		}
	}
}