using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class RtaProcessor
	{
		private readonly IDatabaseReader _databaseReader;
		private readonly IAlarmFinder _alarmFinder;
		private readonly AgentStateAssembler _agentStateAssembler;
		private readonly IShiftEventPublisher _shiftEventPublisher;
		private readonly IActivityEventPublisher _activityEventPublisher;
		private readonly IStateEventPublisher _stateEventPublisher;

		public RtaProcessor(IDatabaseReader databaseReader,
			IAlarmFinder alarmFinder,
			AgentStateAssembler agentStateAssembler,
			IShiftEventPublisher shiftEventPublisher,
			IActivityEventPublisher activityEventPublisher,
			IStateEventPublisher stateEventPublisher
			)
		{
			_databaseReader = databaseReader;
			_alarmFinder = alarmFinder;
			_agentStateAssembler = agentStateAssembler;
			_shiftEventPublisher = shiftEventPublisher;
			_activityEventPublisher = activityEventPublisher;
			_stateEventPublisher = stateEventPublisher;
		}

		public void Process(
			RtaProcessContext context
			)
		{
			if (context.Person == null)
				return;
			var person = context.Person;
			var input = context.Input;

			var scheduleInfo = new ScheduleInfo(_databaseReader, context.Person.PersonId, context.CurrentTime);
			var agentStateInfo = new AgentStateInfo(() => context.MakePreviousState(scheduleInfo), () => context.MakeCurrentState(scheduleInfo));
			var adherenceInfo = new AdherenceInfo(input, person, agentStateInfo, scheduleInfo, _alarmFinder);
			var info = new StateInfo(person, agentStateInfo, scheduleInfo, adherenceInfo);
			
			context.AgentStateReadModelUpdater.Update(info);
			context.MessageSender.Send(info);
			context.AdherenceAggregator.Aggregate(info);
			_shiftEventPublisher.Publish(info);
			_activityEventPublisher.Publish(info);
			_stateEventPublisher.Publish(info);
		}
	}

	public class RtaProcessContext
	{
		private readonly IDatabaseReader _databaseReader;
		private readonly AgentStateAssembler _agentStateAssembler;
		private readonly ICurrentEventPublisher _eventPublisher;
		private readonly IResolve _resolve;
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
			ICurrentEventPublisher eventPublisher,
			IResolve resolve
			)
		{
			_databaseReader = databaseReader;
			_agentStateAssembler = agentStateAssembler;
			_eventPublisher = eventPublisher;
			_resolve = resolve;
			if (!personOrganizationProvider.PersonOrganizationData().TryGetValue(personId, out _person))
				return;
			_person.BusinessUnitId = businessUnitId;
			Input = input ?? new ExternalUserStateInputModel();
			CurrentTime = currentTime;

			AgentStateReadModelUpdater = agentStateReadModelUpdater;
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

		public void PublisEventsTo(object handler)
		{
			((CurrentEventPublisher)_eventPublisher).UseThisPlease(new SpecificEventPublisher(handler));
		}
	}

	public class SpecificEventPublisher : IEventPublisher
	{
		private readonly object _handler;

		public SpecificEventPublisher(object handler)
		{
			_handler = handler;
		}

		public void Publish(IEvent @event)
		{
			var method = _handler.GetType().GetMethods()
				.FirstOrDefault(m => m.Name == "Handle" && m.GetParameters().Single().ParameterType == @event.GetType());
			if (method == null)
				return;
			try
			{
				method.Invoke(_handler, new[] { @event });
			}
			catch (TargetInvocationException e)
			{
				preserveStackTrace(e.InnerException);
				throw e;
			}
		}
		
		private static void preserveStackTrace(Exception e)
		{
			var ctx = new StreamingContext(StreamingContextStates.CrossAppDomain);
			var mgr = new ObjectManager(null, ctx);
			var si = new SerializationInfo(e.GetType(), new FormatterConverter());

			e.GetObjectData(si, ctx);
			mgr.RegisterObject(e, 1, si); // prepare for SetObjectData
			mgr.DoFixups(); // ObjectManager calls SetObjectData

			// voila, e is unmodified save for _remoteStackTraceString
		}
	}
}