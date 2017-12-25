using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ScheduleChangedNotifierHangfire :
		IHandleEvent<ScheduleChangedEvent>,
		IRunOnHangfire
	{
		private readonly IMessageCreator _broker;
		private readonly IScenarioRepository _scenarioRepository;

		public ScheduleChangedNotifierHangfire(IMessageCreator broker, IScenarioRepository scenarioRepository)
		{
			_broker = broker;
			_scenarioRepository = scenarioRepository;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ScheduleChangedEvent @event)
		{
			if (@event.SkipDelete) return;

			_broker.Send(
				@event.LogOnDatasource,
				@event.LogOnBusinessUnitId,
				@event.StartDateTime,
				@event.EndDateTime,
				@event.InitiatorId,
				@event.ScenarioId,
				typeof(Common.Scenario),
				@event.PersonId,
				typeof(IScheduleChangedEvent),
				DomainUpdateType.NotApplicable,
				null, _scenarioRepository.Get(@event.ScenarioId).DefaultScenario);
		}
	}
}