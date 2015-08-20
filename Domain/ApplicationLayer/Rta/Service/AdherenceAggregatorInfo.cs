using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAdherenceAggregatorInfo
	{
		Guid PersonId { get; }
		Guid TeamId { get; }
		Guid SiteId { get; }
		Guid BusinessUnitId { get; }
		AdherenceState AdherenceState { get; }
		AgentStateReadModel MakeAgentStateReadModel();
	}

	public class AdherenceAggregatorInfo : IAdherenceAggregatorInfo
	{
		private readonly AgentStateReadModel _agentStateReadModel;

		public AdherenceAggregatorInfo(AgentStateReadModel agentStateReadModel, PersonOrganizationData person)
		{
			_agentStateReadModel = agentStateReadModel;
			PersonId = person.PersonId;
			TeamId = person.TeamId;
			SiteId = person.SiteId;
			BusinessUnitId = person.BusinessUnitId;
		}

		public Guid PersonId { get; private set; }
		public Guid TeamId { get; private set; }
		public Guid SiteId { get; private set; }
		public Guid BusinessUnitId { get; private set; }
		public AdherenceState AdherenceState { get; set; }

		public AgentStateReadModel MakeAgentStateReadModel()
		{
			return _agentStateReadModel;
		}

	}
}