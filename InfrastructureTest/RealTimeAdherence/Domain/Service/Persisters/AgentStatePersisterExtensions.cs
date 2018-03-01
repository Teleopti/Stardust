using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.Domain.Service.Persisters
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