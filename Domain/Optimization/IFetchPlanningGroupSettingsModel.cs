using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IFetchPlanningGroupSettingsModel
	{
		IEnumerable<PlanningGroupSettingsModel> FetchAllWithoutPlanningGroup();
		PlanningGroupSettingsModel Fetch(Guid id);
		IEnumerable<PlanningGroupSettingsModel> FetchAllForPlanningGroup(Guid planningGroupId);
	}
}