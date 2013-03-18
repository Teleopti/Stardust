﻿using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class InitialLoadScheduleProjectionConsumer : ConsumerOf<InitialLoadScheduleProjection>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private readonly IPersonScheduleDayReadModelRepository _personScheduleDayReadModelRepository;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IServiceBus _serviceBus;
		private IList<IPerson> _people;
		private IScenario _defaultScenario;
		private DateOnlyPeriod _period;
		private DateTimePeriod _utcPeriod;

		public InitialLoadScheduleProjectionConsumer(IUnitOfWorkFactory unitOfWorkFactory,IPersonRepository personRepository, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository, IScheduleDayReadModelRepository scheduleDayReadModelRepository, IPersonScheduleDayReadModelRepository personScheduleDayReadModelRepository, IPersonAssignmentRepository personAssignmentRepository, IScenarioRepository scenarioRepository, IServiceBus serviceBus)
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
			var messages = new List<ScheduleDenormalizeBase>();
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var projectionModelInitialized = _scheduleProjectionReadOnlyRepository.IsInitialized();
				var scheduleDayModelInitialized = _scheduleDayReadModelRepository.IsInitialized();
				var personScheduleDayModelInitialized = _personScheduleDayReadModelRepository.IsInitialized();
				
				if (projectionModelInitialized && scheduleDayModelInitialized && personScheduleDayModelInitialized) return;
				if (!hasAssignments()) return;

				loadPeopleAndScenario();

				if (!projectionModelInitialized && !scheduleDayModelInitialized && !personScheduleDayModelInitialized)
				{
					messages.AddRange(initialLoad<ScheduleChanged>(message));

					projectionModelInitialized = true;
					scheduleDayModelInitialized = true;
					//personScheduleDayModelInitialized = true;
				}
				if (!projectionModelInitialized)
				{
					messages.AddRange(initialLoad<ScheduleProjectionInitialize>(message));
				}
				if (!scheduleDayModelInitialized)
				{
					messages.AddRange(initialLoad<ScheduleDayInitialize>(message));
				}
				/*if (!personScheduleDayModelInitialized)
				{
					messages.AddRange(initialLoad<PersonScheduleDayInitialize>(message));
				}*/
				uow.Clear();
			}
			messages.ForEach(m => _serviceBus.Send(m));
		}

		private void loadPeopleAndScenario()
		{
			_people = _personRepository.LoadAll();
			_defaultScenario = _scenarioRepository.LoadDefaultScenario();
			_period = new DateOnlyPeriod(DateOnly.Today.AddDays(-3652), DateOnly.Today.AddDays(3652));
			_utcPeriod = _period.ToDateTimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone);
		}

		private IEnumerable<T> initialLoad<T>(InitialLoadScheduleProjection message) where T : ScheduleDenormalizeBase, new()
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

		private bool hasAssignments()
		{
			var entitiesCount = _personAssignmentRepository.CountAllEntities();
			return (entitiesCount > 0);
		}
	}
}
