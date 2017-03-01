using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public interface IIntradayOptimizationFromWeb
	{
		void Execute(Guid planningPeriodId);
	}
}