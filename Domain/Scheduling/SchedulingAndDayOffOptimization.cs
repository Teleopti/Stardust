using System;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingAndDayOffOptimization
	{
		private readonly FullScheduling _fullScheduling;

		public SchedulingAndDayOffOptimization(FullScheduling fullScheduling)
		{
			_fullScheduling = fullScheduling;
		}

		public void Execute(Guid planningPeriodId)
		{
			_fullScheduling.DoScheduling(planningPeriodId);
		}
	}
}