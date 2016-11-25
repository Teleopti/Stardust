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
	[EnabledBy(Toggles.Wfm_ImportSchedule_41247)]
	public class ImportScheduleHandler :
		IHandleEvent<ImportScheduleEvent>,
		IRunOnHangfire
	{
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IUpdatedBy _updatedBy;
		private readonly ScheduleDictionaryLoadOptions _options = new ScheduleDictionaryLoadOptions(true, true);
		private readonly IJobResultRepository _jobResultRepository;

		public ImportScheduleHandler(IPersonRepository personRepository, IScenarioRepository scenarioRepository, IScheduleStorage scheduleStorage, ICurrentUnitOfWork currentUnitOfWork, IUpdatedBy updatedBy, IJobResultRepository jobResultRepository)
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
		public virtual void Handle(ImportScheduleEvent @event)
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
				var importPeriod = new DateTimePeriod(timeZone.SafeConvertTimeToUtc(period.StartDate.Date), timeZone.SafeConvertTimeToUtc(period.EndDate.Date).AddHours(23).AddMinutes(59));

				importSchedules(sourceScheduleDictionary, toScenario, person, period, importPeriod);
			});

			_jobResultRepository.AddDetailAndCheckSuccess(@event.JobResultId,
				new JobResultDetail(DetailLevel.Info, $"Imported schedules for {@event.PersonIds.Count} people.", DateTime.UtcNow, null),
				@event.TotalMessages);
		}

		private void importSchedules(IScheduleDictionary scheduleDictionary, IScenario toScenario, IPerson person, DateOnlyPeriod period, DateTimePeriod importPeriod)
		{
			var scheduleDays = scheduleDictionary.SchedulesForPeriod(period, person);
			var added = new HashSet<Guid>();

			foreach (var scheduleDay in scheduleDays)
			{
				foreach (var scheduleData in scheduleDay.PersistableScheduleDataCollection())
				{
					var exportableType = scheduleData as IExportToAnotherScenario;
					var changedScheduleData = exportableType?.CloneAndChangeParameters(new ScheduleParameters(toScenario, person,
							importPeriod));
					var absence = changedScheduleData as PersonAbsence;
					if (absence != null)
					{
						var absencePeriod = absence.Period;
						var shouldChange = false;
						if (absencePeriod.StartDateTime > importPeriod.EndDateTime || absencePeriod.EndDateTime < importPeriod.StartDateTime)
							continue;
						if (absencePeriod.StartDateTime < importPeriod.StartDateTime)
						{
							absencePeriod = absencePeriod.ChangeStartTime(importPeriod.StartDateTime - absencePeriod.StartDateTime);
							shouldChange = true;
						}
						if (absencePeriod.EndDateTime > importPeriod.EndDateTime)
						{
							absencePeriod = absencePeriod.ChangeEndTime(importPeriod.EndDateTime - absencePeriod.EndDateTime);
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