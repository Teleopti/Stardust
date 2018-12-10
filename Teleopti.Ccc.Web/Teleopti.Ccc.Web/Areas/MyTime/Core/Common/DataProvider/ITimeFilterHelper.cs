using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface ITimeFilterHelper
	{
		TimeFilterInfo GetFilter(DateOnly selectedDate, string filterStartTimes, string filterEndTimes, bool isDayOff, bool isEmptyDay);
		TimeFilterInfo GetTeamSchedulesFilter(DateOnly selectedDate,ScheduleFilter scheduleFilter);
	}
}