using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public interface ITimeLineViewModelFactory
	{
		TeamScheduleTimeLineViewModel[] CreateTimeLineHours(DateTimePeriod timeLinePeriod, DateOnly viewDate);
	}

	public interface ITimeLineViewModelFactoryToggle75989Off
	{
		TeamScheduleTimeLineViewModelToggle75989Off[] CreateTimeLineHours(DateTimePeriod timeLinePeriod);
	}
}