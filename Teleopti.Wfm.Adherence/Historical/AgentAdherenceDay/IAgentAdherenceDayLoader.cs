using System;

namespace Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay
{
	public interface IAgentAdherenceDayLoader
	{
		IAgentAdherenceDay LoadUntilNow(Guid personId, DateOnly date);
		IAgentAdherenceDay Load(Guid personId, DateOnly date);
	}
}