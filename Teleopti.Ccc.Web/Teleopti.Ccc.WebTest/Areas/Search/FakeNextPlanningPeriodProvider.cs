using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Search
{
	public class FakeNextPlanningPeriodProvider : INextPlanningPeriodProvider
	{
		private readonly IPlanningPeriod _planningPeriod;

		public FakeNextPlanningPeriodProvider()
		{
			
		}

		public FakeNextPlanningPeriodProvider(IPlanningPeriod planningPeriod)
		{
			_planningPeriod = planningPeriod;
		}
		public IPlanningPeriod Current(IAgentGroup agentGroup)
		{
			return new PlanningPeriod(new DateOnlyPeriod(2015,06,10,2015,07,10)).WithId();
		}

		public IPlanningPeriod Next(SchedulePeriodForRangeCalculation schedulePeriodForRangeCalculation, IAgentGroup agentGroup)
		{
			throw new NotImplementedException();
		}

		public IPlanningPeriod Find(Guid id)
		{
			return _planningPeriod;
		}

		public IEnumerable<SchedulePeriodType> SuggestedPeriods()
		{
			throw new NotImplementedException();
		}

		public IPlanningPeriod Update(SchedulePeriodType periodType, int periodRange)
		{
			throw new NotImplementedException();
		}
	}
}