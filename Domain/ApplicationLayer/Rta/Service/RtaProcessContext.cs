using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator;
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
			IAgentStateMessageSender messageSender, 
			IAdherenceAggregator adherenceAggregator, 
			IPreviousStateInfoLoader previousStateInfoLoader)
		{
			Person = person;
			Input = input ?? new ExternalUserStateInputModel();
			CurrentTime = now.UtcDateTime();

			PreviousStateInfoLoader = previousStateInfoLoader;
			AgentStateReadModelUpdater = agentStateReadModelUpdater ?? new DontUpdateAgentStateReadModel();
			MessageSender = messageSender ?? new NoMessage();
			AdherenceAggregator = adherenceAggregator ?? new NoAggregation();
		}

		public PersonOrganizationData Person { get; private set; }
		public ExternalUserStateInputModel Input { get; private set; }
		public DateTime CurrentTime { get; set; }

		public IPreviousStateInfoLoader PreviousStateInfoLoader { get; private set; }
		public IAgentStateReadModelUpdater AgentStateReadModelUpdater { get; private set; }
		public IAgentStateMessageSender MessageSender { get; private set; }
		public IAdherenceAggregator AdherenceAggregator { get; private set; }

	}
}