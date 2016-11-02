using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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


		public BackoutScheduleChangeCommandHandler(IScheduleHistoryRepository scheduleHistoryRepository, IPersonRepository personRepository, ILoggedOnUser loggedOnUser, IAggregateRootInitializer aggregateRootInitializer, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IScheduleDifferenceSaver scheduleDifferenceSaver, IDifferenceCollectionService<IPersistableScheduleData> differenceService, IAuditSettingRepository auditSettingRepository)
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

			var versionsOnDates = command.Dates.Select(date => new versionsOnDate
			{
				Date = date,
				Versions = _scheduleHistoryRepository.FindRevisions(person, date, 2).ToList()
			}).Where(vd => vd.Versions.Count == 2 && vd.Versions.First().ModifiedBy.Id == _loggedOnUser.CurrentUser().Id).ToList();
			
			if (versionsOnDates.Count == 0)
			{
				command.ErrorMessages.Add(Resources.CannotUndoScheduleChange);
				return;
			}

			var latestModifyTime = versionsOnDates.Max(vd => vd.Versions.First().ModifiedAt);
			var targetVersionsDate = versionsOnDates.First(vd => vd.Versions.First().ModifiedAt == latestModifyTime); 

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

			var scheduleDictionary = getScheduleDictionary(person,targetVersionsDate.Date);

			var sd = toScheduleDay(getCurrentScheduleDay(scheduleDictionary, person,targetVersionsDate.Date), scheduleData);

			var errorResponses = scheduleDictionary.Modify(sd, new DoNothingScheduleDayChangeCallBack()).ToList();

			if (errorResponses.Count > 0)
			{
				command.ErrorMessages = errorResponses.Select(x => x.Message).ToList();
				return;
			}

			var range = scheduleDictionary[person];
			var diffs = scheduleDictionary[person].DifferenceSinceSnapshot(_differenceService);

			diffs.ForEach(diff =>
			{
				var eventHolder = diff.CurrentItem as AggregateRoot;
				eventHolder?.NotifyCommandId(command.TrackedCommandInfo.TrackId);
			});

			_scheduleDifferenceSaver.SaveChanges(diffs,(IUnvalidatedScheduleRangeUpdate)range);			
		}

		private IScheduleDictionary getScheduleDictionary(IPerson person, DateOnly date)
		{
			var dic = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
				person,
				new ScheduleDictionaryLoadOptions(true, true),
				new DateOnlyPeriod(date, date),
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
			resultingAss.Clear();

			foreach(var scheduleData in data)
			{
				var newAss = scheduleData as IPersonAssignment;
				if(newAss != null)
				{
					resultingAss.FillWithDataFrom(newAss);
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
