using System;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingAndDayOffOptimization
	{
		private readonly FullScheduling _fullScheduling;
		private readonly DayOffOptimizationWeb _dayOffOptimizationWeb;

		public SchedulingAndDayOffOptimization(FullScheduling fullScheduling, DayOffOptimizationWeb dayOffOptimizationWeb)
		{
			_fullScheduling = fullScheduling;
			_dayOffOptimizationWeb = dayOffOptimizationWeb;
		}

		public void Execute(Guid planningPeriodId)
		{
			_fullScheduling.DoScheduling(planningPeriodId);
			_dayOffOptimizationWeb.Execute(planningPeriodId);
		}
	}
}