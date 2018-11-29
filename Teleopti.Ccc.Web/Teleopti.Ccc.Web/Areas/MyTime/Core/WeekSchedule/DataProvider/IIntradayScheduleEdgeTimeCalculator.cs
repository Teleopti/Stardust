using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public interface IIntradayScheduleEdgeTimeCalculator
	{
		DateTime GetEndTime(DateOnly date);
		DateTime GetStartTime(DateOnly date);
		IntradayScheduleEdgeTime GetSchedulePeriodForCurrentUser(DateOnly date);
	}
}