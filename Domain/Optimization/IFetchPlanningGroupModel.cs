using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IFetchPlanningGroupModel
	{
		IEnumerable<PlanningGroupModel> FetchAll();
		PlanningGroupModel Fetch(Guid id);
	}
}