using System;
using System.Collections.Generic;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface INextPlanningPeriodProvider
	{
		IPlanningPeriod Current();
		IPlanningPeriod Find(Guid id);
		IEnumerable<SchedulePeriodType> SuggestedPeriods();
	}
}