using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.Service.AgentState
{
	public static class AgentStatePersisterExtensions
	{
		public static IEnumerable<Adherence.States.AgentState> ReadForTest(this IAgentStatePersister instance)
		{
			return instance.LockNLoad(instance.FindForCheck().Select(x => x.PersonId), DeadLockVictim.Yes).AgentStates;
		}

		public static IEnumerable<Adherence.States.AgentState> ReadForTest(this IAgentStatePersister instance, IEnumerable<Guid> personIds)
		{
			return instance.LockNLoad(personIds, DeadLockVictim.Yes).AgentStates;
		}

		public static IEnumerable<Adherence.States.AgentState> ReadForTest(this IAgentStatePersister instance, Guid personId)
		{
			return instance.LockNLoad(new [] { personId }, DeadLockVictim.Yes).AgentStates;
		}
	}
}