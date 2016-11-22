using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
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
			var period = new DateOnlyPeriod(new DateOnly(@event.StartDate), new DateOnly(@event.EndDate));

			var fromScenario = _scenarioRepository.Get(@event.FromScenario);
			var toScenario = _scenarioRepository.Get(@event.ToScenario);
			var people = _personRepository.FindPeople(@event.PersonIds);

			var targetScheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(people, _options, period, toScenario);
			var sourceScheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(people, _options, period, fromScenario);

			people.ForEach(person =>
			{
				clearCurrentSchedules(targetScheduleDictionary, person, period);
				archiveSchedules(sourceScheduleDictionary, toScenario, person, period);
			});

			_jobResultRepository.AddDetailAndCheckSuccess(@event.JobResultId, 
				new JobResultDetail(DetailLevel.Info, $"Archived schedules for {@event.PersonIds.Count} people.", DateTime.UtcNow, null), 
				@event.TotalMessages);
		}

		private void clearCurrentSchedules(IScheduleDictionary scheduleDictionary, IPerson person, DateOnlyPeriod period)
		{
			var scheduleDays = scheduleDictionary.SchedulesForPeriod(period, person);
			var changes = false;
			foreach (var scheduleDay in scheduleDays)
			{
				foreach (var scheduleData in scheduleDay.PersistableScheduleDataCollection())
				{
					if (!(scheduleData is IExportToAnotherScenario)) continue;
					var entity = _scheduleStorage.Get(scheduleData.GetType(), scheduleData.Id.GetValueOrDefault());
					_scheduleStorage.Remove(entity);
					changes = true;
				}
			}
			if (changes)
				_currentUnitOfWork.Current().PersistAll();
		}

		private void archiveSchedules(IScheduleDictionary scheduleDictionary, IScenario toScenario, IPerson person, DateOnlyPeriod period)
		{
			var scheduleDays = scheduleDictionary.SchedulesForPeriod(period, person);

			foreach (var scheduleDay in scheduleDays)
			{
				foreach (var scheduleData in scheduleDay.PersistableScheduleDataCollection())
				{
					var exportableType = scheduleData as IExportToAnotherScenario;
					var changedScheduleData = exportableType?.CloneAndChangeParameters(new ScheduleParameters(toScenario, person,
							period.ToDateTimePeriod(TimeZoneInfo.Utc)));
					if (changedScheduleData != null)
						_scheduleStorage.Add(changedScheduleData);
				}
			}
		}
	}
}