using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public interface IStateStreamSynchronizer
	{
		void Sync();
		void Initialize();
	}

	public class StateStreamSynchronizer : IStateStreamSynchronizer
	{
		private readonly INow _now;
		private readonly RtaProcessor _processor;
		private readonly IPersonOrganizationProvider _personOrganizationProvider;
		private readonly IAgentStateMessageSender _agentStateMessageSender;
		private readonly IAdherenceAggregator _adherenceAggregator;
		private readonly IDatabaseReader _databaseReader;
		private readonly AgentStateAssembler _agentStateAssembler;
		private readonly ICurrentEventPublisherContext _eventPublisher;
		private readonly IEnumerable<IInitializeble> _initializebles;
		private readonly IEnumerable<IRecreatable> _recreatables;

		public StateStreamSynchronizer(
			INow now,
			RtaProcessor processor,
			IPersonOrganizationProvider personOrganizationProvider,
			IAgentStateMessageSender agentStateMessageSender,
			IAdherenceAggregator adherenceAggregator,
			IDatabaseReader databaseReader,
			AgentStateAssembler agentStateAssembler,
			ICurrentEventPublisherContext eventPublisher,
			IEnumerable<IInitializeble> initializebles,
			IEnumerable<IRecreatable> recreatables
			)
		{
			_now = now;
			_processor = processor;
			_personOrganizationProvider = personOrganizationProvider;
			_agentStateMessageSender = agentStateMessageSender;
			_adherenceAggregator = adherenceAggregator;
			_databaseReader = databaseReader;
			_agentStateAssembler = agentStateAssembler;
			_eventPublisher = eventPublisher;
			_initializebles = initializebles;
			_recreatables = recreatables;
		}

		public void Sync()
		{
			var currentTime = _now.UtcDateTime();
			var states = _databaseReader.GetActualAgentStates();
			_recreatables.ForEach(s =>
			{
				s.DeleteAll();
				processStatesTo(s, states, currentTime);
			});
		}

		public void Initialize()
		{
			var currentTime = _now.UtcDateTime();
			var states = _databaseReader.GetActualAgentStates();
			_initializebles.ForEach(s =>
			{
				if (!s.Initialized())
					processStatesTo(s, states, currentTime);
			});
		}

		private void processStatesTo(object handler, IEnumerable<AgentStateReadModel> states, DateTime currentTime)
		{
			states.ForEach(s =>
				{
					var context = new RtaProcessContext(
						null,
						s.PersonId,
						s.BusinessUnitId,
						currentTime,
						_personOrganizationProvider,
						null,
						_agentStateMessageSender,
						_adherenceAggregator,
						_databaseReader,
						_agentStateAssembler,
						_eventPublisher
						);
					context.SetPreviousMakeMethodToReturnEmptyState();
					context.SetCurrentMakeMethodToReturnPreviousState(s);
					context.PublishEventsTo(handler);
					_processor.Process(context);
				});
		}
	}

}