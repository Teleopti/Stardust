using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public static class DateTimePeriodFormMapper
	{
		public static DateTimePeriod Map(this DateTimePeriodForm s, IUserTimeZone userTimeZone)
		{
			if (s == null)
				return new DateTimePeriod();
			var fromTime = s.StartDate.Date.Add(s.StartTime.Time);
			var toTime = s.EndDate.Date.Add(s.EndTime.Time);
			return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(fromTime,toTime,userTimeZone.TimeZone());
		}
	}
}