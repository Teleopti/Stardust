using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay
{
	public interface IAgentAdherenceDayLoader
	{
		IAgentAdherenceDay LoadUntilNow(Guid personId, DateOnly date);
		IAgentAdherenceDay Load(Guid personId, DateOnly date);
	}
}