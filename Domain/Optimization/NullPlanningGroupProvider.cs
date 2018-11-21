using System;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class NullPlanningGroupProvider : IPlanningGroupProvider
	{
		public PlanningGroup Execute(Guid planningPeriodId)
		{
			return null;
		}
	}
}