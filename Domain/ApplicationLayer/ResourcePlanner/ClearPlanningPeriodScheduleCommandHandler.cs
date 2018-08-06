using System;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class ClearPlanningPeriodScheduleCommandHandler
	{
		private readonly ClearPlanningPeriodSchedule _clearPlanningPeriodSchedule;

		public ClearPlanningPeriodScheduleCommandHandler(ClearPlanningPeriodSchedule clearPlanningPeriodSchedule)
		{
			_clearPlanningPeriodSchedule = clearPlanningPeriodSchedule;
		}

		[UnitOfWork]
		public virtual void ClearSchedules(Guid planningPeriodId)
		{
			_clearPlanningPeriodSchedule.ClearSchedules(planningPeriodId);
		}
	}
}