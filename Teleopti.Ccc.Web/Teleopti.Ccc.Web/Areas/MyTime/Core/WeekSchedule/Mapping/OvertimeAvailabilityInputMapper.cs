using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping
{
	public class OvertimeAvailabilityInputMapper
	{
		private readonly ILoggedOnUser _loggedOnUser;

		public OvertimeAvailabilityInputMapper(ILoggedOnUser loggedOnUser)
		{
			_loggedOnUser = loggedOnUser;
		}

		public IOvertimeAvailability Map(OvertimeAvailabilityInput s)
		{
			return new OvertimeAvailability(_loggedOnUser.CurrentUser(), s.Date, s.StartTime.ToTimeSpan(),
				s.EndTime.ToTimeSpan(s.EndTimeNextDay));
		}
	}
}