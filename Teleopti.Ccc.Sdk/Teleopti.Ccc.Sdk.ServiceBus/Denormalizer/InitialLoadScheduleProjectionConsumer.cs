using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class InitialLoadScheduleProjectionConsumer : ConsumerOf<InitialLoadScheduleProjection>
	{
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private readonly IPersonScheduleDayReadModelPersister _personScheduleDayReadModelRepository;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IServiceBus _serviceBus;
		private IList<IPerson> _people;
		private IScenario _defaultScenario;
		private DateOnlyPeriod _period;
		private DateTimePeriod _utcPeriod;

		public InitialLoadScheduleProjectionConsumer(ICurrentUnitOfWorkFactory unitOfWorkFactory, IPersonRepository personRepository, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository, IScheduleDayReadModelRepository scheduleDayReadModelRepository, IPersonScheduleDayReadModelPersister personScheduleDayReadModelRepository, IPersonAssignmentRepository personAssignmentRepository, ICurrentScenario scenarioRepository, IServiceBus serviceBus)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_personRepository = personRepository;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_personAssignmentRepository = personAssignmentRepository;
			_scenarioRepository = scenarioRepository;
			_serviceBus = serviceBus;
		}

		public void Consume(InitialLoadScheduleProjection message)
		{
			var messages = new List<ScheduleChangedEventBase>();
			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var projectionModelInitialized = _scheduleProjectionReadOnlyRepository.IsInitialized();
				var scheduleDayModelInitialized = _scheduleDayReadModelRepository.IsInitialized();
				var personScheduleDayModelInitialized = _personScheduleDayReadModelRepository.IsInitialized();
				
				if (projectionModelInitialized && scheduleDayModelInitialized && personScheduleDayModelInitialized) return;

				loadPeopleAndScenario(message.StartDays, message.EndDays);

				if (!projectionModelInitialized && !scheduleDayModelInitialized && !personScheduleDayModelInitialized)
				{
					messages.AddRange(initialLoad<ScheduleChangedEvent>(message));

					projectionModelInitialized = true;
					scheduleDayModelInitialized = true;
					personScheduleDayModelInitialized = true;
				}
				if (!projectionModelInitialized)
				{
					messages.AddRange(initialLoad<ScheduleInitializeTriggeredEventForScheduleProjection>(message));
				}
				if (!scheduleDayModelInitialized)
				{
					messages.AddRange(initialLoad<ScheduleInitializeTriggeredEventForScheduleDay>(message));
				}
				if (!personScheduleDayModelInitialized)
				{
					messages.AddRange(initialLoad<ScheduleInitializeTriggeredEventForPersonScheduleDay>(message));
				}
				uow.Clear();
			}
			messages.ForEach(m => _serviceBus.Send(m));
		}

		private void loadPeopleAndScenario(int startDays, int endDays)
		{
			_people = _personRepository.LoadAll();
			_defaultScenario = _scenarioRepository.Current();
			_period = new DateOnlyPeriod(DateOnly.Today.AddDays(startDays), DateOnly.Today.AddDays(endDays));
			_utcPeriod = _period.ToDateTimePeriod(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
		}

		private IEnumerable<T> initialLoad<T>(InitialLoadScheduleProjection message) where T : ScheduleChangedEventBase, new()
		{
			return _people.Select(
				p =>
				new T
					{
						BusinessUnitId = message.BusinessUnitId,
						Datasource = message.Datasource,
						PersonId = p.Id.GetValueOrDefault(),
						ScenarioId = _defaultScenario.Id.GetValueOrDefault(),
						Timestamp = DateTime.UtcNow,
						StartDateTime = _utcPeriod.StartDateTime,
						EndDateTime = _utcPeriod.EndDateTime,
						SkipDelete = true
					}).ToArray();
		}
	}
}
