using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
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
			var newReadModelDicByDate = message.ScheduleDays
			.ToDictionary(scheduleDay => new DateOnly(scheduleDay.Date), scheduleDay => _scheduleDayReadModelsCreator.GetReadModel(scheduleDay, person));

			if (message.IsDefaultScenario && !message.IsInitialLoad)
				_teamScheduleWeekViewChangeCheck.InitiateNotify(new ScheduleChangeForWeekViewEvent
				{
					LogOnBusinessUnitId = message.LogOnBusinessUnitId,
					LogOnDatasource = message.LogOnDatasource,
					Person = person,
					NewReadModels = newReadModelDicByDate
				});

			newReadModelDicByDate.ForEach(readModel =>
			{
				var date = readModel.Key;
				var dateOnlyPeriod = new DateOnlyPeriod(date, date);
				if (!message.IsInitialLoad)
				{
					_notificationValidationCheck.InitiateNotify(readModel.Value, readModel.Key, person);
					_scheduleDayReadModelRepository.ClearPeriodForPerson(dateOnlyPeriod, message.PersonId);
				}
				_scheduleDayReadModelRepository.SaveReadModel(readModel.Value);
			});


		}
	}
}