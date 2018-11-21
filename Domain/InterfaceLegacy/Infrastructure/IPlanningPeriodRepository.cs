using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IPlanningPeriodRepository : IRepository<PlanningPeriod>
	{
		IPlanningPeriodSuggestions Suggestions(INow now);
		IPlanningPeriodSuggestions Suggestions(INow now, ICollection<Guid> personIds);
		IEnumerable<PlanningPeriod> LoadForPlanningGroup(PlanningGroup planningGroup);
	}
}