using Teleopti.Ccc.Web.Areas.MyTime.Core.DaySchedule.Mapping;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.DaySchedule.ViewModelFactory
{
	public interface IScheduleDayMinMaxTimeCalculator
	{
		void AdjustScheduleMinMaxTime(DayScheduleDomainData weekDomainData);
	}
}