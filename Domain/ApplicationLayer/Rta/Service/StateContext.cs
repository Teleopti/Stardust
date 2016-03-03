using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateContext
	{
		public StateContext(
			ExternalUserStateInputModel input, 
			Guid personId,
			Guid businessUnitId,
			Guid teamId,
			Guid siteId,
			INow now, 
			IAgentStateReadModelUpdater agentStateReadModelUpdater, 
			IPreviousStateInfoLoader previousStateInfoLoader
			)
		{
			Input = input ?? new ExternalUserStateInputModel();
			CurrentTime = now.UtcDateTime();
			PersonId = personId;
			BusinessUnitId = businessUnitId;
			TeamId = teamId;
			SiteId = siteId;
			PreviousStateInfoLoader = previousStateInfoLoader;
			AgentStateReadModelUpdater = agentStateReadModelUpdater ?? new DontUpdateAgentStateReadModel();
		}

		public ExternalUserStateInputModel Input { get; private set; }

		public Guid PersonId { get; private set; }
		public Guid BusinessUnitId { get; private set; }
		public Guid TeamId { get; private set; }
		public Guid SiteId { get; private set; }

		public DateTime CurrentTime { get; private set; }

		public IPreviousStateInfoLoader PreviousStateInfoLoader { get; private set; }
		public IAgentStateReadModelUpdater AgentStateReadModelUpdater { get; private set; }

		// for logging
		public override string ToString()
		{
			return string.Format(
				"PersonId: {0}, BusinessUnitId: {1}, TeamId: {2}, SiteId: {3}",
				PersonId, BusinessUnitId, TeamId, SiteId);
		}

	}
}