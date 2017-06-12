using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public interface IPlanningGroupStaffLoader
	{
		PeopleSelection Load(DateOnlyPeriod period, IPlanningGroup planningGroup);
		int NumberOfAgents(DateOnlyPeriod period, IPlanningGroup planningGroup);
		IList<Guid> LoadPersonIds(DateOnlyPeriod period, IPlanningGroup planningGroup);
	}
}