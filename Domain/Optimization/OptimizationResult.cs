using System;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationResult
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

		public OptimizationResult(Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_schedulerStateHolder = schedulerStateHolder;
		}

		[LogTime]
		public virtual OptimizationResultModel Create(DateOnlyPeriod period)
		{
			var resultStateHolder = _schedulerStateHolder().SchedulingResultState;
			var result = new OptimizationResultModel();
			if (resultStateHolder.SkillDays != null)
			{
				result.Map(resultStateHolder.SkillDays, period);
			}
			return result;
		}
	}
}