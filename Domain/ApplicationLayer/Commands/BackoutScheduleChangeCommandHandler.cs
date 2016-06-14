using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
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


		public BackoutScheduleChangeCommandHandler(IScheduleHistoryRepository scheduleHistoryRepository, IPersonRepository personRepository, ILoggedOnUser loggedOnUser, IAggregateRootInitializer aggregateRootInitializer, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IScheduleDifferenceSaver scheduleDifferenceSaver, IDifferenceCollectionService<IPersistableScheduleData> differenceService)
		{
			_scheduleHistoryRepository = scheduleHistoryRepository;
			_personRepository = personRepository;
			_loggedOnUser = loggedOnUser;
			_aggregateRootInitializer = aggregateRootInitializer;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_differenceService = differenceService;
		}

		public void Handle(BackoutScheduleChangeCommand command)
		{
			command.ErrorMessages = new List<string>();
			var person = _personRepository.Get(command.PersonId);
			var versions = _scheduleHistoryRepository.FindRevisions(person,command.Date,2).ToList();

			if(versions.Count != 2)
			{
				command.ErrorMessages.Add(Resources.CannotBackoutScheduleChange);
				return;
			}

			var version = versions.Last();

			if (version.ModifiedBy.Id != _loggedOnUser.CurrentUser().Id)
			{
				command.ErrorMessages.Add(Resources.CannotBackoutScheduleChange);
				return;
			}

			var scheduleData = _scheduleHistoryRepository.FindSchedules(version, person, command.Date).ToList();
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

			var scheduleDictionary = getScheduleDictionary(person, command.Date);

			var sd = toScheduleDay(getCurrentScheduleDay(scheduleDictionary, person, command.Date), scheduleData);

			var errorResponses = scheduleDictionary.Modify(sd).ToList();

			if (errorResponses.Count > 0)
			{
				command.ErrorMessages = errorResponses.Select(x => x.Message).ToList();
				return;
			}

			var range = scheduleDictionary[person];
			var diff = scheduleDictionary[person].DifferenceSinceSnapshot(_differenceService);
			_scheduleDifferenceSaver.SaveChanges(diff,(IUnvalidatedScheduleRangeUpdate)range);			
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
	}
}
