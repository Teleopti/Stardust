using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public abstract class FillSchedulerStateHolder : IFillSchedulerStateHolder
	{
		public void Fill(ISchedulerStateHolder schedulerStateHolder, IEnumerable<Guid> agentIds, DateOnlyPeriod period)
		{
			PreFill(schedulerStateHolder);
			var agents = FillAgents(schedulerStateHolder, agentIds, period);
			//filtrera bort on�diga agenter h�r
			FillSkillDays(schedulerStateHolder, agents, period);
			//filtrera bort on�diga skilldays h�r
			FillSchedules(schedulerStateHolder, agents, period);
			//filtrera bort on�diga scheman h�r
			PostFill(schedulerStateHolder, agents, period);
		}

		protected abstract IEnumerable<IPerson> FillAgents(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIds, DateOnlyPeriod period);
		protected abstract void FillSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<IPerson> agents, DateOnlyPeriod period);
		protected abstract void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<IPerson> agents, DateOnlyPeriod period);
		protected abstract void PreFill(ISchedulerStateHolder schedulerStateHolderTo);
		protected abstract void PostFill(ISchedulerStateHolder schedulerStateHolder, IEnumerable<IPerson> agents, DateOnlyPeriod period);
	}
}