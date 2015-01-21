using System;
using System.Collections.Generic;
using System.Diagnostics;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
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
		private readonly IDatabaseReader _databaseReader;
		private readonly AgentStateAssembler _agentStateAssembler;
		private readonly IEventPublisherScope _eventPublisherScope;
		private readonly IEnumerable<IInitializeble> _initializebles;
		private readonly IEnumerable<IRecreatable> _recreatables;

		public StateStreamSynchronizer(
			INow now,
			RtaProcessor processor,
			IPersonOrganizationProvider personOrganizationProvider,
			IDatabaseReader databaseReader,
			AgentStateAssembler agentStateAssembler,
			IEventPublisherScope eventPublisherScope,
			IEnumerable<IInitializeble> initializebles,
			IEnumerable<IRecreatable> recreatables
			)
		{
			_now = now;
			_processor = processor;
			_personOrganizationProvider = personOrganizationProvider;
			_databaseReader = databaseReader;
			_agentStateAssembler = agentStateAssembler;
			_eventPublisherScope = eventPublisherScope;
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
			using (_eventPublisherScope.OnThisThreadPublishTo(new SyncPublishToSingleHandler(handler)))
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
						null, 
						null,
						() => _agentStateAssembler.MakeEmpty(s.PersonId),
						(a, b) => _agentStateAssembler.MakeCurrentStateFromPrevious(s)
						);
					_processor.Process(context);
				});
			}
		}
	}

}