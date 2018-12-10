using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public class SeatManagementProviderUtils
	{
		public static DateTimePeriod GetUtcDateTimePeriodForLocalFullDay (DateOnly date, TimeZoneInfo userTimeZoneInfo)
		{
			var localStartOfDay = date.Date;
			var localEndOfDay = date.Date.AddDays (1).AddSeconds (-1);
			var dateTimePeriodUtc = new DateTimePeriod (
				TimeZoneHelper.ConvertToUtc(localStartOfDay, userTimeZoneInfo),
				TimeZoneHelper.ConvertToUtc(localEndOfDay, userTimeZoneInfo)
				);
			return dateTimePeriodUtc;
		}
	}
}