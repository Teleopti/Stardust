using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class DateTimePeriodFormMapper
	{
		private readonly IUserTimeZone _userTimeZone;

		public DateTimePeriodFormMapper(IUserTimeZone userTimeZone)
		{
			_userTimeZone = userTimeZone;
		}

		public DateTimePeriod Map(DateTimePeriodForm s)
		{
			if (s == null)
				return new DateTimePeriod();
			var fromTime = s.StartDate.Date.Add(s.StartTime.Time);
			var toTime = s.EndDate.Date.Add(s.EndTime.Time);
			var fromTimeUtc = TimeZoneHelper.ConvertToUtc(fromTime, _userTimeZone.TimeZone());
			var toTimeUtc = TimeZoneHelper.ConvertToUtc(toTime, _userTimeZone.TimeZone());
			return new DateTimePeriod(fromTimeUtc, toTimeUtc);
		}
	}
}