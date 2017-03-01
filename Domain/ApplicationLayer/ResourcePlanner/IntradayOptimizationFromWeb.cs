using System;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationFromWeb: IntradayOptimizationFromWebBase
	{
		public IntradayOptimizationFromWeb(IWebIntradayOptimizationCommandHandler intradayOptimizationCommandHandler,
			IPlanningPeriodRepository planningPeriodRepository, IPersonRepository personRepository)
			: base(intradayOptimizationCommandHandler, planningPeriodRepository, personRepository)
		{
		}

		protected override Guid? SaveJobResultIfNeeded(IPlanningPeriod planningPeriod)
		{
			// no need to save job result
			return null;
		}
	}
}