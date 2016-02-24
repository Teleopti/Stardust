using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class RtaProcessContext
	{
		public RtaProcessContext(
			ExternalUserStateInputModel input, 
			PersonOrganizationData person, 
			INow now, 
			IAgentStateReadModelUpdater agentStateReadModelUpdater, 
			IPreviousStateInfoLoader previousStateInfoLoader)
		{
			Person = person;
			Input = input ?? new ExternalUserStateInputModel();
			CurrentTime = now.UtcDateTime();

			PreviousStateInfoLoader = previousStateInfoLoader;
			AgentStateReadModelUpdater = agentStateReadModelUpdater ?? new DontUpdateAgentStateReadModel();
		}

		public PersonOrganizationData Person { get; private set; }
		public ExternalUserStateInputModel Input { get; private set; }
		public DateTime CurrentTime { get; set; }

		public IPreviousStateInfoLoader PreviousStateInfoLoader { get; private set; }
		public IAgentStateReadModelUpdater AgentStateReadModelUpdater { get; private set; }

	}
}