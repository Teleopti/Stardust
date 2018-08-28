using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class FindSchedulesForPersonsInternallyAsync : IFindSchedulesForPersons
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IPersistableScheduleDataPermissionChecker _dataPermissionChecker;
		private readonly IFindPersonAssignment _findPersonAssignment;

		public FindSchedulesForPersonsInternallyAsync(ICurrentUnitOfWork currentUnitOfWork, 
			IRepositoryFactory repositoryFactory, 
			IPersistableScheduleDataPermissionChecker dataPermissionChecker,
			IFindPersonAssignment findPersonAssignment)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_repositoryFactory = repositoryFactory;
			_dataPermissionChecker = dataPermissionChecker;
			_findPersonAssignment = findPersonAssignment;
		}

		public IScheduleDictionary FindSchedulesForPersons(IScenario scenario, IEnumerable<IPerson> personsInOrganization,
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, DateTimePeriod visiblePeriod,
			IEnumerable<IPerson> visiblePersons, bool extendPeriodBasedOnVisiblePersons)
		{
			var periodBasedOnSelectedPersons = extendPeriodBasedOnVisiblePersons ?
				new ScheduleDateTimePeriod(visiblePeriod, visiblePersons) :
				new ScheduleDateTimePeriod(visiblePeriod);

			var scheduleDictionary = new ScheduleDictionary(scenario, periodBasedOnSelectedPersons,
				new DifferenceEntityCollectionService<IPersistableScheduleData>(), _dataPermissionChecker);

			var uow = _currentUnitOfWork.Current();
			using (TurnoffPermissionScope.For(scheduleDictionary))
			{
				loadSchedules(scenario, scheduleDictionaryLoadOptions, visiblePersons, scheduleDictionary, periodBasedOnSelectedPersons, personsInOrganization).GetAwaiter().GetResult();

				if (scheduleDictionaryLoadOptions.LoadRestrictions)
				{
					var loadedPeriod = periodBasedOnSelectedPersons.LoadedPeriod();
					var longDateOnlyPeriod = new DateOnlyPeriod(new DateOnly(loadedPeriod.StartDateTime.AddDays(-1)), new DateOnly(loadedPeriod.EndDateTime.AddDays(1)));
					addPreferencesDays(scheduleDictionary,
						_repositoryFactory.CreatePreferenceDayRepository(uow).Find(longDateOnlyPeriod, visiblePersons));
					addStudentAvailabilityDays(scheduleDictionary,
						_repositoryFactory.CreateStudentAvailabilityDayRepository(uow).Find(longDateOnlyPeriod, visiblePersons));
					addOvertimeAvailability(scheduleDictionary,
						_repositoryFactory.CreateOvertimeAvailabilityRepository(uow).Find(longDateOnlyPeriod, visiblePersons));
					if (!scheduleDictionaryLoadOptions.LoadOnlyPreferensesAndHourlyAvailability)
					{
						addPersonAvailabilities(longDateOnlyPeriod, scheduleDictionary, visiblePersons);
						addPersonRotations(longDateOnlyPeriod, scheduleDictionary, visiblePersons);
					}
				}
			}

			// do we need this if personsProvider.DoLoadByPerson???
			removeSchedulesOfPersonsNotInOrganization(scheduleDictionary, personsInOrganization);

			scheduleDictionary.TakeSnapshot();
			return scheduleDictionary;
		}
		
		
		private void addOvertimeAvailability(IScheduleDictionary retDic, IEnumerable<IOvertimeAvailability> availabilityDays)
		{
			foreach (var availabilityDay in availabilityDays)
			{
				((ScheduleRange)retDic[availabilityDay.Person]).Add(availabilityDay);
			}
		}


		private async Task loadSchedules(IScenario scenario, ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, IEnumerable<IPerson> visiblePersons,
			IScheduleDictionary scheduleDictionary, ScheduleDateTimePeriod periodBasedOnSelectedPersons,
			IEnumerable<IPerson> personsInOrganization)
		{
			await doLoadSchedulesPerPersons(scenario, scheduleDictionaryLoadOptions, scheduleDictionary,
				periodBasedOnSelectedPersons.LongLoadedDateOnlyPeriod(), visiblePersons);
			await doLoadSchedulesPerPersons(scenario, scheduleDictionaryLoadOptions, scheduleDictionary,
				periodBasedOnSelectedPersons.LongVisibleDateOnlyPeriod(), personsInOrganization.Except(visiblePersons));
		}

		private async Task doLoadSchedulesPerPersons(IScenario scenario, ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, IScheduleDictionary scheduleDictionary, DateOnlyPeriod longDateOnlyPeriod, IEnumerable<IPerson> personsToLoad)
		{
			var uow = _currentUnitOfWork.Current();
			addPersonAbsences(scheduleDictionary, _repositoryFactory.CreatePersonAbsenceRepository(uow).Find(personsToLoad, longDateOnlyPeriod.ToDateTimePeriod(TimeZoneInfo.Utc), scenario), scheduleDictionaryLoadOptions.LoadDaysAfterLeft);
			addPersonAssignments(scheduleDictionary, await _findPersonAssignment.Find(personsToLoad, longDateOnlyPeriod, scenario), scheduleDictionaryLoadOptions.LoadDaysAfterLeft);
			addPersonMeetings(scheduleDictionary, _repositoryFactory.CreateMeetingRepository(uow).Find(personsToLoad, longDateOnlyPeriod, scenario, false), personsToLoad, scheduleDictionaryLoadOptions.LoadDaysAfterLeft);
			if (scheduleDictionaryLoadOptions.LoadNotes)
			{
				addNotes(scheduleDictionary, _repositoryFactory.CreateNoteRepository(uow).Find(longDateOnlyPeriod, personsToLoad, scenario));
				addPublicNotes(scheduleDictionary, _repositoryFactory.CreatePublicNoteRepository(uow).Find(longDateOnlyPeriod, personsToLoad, scenario));
			}

			if (scheduleDictionaryLoadOptions.LoadAgentDayScheduleTags)
			{
				addAgentDayScheduleTags(scheduleDictionary, _repositoryFactory.CreateAgentDayScheduleTagRepository(uow).Find(longDateOnlyPeriod, personsToLoad, scenario));
			}
		}

		private static void removeSchedulesOfPersonsNotInOrganization(IScheduleDictionary scheduleDictionary, IEnumerable<IPerson> personsInOrganization)
		{
			foreach (var person in personsInOrganization)
			{
				if (!scheduleDictionary[person].Person.Equals(person))
					throw new InvalidDataException("ScheduleDictionary could not be initiated");
			}

			// Clean ScheduleDictionary from all persons not present in LoadedAgents
			var personIds = personsInOrganization.ToDictionary(p => p.Id);
			var personsToRemove = new List<IPerson>();
			foreach (var person in scheduleDictionary.Keys)
			{
				if (!personIds.ContainsKey(person.Id))
					personsToRemove.Add(person);
			}
			foreach (var person in personsToRemove)
			{
				scheduleDictionary.Remove(person);
			}
		}

		private static void addPreferencesDays(IScheduleDictionary retDic, IEnumerable<IPreferenceDay> preferenceDays)
		{
			var preferenceByPerson = preferenceDays.ToLookup(p => p.Person);
			foreach (var preferenceDay in preferenceByPerson)
			{
				((ScheduleRange)retDic[preferenceDay.Key]).AddRange(preferenceDay);
			}
		}

		private static void addStudentAvailabilityDays(IScheduleDictionary retDic, IEnumerable<IStudentAvailabilityDay> availabilityDays)
		{
			var availabilityByPerson = availabilityDays.ToLookup(a => a.Person);
			foreach (var availabilityDay in availabilityByPerson)
			{
				((ScheduleRange)retDic[availabilityDay.Key]).AddRange(availabilityDay);
			}
		}

		private void addPersonRotations(DateOnlyPeriod period, IScheduleDictionary retDic, IEnumerable<IPerson> persons)
		{
			var people = persons.ToArray();
			var rotations = _repositoryFactory.CreatePersonRotationRepository(_currentUnitOfWork.Current())
				.LoadPersonRotationsWithHierarchyData(people, period.StartDate)
				.ToLookup(x => x.Person);

			foreach (var person in people)
			{
				foreach (var dateTime in period.DayCollection())
				{
					var rotationDayRestrictions = person.GetPersonRotationDayRestrictions(rotations[person], dateTime);
					foreach (var restriction in rotationDayRestrictions)
					{
						var personRotation = new ScheduleDataRestriction(person, restriction, dateTime);
						((ScheduleRange)retDic[personRotation.Person]).Add(personRotation);
					}
				}
			}
		}

		private void addPersonAvailabilities(DateOnlyPeriod period, IScheduleDictionary retDic, IEnumerable<IPerson> persons)
		{
			var people = persons.ToArray();
			var availabilities =
				_repositoryFactory.CreatePersonAvailabilityRepository(_currentUnitOfWork.Current())
					.LoadPersonAvailabilityWithHierarchyData(people, period.StartDate)
					.ToLookup(x => x.Person);

			foreach (var person in people)
			{
				var availForPerson = availabilities[person];
				IOrderedEnumerable<IPersonAvailability> sortedPersonRestrictions = availForPerson.OrderByDescending(n2 => n2.StartDate);
				if (!sortedPersonRestrictions.Any())
					continue;

				foreach (var dateTime in period.DayCollection())
				{
					var restriction = getPersonAvailabilityDayRestriction(sortedPersonRestrictions, dateTime);
					if (restriction != null)
					{
						var personRestriction = new ScheduleDataRestriction(person, restriction, dateTime);
						((ScheduleRange)retDic[personRestriction.Person]).Add(personRestriction);
					}
				}
			}
		}

		private IAvailabilityRestriction getPersonAvailabilityDayRestriction(IOrderedEnumerable<IPersonAvailability> sortedPersonRestrictions, DateOnly currentDate)
		{
			foreach (var availability in sortedPersonRestrictions)
			{
				if (availability.StartDate <= currentDate)
				{
					return availability.GetAvailabilityDay(currentDate).Restriction;
				}
			}
			return null;
		}

		private static void addPersonMeetings(IScheduleDictionary retDic, IEnumerable<IMeeting> meetings, IEnumerable<IPerson> addForThesePersons, bool loadDaysAfterLeft = false)
		{
			var persons = addForThesePersons.ToArray();
			var period = retDic.Period.LoadedPeriod();
			var personMeetings = meetings.SelectMany(m => m.GetPersonMeetings(period, persons)).ToLookup(p => p.Person);

			foreach (var personMeeting in personMeetings)
			{
				((ScheduleRange) retDic[personMeeting.Key]).AddRange(personMeeting.Where(pm =>
					loadDaysAfterLeft || !checkIfPersonLeft(pm.Person, pm.Period)));
			}
		}

		private static void addNotes(IScheduleDictionary retDic, IEnumerable<INote> notes)
		{
			var notesByPerson = notes.ToLookup(n => n.Person);
			foreach (var note in notesByPerson)
			{
				((ScheduleRange)retDic[note.Key]).AddRange(note);
			}
		}

		private static void addPublicNotes(IScheduleDictionary retDic, IEnumerable<IPublicNote> notes)
		{
			var notesByPerson = notes.ToLookup(n => n.Person);
			foreach (var note in notesByPerson)
			{
				((ScheduleRange)retDic[note.Key]).AddRange(note);
			}
		}

		private static void addAgentDayScheduleTags(IScheduleDictionary retDic, IEnumerable<IAgentDayScheduleTag> tags)
		{
			var tagsByPerson = tags.ToLookup(t => t.Person);
			foreach (var tag in tagsByPerson)
			{
				((ScheduleRange)retDic[tag.Key]).AddRange(tag);
			}
		}

		private static void addPersonAssignments(IScheduleDictionary retDic, IEnumerable<IPersonAssignment> personAssignments, bool loadDaysAfterLeft = false)
		{
			var assignmentsByPerson = personAssignments.ToLookup(k => k.Person);
			foreach (var person in assignmentsByPerson)
			{
				((ScheduleRange)retDic[person.Key]).AddRange(
					person.Where(pa => loadDaysAfterLeft || !checkIfPersonLeft(person.Key, pa.Date)));
			}
		}

		private static void addPersonAbsences(IScheduleDictionary retDic, IEnumerable<IPersonAbsence> personAbsences, bool loadDaysAfterLeft = false)
		{
			var absencesByPerson = personAbsences.ToLookup(p => p.Person);
			foreach (var personAbsence in absencesByPerson)
			{
				((ScheduleRange)retDic[personAbsence.Key]).AddRange(
					personAbsence.Where(pa => loadDaysAfterLeft || !checkIfPersonLeft(personAbsence.Key, pa.Period)));
			}
		}

		private static bool checkIfPersonLeft(IPerson person, DateTimePeriod period)
		{
			bool retValue = false;
			if (person.TerminalDate.HasValue)
			{
				var timezone = person.PermissionInformation.DefaultTimeZone();
				var convertedTerminalDate = TimeZoneHelper.ConvertToUtc(person.TerminalDate.Value.Date, timezone);
				retValue = convertedTerminalDate.AddDays(1) < period.StartDateTime;
			}

			return retValue;
		}

		private static bool checkIfPersonLeft(IPerson person, DateOnly date)
		{
			bool retValue = false;
			if (person.TerminalDate.HasValue)
			{
				retValue = person.TerminalDate.Value < date;
			}

			return retValue;
		}
	}
}