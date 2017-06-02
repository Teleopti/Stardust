using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class BackoutScheduleChangeCommandHandler : IHandleCommand<BackoutScheduleChangeCommand>
	{
		private readonly IScheduleHistoryRepository _scheduleHistoryRepository;
		private readonly IPersonRepository _personRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IAggregateRootInitializer _aggregateRootInitializer;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceService;
		private readonly IAuditSettingRepository _auditSettingRepository;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly IEventPublisher _eventPublisher;
		private readonly ICurrentDataSource _currentDataSource;

		public BackoutScheduleChangeCommandHandler(IScheduleHistoryRepository scheduleHistoryRepository, IPersonRepository personRepository, ILoggedOnUser loggedOnUser, IAggregateRootInitializer aggregateRootInitializer, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IScheduleDifferenceSaver scheduleDifferenceSaver, IDifferenceCollectionService<IPersistableScheduleData> differenceService, IAuditSettingRepository auditSettingRepository, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate, IEventPublisher eventPublisher, ICurrentDataSource currentDataSource)
		{
			_scheduleHistoryRepository = scheduleHistoryRepository;
			_personRepository = personRepository;
			_loggedOnUser = loggedOnUser;
			_aggregateRootInitializer = aggregateRootInitializer;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_differenceService = differenceService;
			_auditSettingRepository = auditSettingRepository;
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_eventPublisher = eventPublisher;
			_currentDataSource = currentDataSource;
		}


		private DateOnlyPeriod findAbsencePeriod(IRevision prevVersion, IRevision nextVersion, IPerson person, DateOnly date)
		{			
			var scheduleData = new[] {prevVersion, nextVersion}.SelectMany(v => _scheduleHistoryRepository.FindSchedules(v, person, date));
			var periods = new List<DateOnlyPeriod> {new DateOnlyPeriod(date, date)};
			
			foreach(var data in scheduleData)
			{				
				var personAbsence = data as IPersonAbsence;
				if (personAbsence != null)
				{
					periods.Add( personAbsence.Period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()));					
				}
			}

			return new DateOnlyPeriod( periods.Min( p => p.StartDate), periods.Max( p => p.EndDate));
		}


		public void Handle(BackoutScheduleChangeCommand command)
		{
			command.ErrorMessages = new List<string>();

			if (!_auditSettingRepository.Read().IsScheduleEnabled)
			{
				command.ErrorMessages.Add(Resources.ScheduleAuditTrailIsNotRunning);
				return;
			}

			var person = _personRepository.Get(command.PersonId);

			var currentUser = _loggedOnUser.CurrentUser();
			var currentUserId = currentUser.Id;
			var versionsOnDates = command.Dates.Select(date => new versionsOnDate
			{
				Date = date,
				Versions = _scheduleHistoryRepository.FindRevisions(person,date,2).ToList(),
			}).Where(vd => vd.Versions.Count == 2 && vd.Versions.First().ModifiedBy.Id == currentUserId).ToList();
			
			if (versionsOnDates.Count == 0)
			{
				command.ErrorMessages.Add(Resources.CannotUndoScheduleChange);
				return;
			}

			var latestModifyTime = versionsOnDates.Max(vd => vd.Versions.First().ModifiedAt);
			var targetVersionsDate = versionsOnDates.First(vd => vd.Versions.First().ModifiedAt == latestModifyTime);

			var preVersion = targetVersionsDate.Versions.First();
			var lastVersion = targetVersionsDate.Versions.Last();			

			var scheduleData = _scheduleHistoryRepository.FindSchedules(lastVersion, person, targetVersionsDate.Date).ToList();
			IList<IPersonAssignment> personAssignments = new List<IPersonAssignment>();
			IList<IPersonAbsence> personAbsences = new List<IPersonAbsence>();

			foreach(var data in scheduleData)
			{
				var personAssignment = data as IPersonAssignment;
				var personAbsence = data as IPersonAbsence;

				if(personAssignment != null) personAssignments.Add(personAssignment);
				if(personAbsence != null) personAbsences.Add(personAbsence);
			}

			if(!personAssignments.IsEmpty())
			{
				_aggregateRootInitializer.Initialize(personAssignments);				
			}

			if(!personAbsences.IsEmpty())
			{
				_aggregateRootInitializer.Initialize(personAbsences);
			}

			var periodForPersonalAccountCheck = findAbsencePeriod(preVersion, lastVersion, person, targetVersionsDate.Date);
			var datePeriod =
				targetVersionsDate.Date.ToDateTimePeriod(currentUser.PermissionInformation.DefaultTimeZone());

			var scheduleDictionary = getScheduleDictionary(person, periodForPersonalAccountCheck);

			var sd = toScheduleDay(getCurrentScheduleDay(scheduleDictionary, person,targetVersionsDate.Date), scheduleData);


			var businessRulesForPersonAccountUpdate = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleDictionary[person]);

			scheduleDictionary.Modify( sd , businessRulesForPersonAccountUpdate, true);		

			var range = scheduleDictionary[person];
			var diffsForTargetDate = new DifferenceCollection<IPersistableScheduleData>();

			scheduleDictionary[person].DifferenceSinceSnapshot(_differenceService).Where(diff =>
			{
				var originalPersonAbsence = diff.OriginalItem as IPersonAbsence;
				return originalPersonAbsence == null || originalPersonAbsence.Period.Intersect(datePeriod);
			}).ForEach(diff =>
			{
				diffsForTargetDate.Add(diff);
			});

			diffsForTargetDate.ForEach(diff =>
			{
				var eventHolder = diff.CurrentItem as AggregateRoot;
				eventHolder?.NotifyCommandId(command.TrackedCommandInfo.TrackId);
			});

			
			_scheduleDifferenceSaver.SaveChanges(diffsForTargetDate,(IUnvalidatedScheduleRangeUpdate)range);

			var affectedData = diffsForTargetDate.Select(x => x.OriginalItem).Where(d => d != null).Concat(diffsForTargetDate.Select(x => x.CurrentItem).Where(d => d != null));

			var scenario = _currentScenario.Current();
			_eventPublisher.Publish(new ScheduleBackoutEvent
			{
				PersonId = command.PersonId,
				ScenarioId = scenario.Id.GetValueOrDefault(),
				StartDateTime = affectedData.Min(s => s.Period.StartDateTime),
				EndDateTime = affectedData.Max(s => s.Period.EndDateTime),
				InitiatorId = command.TrackedCommandInfo.OperatedPersonId,
				CommandId = command.TrackedCommandInfo.TrackId,
				LogOnBusinessUnitId = scenario.BusinessUnit.Id.Value,
				LogOnDatasource = _currentDataSource.Current().DataSourceName
			});
		}

		private IScheduleDictionary getScheduleDictionary(IPerson person, DateOnlyPeriod period)
		{
			var dic = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
				person,
				new ScheduleDictionaryLoadOptions(true, true),
				period,
				_currentScenario.Current());
			((IReadOnlyScheduleDictionary) dic).MakeEditable();

			return dic;
		}

		private IScheduleDay getCurrentScheduleDay(IScheduleDictionary scheduleDictionary, IPerson person, DateOnly date)
		{
			var period = new DateOnlyPeriod(date, date);		
			return scheduleDictionary[person].ScheduledDayCollection(period).SingleOrDefault();
		}

		private IScheduleDay toScheduleDay(IScheduleDay currentScheduleDay, IEnumerable<IPersistableScheduleData> data)
		{			
			currentScheduleDay.ReFetch();
			currentScheduleDay.Clear<IPersonAbsence>();
			var resultingAss = currentScheduleDay.PersonAssignment(true);
			resultingAss.Clear(true);

			foreach(var scheduleData in data)
			{
				var newAss = scheduleData as IPersonAssignment;
				if(newAss != null)
				{
					resultingAss.FillWithDataFrom(newAss, true);
				}
				else
				{
					currentScheduleDay.Add(scheduleData.CreateTransient());
				}
			}

			return currentScheduleDay;			
		}

		private class versionsOnDate
		{
			public IList<IRevision> Versions;
			public DateOnly Date;
		}
	}
}
