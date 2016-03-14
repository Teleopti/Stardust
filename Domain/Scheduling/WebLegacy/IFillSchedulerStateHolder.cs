using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public interface IFillSchedulerStateHolder
	{
		WebSchedulingSetupResult Fill(ISchedulerStateHolder schedulerStateHolder, IEnumerable<Guid> agentIds, DateOnlyPeriod period);
	}
}