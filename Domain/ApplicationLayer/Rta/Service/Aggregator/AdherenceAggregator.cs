using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator
{
	public interface IAdherenceAggregator
	{
		void Aggregate(IAdherenceAggregatorInfo state);
		void Initialize();
	}

	public class NoAggregation : IAdherenceAggregator
	{
		public void Aggregate(IAdherenceAggregatorInfo state)
		{
		}

		public void Initialize()
		{
		}
	}

	public class AdherenceAggregator : IAdherenceAggregator
	{
		private readonly IMessageSender _messageSender;
		private readonly IAgentStateReadModelReader _agentStateReadModelReader;
		private readonly IPersonLoader _personLoader;
		private readonly TeamAdherenceAggregator _teamAdherenceAggregator;
		private readonly SiteAdherenceAggregator _siteAdherenceAggregator;
		private readonly AgentAdherenceAggregator _agentAdherenceAggregator;
		private readonly AggregationState _aggregationState;

		public AdherenceAggregator(
			IMessageSender messageSender, 
			IAgentStateReadModelReader agentStateReadModelReader,
			IPersonLoader personLoader,
			IJsonSerializer jsonSerializer
			)
		{
			_messageSender = messageSender;
			_agentStateReadModelReader = agentStateReadModelReader;
			_personLoader = personLoader;
			_aggregationState = new AggregationState();
			_teamAdherenceAggregator = new TeamAdherenceAggregator(_aggregationState, jsonSerializer);
			_siteAdherenceAggregator = new SiteAdherenceAggregator(_aggregationState, jsonSerializer);
			_agentAdherenceAggregator = new AgentAdherenceAggregator(_aggregationState, jsonSerializer);
		}

		public void Aggregate(IAdherenceAggregatorInfo state)
		{
			aggregate(state, true);
		}

		public void Initialize()
		{
			var persons = _personLoader.LoadAllPersonsData();
			foreach (var actualAgentState in _agentStateReadModelReader.GetActualAgentStates())
			{
				var person = persons.SingleOrDefault(x => x.PersonId == actualAgentState.PersonId);
				if (person == null)
					continue;

				var adherenceAggregatorInfo = new AdherenceAggregatorInfo(actualAgentState, person)
				{
					AggregatorAdherence = AdherenceInfo.AggregatorAdherence(actualAgentState)
				};
				aggregate(adherenceAggregatorInfo, false);
			}
		}

		private void aggregate(IAdherenceAggregatorInfo state, bool sendMessages)
		{
			var adherenceChanged = _aggregationState.Update(state);

			if (!sendMessages)
				return;

			var agentsAdherences = _agentAdherenceAggregator.CreateNotification(state);
			if (agentsAdherences != null)
				agentsAdherences.ForEach(_messageSender.Send);

			if (!adherenceChanged)
				return;

			var siteAdherence = _siteAdherenceAggregator.CreateNotification(state);
			if (siteAdherence != null)
				_messageSender.Send(siteAdherence);

			var teamAdherence = _teamAdherenceAggregator.CreateNotification(state);
			if (teamAdherence != null)
				_messageSender.Send(teamAdherence);

		}

	}

}