using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ManageSchedule
{
	public class ImportScheduleHandler : ScheduleManagementHandlerBase,
		IHandleEvent<ImportScheduleEvent>,
		IRunOnStardust
	{
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IUpdatedBy _updatedBy;
		private readonly ScheduleDictionaryLoadOptions _options = new ScheduleDictionaryLoadOptions(true, true);
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IPersonAccountUpdater _personAccountUpdater;

		public ImportScheduleHandler(IPersonRepository personRepository, IScenarioRepository scenarioRepository, IScheduleStorage scheduleStorage, ICurrentUnitOfWork currentUnitOfWork, IUpdatedBy updatedBy, IJobResultRepository jobResultRepository, IPersonAccountUpdater personAccountUpdater)
		{
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_scheduleStorage = scheduleStorage;
			_currentUnitOfWork = currentUnitOfWork;
			_updatedBy = updatedBy;
			_jobResultRepository = jobResultRepository;
			_personAccountUpdater = personAccountUpdater;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(ImportScheduleEvent @event)
		{
			_currentUnitOfWork.Current().Reassociate(_updatedBy.Person() as IAggregateRoot);
			var period = new DateOnlyPeriod(@event.StartDate, @event.EndDate);

			var fromScenario = _scenarioRepository.Get(@event.FromScenario);
			var toScenario = _scenarioRepository.Get(@event.ToScenario);
			var people = _personRepository.FindPeople(@event.PersonIds);

			var targetScheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(people, _options, period, toScenario);
			var sourceScheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(people, _options, period, fromScenario);

			people.ForEach(person =>
			{
				var timeZone = person.PermissionInformation.DefaultTimeZone();
				var importPeriod = new DateTimePeriod(timeZone.SafeConvertTimeToUtc(period.StartDate.Date), timeZone.SafeConvertTimeToUtc(period.EndDate.Date).AddHours(23).AddMinutes(59));
				clearSchedules(targetScheduleDictionary, person, period);
				importSchedules(sourceScheduleDictionary, targetScheduleDictionary, toScenario, person, period, importPeriod);
			});

			_jobResultRepository.AddDetailAndCheckSuccess(@event.JobResultId,
				new JobResultDetail(DetailLevel.Info, $"Imported schedules for {@event.PersonIds.Count()} people.", DateTime.UtcNow, null),
				@event.TotalMessages);
		}

		private void clearSchedules(IScheduleDictionary scheduleDictionary, IPerson person, DateOnlyPeriod period)
		{
			var scheduleDays = scheduleDictionary.SchedulesForPeriod(period, person);
			var changes = false;
			foreach (var scheduleDay in scheduleDays)
			{
				foreach (var scheduleData in scheduleDay.PersistableScheduleDataCollection())
				{
					if (!(scheduleData is IExportToAnotherScenario) || scheduleData is PersonAbsence || scheduleData is PersonAssignment) continue;
					_scheduleStorage.Remove(_scheduleStorage.Get(scheduleData.GetType(), scheduleData.Id.GetValueOrDefault()));
					changes = true;
				}
			}
			if (changes)
				_currentUnitOfWork.Current().PersistAll();
		}

		private void importSchedules(IScheduleDictionary sourceScheduleDictionary, IScheduleDictionary targetScheduleDictionary, IScenario toScenario, IPerson person, DateOnlyPeriod period, DateTimePeriod importPeriod)
		{
			var scheduleDays = sourceScheduleDictionary.SchedulesForPeriod(period, person).ToList();
			var added = new HashSet<Guid>();
			foreach (var scheduleDay in scheduleDays)
			{
				var targetPersonAss = targetScheduleDictionary[scheduleDay.Person].ScheduledDay(scheduleDay.DateOnlyAsPeriod.DateOnly).PersonAssignment();

				if (scheduleDay.PersistableScheduleDataCollection().IsEmpty())
				{
					if (targetPersonAss != null)
					{
						targetPersonAss.Clear(true);
						_currentUnitOfWork.Current().Merge(targetPersonAss);
					}
				}

				foreach (var scheduleData in scheduleDay.PersistableScheduleDataCollection())
				{
					var exportableType = scheduleData as IExportToAnotherScenario;
					var changedScheduleData = exportableType?.CloneAndChangeParameters(new ScheduleParameters(toScenario, person, importPeriod));

					if (changedScheduleData is PersonAssignment sourcePersonAssignment && targetPersonAss != null)
					{
						targetPersonAss.FillWithDataFrom(sourcePersonAssignment);
						_currentUnitOfWork.Current().Merge(targetPersonAss);
						added.Add(scheduleData.Id.GetValueOrDefault());	
					}

					if (changedScheduleData is PersonAbsence absence)
					{
						if (HandleAbsenceSplits(importPeriod, absence)) continue;

						foreach (var targetAbsence in GetTargetSchedulesForDay(targetScheduleDictionary, period, person, scheduleDay.DateOnlyAsPeriod.DateOnly).OfType<PersonAbsence>())
						{
							if (absence.Period == targetAbsence.Period && absence.Layer.Payload.Id == targetAbsence.Layer.Payload.Id)
							{
								// already exists, don't save it
								added.Add(scheduleData.Id.GetValueOrDefault());
							}
						}
					}
					if (changedScheduleData != null && !added.Contains(scheduleData.Id.GetValueOrDefault()))
					{
						added.Add(scheduleData.Id.GetValueOrDefault());
						_scheduleStorage.Add(changedScheduleData);
						var changedAbsence = changedScheduleData as PersonAbsence;
						if (changedAbsence?.Layer.Payload.Tracker != null)
						{
							_currentUnitOfWork.Current().PersistAll();
							_personAccountUpdater.UpdateForAbsence(person, changedAbsence.Layer.Payload, new DateOnly(changedAbsence.Period.StartDateTime));
						}
					}
				}
			}
		}
	}
}