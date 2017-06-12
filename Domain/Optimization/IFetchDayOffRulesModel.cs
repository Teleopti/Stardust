using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IFetchDayOffRulesModel
	{
		IEnumerable<DayOffRulesModel> FetchAllWithoutPlanningGroup();
		DayOffRulesModel Fetch(Guid id);
		IEnumerable<DayOffRulesModel> FetchAllForPlanningGroup(Guid planningGroupId);
	}
}