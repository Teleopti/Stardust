using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping
{
	public class OvertimeAvailabilityViewModelMapper
	{
		private readonly IUserCulture _culture;

		public OvertimeAvailabilityViewModelMapper(IUserCulture culture)
		{
			_culture = culture;
		}

		public OvertimeAvailabilityViewModel Map(IOvertimeAvailability s)
		{
			return new OvertimeAvailabilityViewModel
			{
				HasOvertimeAvailability = true,
				StartTime = TimeHelper.TimeOfDayFromTimeSpan(s.StartTime.Value, _culture.GetCulture()),
				EndTime = TimeHelper.TimeOfDayFromTimeSpan(s.EndTime.Value, _culture.GetCulture()),
				EndTimeNextDay = s.EndTime.Value.Days > 0
			};
		}
	}
}