using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAdherenceAggregatorInfo
	{
		PersonOrganizationData Person { get; }
		EventAdherence AggregatorAdherence { get; }
		AgentStateReadModel MakeAgentStateReadModel();
	}

	public class AdherenceAggregatorInfo : IAdherenceAggregatorInfo
	{
		private readonly AgentStateReadModel _agentStateReadModel;

		public AdherenceAggregatorInfo(AgentStateReadModel agentStateReadModel, PersonOrganizationData person)
		{
			_agentStateReadModel = agentStateReadModel;
			Person = person;
		}
		
		public PersonOrganizationData Person { get; private set; }
		public EventAdherence AggregatorAdherence { get; set; }

		public AgentStateReadModel MakeAgentStateReadModel()
		{
			return _agentStateReadModel;
		}

	}
}