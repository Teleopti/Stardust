using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public class ScheduleDayReadModelPersister
	{
		private readonly IPersonRepository _personRepository;
		private readonly INotificationValidationCheck _notificationValidationCheck;
		private readonly IScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;

		public ScheduleDayReadModelPersister(IPersonRepository personRepository, INotificationValidationCheck notificationValidationCheck, IScheduleDayReadModelsCreator scheduleDayReadModelsCreator, IScheduleDayReadModelRepository scheduleDayReadModelRepository)
		{
			_personRepository = personRepository;
			_notificationValidationCheck = notificationValidationCheck;
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
		}
		
		public void Execute(ProjectionChangedEventBase message)
		{
			if (!message.IsDefaultScenario) return;

			var person = _personRepository.Get(message.PersonId);

			foreach (var denormalizedScheduleDay in message.ScheduleDays)
			{
				var date = new DateOnly(denormalizedScheduleDay.Date);
				var dateOnlyPeriod = new DateOnlyPeriod(date, date);

				var readModel = _scheduleDayReadModelsCreator.GetReadModel(denormalizedScheduleDay, person);

				if (!message.IsInitialLoad)
				{

					_notificationValidationCheck.InitiateNotify(readModel, date, person);

					_scheduleDayReadModelRepository.ClearPeriodForPerson(dateOnlyPeriod, message.PersonId);
				}
				_scheduleDayReadModelRepository.SaveReadModel(readModel);
			}
		}
	}
}