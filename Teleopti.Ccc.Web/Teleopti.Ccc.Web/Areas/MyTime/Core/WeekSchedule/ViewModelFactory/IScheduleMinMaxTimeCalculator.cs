using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public interface IScheduleWeekMinMaxTimeCalculator
	{
		void AdjustScheduleMinMaxTime(WeekScheduleDomainData weekDomainData);
	}
}