using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
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

		public DenormalizeScheduleDayMessageConsumer(IUnitOfWorkFactory unitOfWorkFactory, IScenarioRepository scenarioRepository,
			IPersonRepository personRepository, IScheduleDayReadModelsCreator scheduleDayReadModelsCreator)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
		}
		public void Consume(DenormalizeScheduleDayMessage message)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var scenario = _scenarioRepository.Get(message.ScenarioId);
				if (!scenario.DefaultScenario) return;

				var period = new DateTimePeriod(message.StartDateTime, message.EndDateTime);
				var person = _personRepository.Get(message.PersonId);

				// Get list of readmodels from class that fetch for person and period and turn into list of readmodels
				// _scheduleDayReadModelsCreator
				var readModels = _scheduleDayReadModelsCreator.GetReadModels(scenario, period, person);
				// send list to class that deletes current (if not skip delete???) and insert new
				//if (message.SkipDelete)
				//{
				//    _updateScheduleDayReadModel.SetInitialLoad(true);
				//}
				//_updateScheduleDayReadModel.Execute(scenario, period, person);
				unitOfWork.PersistAll();
			}
		}
	}
}