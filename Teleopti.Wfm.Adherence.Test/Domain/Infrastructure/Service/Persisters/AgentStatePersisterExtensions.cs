using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Test.Domain.Infrastructure.Service.Persisters
{
	public static class AgentStatePersisterExtensions
	{
		public static IEnumerable<AgentState> ReadForTest(this IAgentStatePersister instance)
		{
			return instance.LockNLoad(instance.FindForCheck().Select(x => x.PersonId), DeadLockVictim.Yes).AgentStates;
		}

		public static IEnumerable<AgentState> ReadForTest(this IAgentStatePersister instance, IEnumerable<Guid> personIds)
		{
			return instance.LockNLoad(personIds, DeadLockVictim.Yes).AgentStates;
		}

		public static IEnumerable<AgentState> ReadForTest(this IAgentStatePersister instance, Guid personId)
		{
			return instance.LockNLoad(new [] { personId }, DeadLockVictim.Yes).AgentStates;
		}
	}
}