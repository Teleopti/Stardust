using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay
{
	public interface IAgentAdherenceDayLoader
	{
		IAgentAdherenceDay Load(Guid personId, DateOnly date);
	}
}