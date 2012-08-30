using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class InitialLoadScheduleDayConsumer : ConsumerOf<InitialLoadScheduleDay>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IPersonRepository _personRepository;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IServiceBus _serviceBus;

		public InitialLoadScheduleDayConsumer(IUnitOfWorkFactory unitOfWorkFactory, IPersonRepository personRepository, 
			IPersonAssignmentRepository personAssignmentRepository, IScenarioRepository scenarioRepository, IServiceBus serviceBus)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_personRepository = personRepository;
			
			_personAssignmentRepository = personAssignmentRepository;
			_scenarioRepository = scenarioRepository;
			_serviceBus = serviceBus;
		}

		public void Consume(InitialLoadScheduleDay message)
		{
			IEnumerable<DenormalizeScheduleDayMessage> messages = new DenormalizeScheduleDayMessage[] { };
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				//if (!_scheduleProjectionReadOnlyRepository.IsInitialized())
				{
					if (hasAssignments())
					{
						messages = initialLoad(message);
					}
				}
				uow.Clear();
			}
			messages.ForEach(m => _serviceBus.Send(m));
		}

		private IEnumerable<DenormalizeScheduleDayMessage> initialLoad(InitialLoadScheduleDay message)
		{
			var people = _personRepository.LoadAll();
			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(3652));
			var utcPeriod = period.ToDateTimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone);

			return people.Select(
				p =>
				new DenormalizeScheduleDayMessage
					{
						BusinessUnitId = message.BusinessUnitId,
						Datasource = message.Datasource,
						PersonId = p.Id.GetValueOrDefault(),
						ScenarioId = defaultScenario.Id.GetValueOrDefault(),
						Timestamp = DateTime.UtcNow,
						StartDateTime = utcPeriod.StartDateTime,
						EndDateTime = utcPeriod.EndDateTime,
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
