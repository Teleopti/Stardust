using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public class ScheduleDayReadModelPersister
	{
		private readonly IPersonRepository _personRepository;
		private readonly INotificationValidationCheck _notificationValidationCheck;
		private readonly IScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private readonly ITeamScheduleWeekViewChangeCheck _teamScheduleWeekViewChangeCheck;

		public ScheduleDayReadModelPersister(IPersonRepository personRepository,
			INotificationValidationCheck notificationValidationCheck,
			IScheduleDayReadModelsCreator scheduleDayReadModelsCreator,
			IScheduleDayReadModelRepository scheduleDayReadModelRepository,
			ITeamScheduleWeekViewChangeCheck teamScheduleWeekViewChangeCheck)
		{
			_personRepository = personRepository;
			_notificationValidationCheck = notificationValidationCheck;
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
			_teamScheduleWeekViewChangeCheck = teamScheduleWeekViewChangeCheck;
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

					if (message.IsDefaultScenario)
						_teamScheduleWeekViewChangeCheck.InitiateNotify(new ScheduleChangeForWeekViewEvent
						{
							LogOnBusinessUnitId = message.LogOnBusinessUnitId,
							LogOnDatasource = message.LogOnDatasource,
							Date = date,
							Person = person,
							NewReadModel = readModel
						});

					_scheduleDayReadModelRepository.ClearPeriodForPerson(dateOnlyPeriod, message.PersonId);
				}
				_scheduleDayReadModelRepository.SaveReadModel(readModel);
			}
		}
	}
}