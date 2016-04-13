using System;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IScheduleOptimization
	{
		OptimizationResultModel Execute(Guid planningPeriodId);
	}
}