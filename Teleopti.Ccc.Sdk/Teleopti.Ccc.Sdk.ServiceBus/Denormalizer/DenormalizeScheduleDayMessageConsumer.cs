using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class DenormalizeScheduleDayMessageConsumer: ConsumerOf<DenormalizeScheduleDayMessage>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;

		public DenormalizeScheduleDayMessageConsumer(IUnitOfWorkFactory unitOfWorkFactory, IScenarioRepository scenarioRepository,
			IPersonRepository personRepository, IScheduleDayReadModelsCreator scheduleDayReadModelsCreator,IScheduleDayReadModelRepository scheduleDayReadModelRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
		}
		
		public void Consume(DenormalizeScheduleDayMessage message)
		{
			if (message != null)
			{
				using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
				{
					var scenario = _scenarioRepository.Get(message.ScenarioId);
					if (!scenario.DefaultScenario) return;

					var period = new DateTimePeriod(message.StartDateTime, message.EndDateTime);
					var person = _personRepository.Get(message.PersonId);

					// Get list of readmodels from class that fetch for person and period and turn into list of readmodels
					if (message.SkipDelete)
					{
						_scheduleDayReadModelsCreator.SetInitialLoad(true);
					}
					else
					{
						_scheduleDayReadModelRepository.ClearPeriodForPerson(
							period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()), message.PersonId);
					}
					var readModels = _scheduleDayReadModelsCreator.GetReadModels(scenario, period, person);
					// save them
					if (readModels.Count > 0)
					{
						_scheduleDayReadModelRepository.SaveReadModels(readModels);
					}
				}
			}
		}
	}
}