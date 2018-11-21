using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public interface IPlanningGroupStaffLoader
	{
		PeopleSelection Load(DateOnlyPeriod period, PlanningGroup planningGroup);
		int NumberOfAgents(DateOnlyPeriod period, PlanningGroup planningGroup);
		IList<Guid> LoadPersonIds(DateOnlyPeriod period, PlanningGroup planningGroup);
	}
}