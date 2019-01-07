using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IFetchPlanningGroupSettingsModel
	{
		PlanningGroupSettingsModel Fetch(Guid id);
	}
}