using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DBConverter
{
	//don't dare to change much - this just replace all old LocalStart/EndTime to "correct" ones
	public static class CorrectLocalDates
	{
		public static DateTime LocalStartDateTime(this DateTimePeriod period, TimeZoneInfo timeZoneInfo)
		{
			return TimeZoneHelper.ConvertFromUtc(period.StartDateTime, timeZoneInfo);
		}
		public static DateTime LocalEndDateTime(this DateTimePeriod period, TimeZoneInfo timeZoneInfo)
		{
			return TimeZoneHelper.ConvertFromUtc(period.EndDateTime, timeZoneInfo);
		}
	}
}