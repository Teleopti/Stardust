using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Archiving
{
	[EnabledBy(Toggles.Wfm_ArchiveSchedule_41498)]
	public class ArchiveScheduleHandler : 
		IHandleEvent<ArchiveScheduleEvent>, 
		IRunOnHangfire
	{
		private readonly ITrackingMessageSender _trackingMessageSender;
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IScheduleStorage _scheduleStorage;

		public ArchiveScheduleHandler(ITrackingMessageSender trackingMessageSender, IPersonRepository personRepository, IScenarioRepository scenarioRepository, IScheduleStorage scheduleStorage)
		{
			_trackingMessageSender = trackingMessageSender;
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_scheduleStorage = scheduleStorage;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ArchiveScheduleEvent @event)
		{
			var period = new DateOnlyPeriod(new DateOnly(@event.StartDate), new DateOnly(@event.EndDate));
			var person = _personRepository.Get(@event.PersonId);
			var fromScenario = _scenarioRepository.Get(@event.FromScenario);
			var toScenario = _scenarioRepository.Get(@event.ToScenario);
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(true, true),
				period, fromScenario);
			var scheduleDays = scheduleDictionary.SchedulesForPeriod(period, person);
			foreach (var scheduleDay in scheduleDays)
			{
				foreach (var scheduleData in scheduleDay.PersistableScheduleDataCollection())
				{
					var exportableType = scheduleData as IExportToAnotherScenario;
					var changedScheduleData = exportableType?.CloneAndChangeParameters(new ScheduleParameters(toScenario, person, period.ToDateTimePeriod(TimeZoneInfo.Utc)));
					if (changedScheduleData != null)
						_scheduleStorage.Add(changedScheduleData);
				}
			}

			_trackingMessageSender.SendTrackingMessage(@event, new TrackingMessage
			{
				Status = TrackingMessageStatus.Success,
				TrackId = @event.TrackingId
			});
		}
	}
}