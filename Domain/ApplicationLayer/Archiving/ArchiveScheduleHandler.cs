using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Archiving
{
	[EnabledBy(Toggles.Wfm_ArchiveSchedule_41498)]
	public class ArchiveScheduleHandler : 
		IHandleEvent<ArchiveScheduleEvent>, 
		IRunOnHangfire
	{
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IUpdatedBy _updatedBy;
		private readonly ScheduleDictionaryLoadOptions _options = new ScheduleDictionaryLoadOptions(true, true);
		private readonly IJobResultRepository _jobResultRepository;

		public ArchiveScheduleHandler(IPersonRepository personRepository, IScenarioRepository scenarioRepository, IScheduleStorage scheduleStorage, ICurrentUnitOfWork currentUnitOfWork, IUpdatedBy updatedBy, IJobResultRepository jobResultRepository)
		{
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_scheduleStorage = scheduleStorage;
			_currentUnitOfWork = currentUnitOfWork;
			_updatedBy = updatedBy;
			_jobResultRepository = jobResultRepository;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ArchiveScheduleEvent @event)
		{
			_currentUnitOfWork.Current().Reassociate(_updatedBy.Person());
			var period = new DateOnlyPeriod(@event.StartDate, @event.EndDate);

			var fromScenario = _scenarioRepository.Get(@event.FromScenario);
			var toScenario = _scenarioRepository.Get(@event.ToScenario);
			var people = _personRepository.FindPeople(@event.PersonIds);

			var targetScheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(people, _options, period, toScenario);
			var sourceScheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(people, _options, period, fromScenario);

			people.ForEach(person =>
			{
				var timeZone = person.PermissionInformation.DefaultTimeZone();
				var archivePeriod = new DateTimePeriod(timeZone.SafeConvertTimeToUtc(period.StartDate.Date), timeZone.SafeConvertTimeToUtc(period.EndDate.Date).AddHours(23).AddMinutes(59));

				clearCurrentSchedules(targetScheduleDictionary, person, period, archivePeriod);
				archiveSchedules(sourceScheduleDictionary, toScenario, person, period, archivePeriod);
			});

			_jobResultRepository.AddDetailAndCheckSuccess(@event.JobResultId, 
				new JobResultDetail(DetailLevel.Info, $"Archived schedules for {@event.PersonIds.Count} people.", DateTime.UtcNow, null), 
				@event.TotalMessages);
		}

		private void clearCurrentSchedules(IScheduleDictionary scheduleDictionary, IPerson person, DateOnlyPeriod period, DateTimePeriod archivePeriod)
		{
			var scheduleDays = scheduleDictionary.SchedulesForPeriod(period, person);
			var changes = false;
			foreach (var scheduleDay in scheduleDays)
			{
				foreach (var scheduleData in scheduleDay.PersistableScheduleDataCollection())
				{
					if (!(scheduleData is IExportToAnotherScenario)) continue;
					var entity = _scheduleStorage.Get(scheduleData.GetType(), scheduleData.Id.GetValueOrDefault());
					var absence = entity as PersonAbsence;
					if (absence != null)
					{
						if (absence.Period.StartDateTime > archivePeriod.EndDateTime || absence.Period.EndDateTime < archivePeriod.StartDateTime)
							continue;

						if (absence.Period.StartDateTime < archivePeriod.StartDateTime)
						{
							// add the part before archivePeriod.Start
							var newLayer = new AbsenceLayer(absence.Layer.Payload, new DateTimePeriod(absence.Period.StartDateTime, archivePeriod.StartDateTime.AddMinutes(-1)));
							IPersonAbsence personAbsence = new PersonAbsence(absence.Person, absence.Scenario, newLayer, absence.PersonRequest);
							_scheduleStorage.Add(personAbsence);

						}
						if (absence.Period.EndDateTime > archivePeriod.EndDateTime)
						{
							// add the part after archivePeriod.End
							var newLayer = new AbsenceLayer(absence.Layer.Payload, new DateTimePeriod(archivePeriod.EndDateTime.AddMinutes(1), absence.Period.EndDateTime));
							IPersonAbsence personAbsence = new PersonAbsence(absence.Person, absence.Scenario, newLayer, absence.PersonRequest);
							_scheduleStorage.Add(personAbsence);
						}

					}
					_scheduleStorage.Remove(entity);
					changes = true;
				}
			}
			if (changes)
				_currentUnitOfWork.Current().PersistAll();
		}

		private void archiveSchedules(IScheduleDictionary scheduleDictionary, IScenario toScenario, IPerson person, DateOnlyPeriod period, DateTimePeriod archivePeriod)
		{
			var scheduleDays = scheduleDictionary.SchedulesForPeriod(period, person);
			var added = new HashSet<Guid>();

			foreach (var scheduleDay in scheduleDays)
			{
				foreach (var scheduleData in scheduleDay.PersistableScheduleDataCollection())
				{
					var exportableType = scheduleData as IExportToAnotherScenario;
					var changedScheduleData = exportableType?.CloneAndChangeParameters(new ScheduleParameters(toScenario, person,
							archivePeriod));
					var absence = changedScheduleData as PersonAbsence;
					if (absence != null)
					{
						var absencePeriod = absence.Period;
						var shouldChange = false;
						if (absencePeriod.StartDateTime > archivePeriod.EndDateTime || absencePeriod.EndDateTime < archivePeriod.StartDateTime)
							continue;
						if (absencePeriod.StartDateTime < archivePeriod.StartDateTime)
						{
							absencePeriod = absencePeriod.ChangeStartTime(archivePeriod.StartDateTime - absencePeriod.StartDateTime);
							shouldChange = true;
						}
						if (absencePeriod.EndDateTime > archivePeriod.EndDateTime)
						{
							absencePeriod = absencePeriod.ChangeEndTime(archivePeriod.EndDateTime - absencePeriod.EndDateTime);
							shouldChange = true;
						}
							
						if (shouldChange)
							absence.ModifyPersonAbsencePeriod(absencePeriod, null);
					}
					if (changedScheduleData != null && !added.Contains(scheduleData.Id.GetValueOrDefault()))
					{
						added.Add(scheduleData.Id.GetValueOrDefault());
						_scheduleStorage.Add(changedScheduleData);
					}
						
				}
			}
		}
	}
}