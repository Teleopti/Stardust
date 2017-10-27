using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public interface IFillSchedulerStateHolder
	{
		void Fill(ISchedulerStateHolder schedulerStateHolderTo, 
			IEnumerable<Guid> agentsInIsland,
			IEnumerable<Guid> choosenAgents,
			LockInfoForStateHolder lockInfoForStateHolder, 
			DateOnlyPeriod period, 
			IEnumerable<Guid> onlyUseSkills = null);
	}
}