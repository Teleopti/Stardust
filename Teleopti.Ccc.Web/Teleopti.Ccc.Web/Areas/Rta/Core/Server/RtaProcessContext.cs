using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class RtaProcessContext
	{
		private readonly IDatabaseReader _databaseReader;
		private readonly AgentStateAssembler _agentStateAssembler;
		private readonly ICurrentEventPublisherContext _publishingContext;
		private readonly PersonOrganizationData _person;

		private Lazy<AgentState> _previousState;

		public RtaProcessContext(
			ExternalUserStateInputModel input,
			Guid personId,
			Guid businessUnitId,
			DateTime currentTime,
			IPersonOrganizationProvider personOrganizationProvider,
			IAgentStateReadModelUpdater agentStateReadModelUpdater, 
			IAgentStateMessageSender messageSender, 
			IAdherenceAggregator adherenceAggregator,
			IDatabaseReader databaseReader,
			AgentStateAssembler agentStateAssembler,
			ICurrentEventPublisherContext publishingContext
			)
		{
			_databaseReader = databaseReader;
			_agentStateAssembler = agentStateAssembler;
			_publishingContext = publishingContext;
			if (!personOrganizationProvider.PersonOrganizationData().TryGetValue(personId, out _person))
				return;
			_person.BusinessUnitId = businessUnitId;
			Input = input ?? new ExternalUserStateInputModel();
			CurrentTime = currentTime;

			AgentStateReadModelUpdater = agentStateReadModelUpdater ?? new DontUpdateAgentStateReadModel();
			MessageSender = messageSender;
			AdherenceAggregator = adherenceAggregator;

			_previousState = new Lazy<AgentState>(() => _agentStateAssembler.MakePreviousState(Person.PersonId, _databaseReader.GetCurrentActualAgentState(Person.PersonId)));
		}

		public ExternalUserStateInputModel Input { get; private set; }
		public PersonOrganizationData Person { get { return _person; } }
		public DateTime CurrentTime { get; private set; }

		public IAgentStateReadModelUpdater AgentStateReadModelUpdater { get; private set; }
		public IAgentStateMessageSender MessageSender { get; private set; }
		public IAdherenceAggregator AdherenceAggregator { get; private set; }

		public AgentState MakePreviousState(ScheduleInfo scheduleInfo)
		{
			return _previousState.Value;
		}

		private Func<AgentState> _makeCurrentState; 

		public AgentState MakeCurrentState(ScheduleInfo scheduleInfo)
		{
			if (_makeCurrentState == null)
				_makeCurrentState = () => _agentStateAssembler.MakeCurrentState(scheduleInfo, Input, Person, _previousState.Value, CurrentTime);
			return _makeCurrentState.Invoke();
		}

		public void SetPreviousMakeMethodToReturnEmptyState()
		{
			_previousState = new Lazy<AgentState>(() => _agentStateAssembler.MakeEmpty(Person.PersonId));
		}

		public void SetCurrentMakeMethodToReturnPreviousState(AgentStateReadModel previousState)
		{
			_makeCurrentState = () => _agentStateAssembler.MakeCurrentStateFromPrevious(previousState);
		}

		public void PublishEventsTo(object handler)
		{
			_publishingContext.PublishTo(new SyncPublishToSingleHandler(handler));
		}
	}
}