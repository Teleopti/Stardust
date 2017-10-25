﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
	public class ScheduleStorage : IScheduleStorage, IFindSchedulesForPersons
	{
	    private readonly IRepositoryFactory _repositoryFactory;
	    private readonly ICurrentUnitOfWork _currentUnitOfWork;
	    private readonly IPersistableScheduleDataPermissionChecker _dataPermissionChecker;
	    private readonly IScheduleStorageRepositoryWrapper _scheduleStorageRepositoryWrapper;

	    public ScheduleStorage(ICurrentUnitOfWork currentUnitOfWork, IRepositoryFactory repositoryFactory, IPersistableScheduleDataPermissionChecker dataPermissionChecker, IScheduleStorageRepositoryWrapper scheduleStorageRepositoryWrapper)
	    {
		    _currentUnitOfWork = currentUnitOfWork;
		    _repositoryFactory = repositoryFactory;
		    _dataPermissionChecker = dataPermissionChecker;
		    _scheduleStorageRepositoryWrapper = scheduleStorageRepositoryWrapper;
	    }

	    public void Add(IPersistableScheduleData scheduleData)
	    {
			_scheduleStorageRepositoryWrapper.Add(scheduleData);
		}

	    public void Remove(IPersistableScheduleData scheduleData)
	    {
			_scheduleStorageRepositoryWrapper.Remove(scheduleData);
	    }

	    public IPersistableScheduleData Get(Type concreteType, Guid id)
        {
			return _scheduleStorageRepositoryWrapper.Get(concreteType, id);
        }

			//todo: Fixa lazy-problem utanför! inte ladda andra rötter här inne!
        public IPersistableScheduleData LoadScheduleDataAggregate(Type scheduleDataType, Guid id)
        {
	        return _scheduleStorageRepositoryWrapper.LoadScheduleDataAggregate(scheduleDataType, id);
        }

		public IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(
			IPerson person,
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			DateTimePeriod dateTimePeriod,
			IScenario scenario
			)
		{
			var people = new[] { person };
			var period = dateTimePeriod.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());

			return findSchedulesOnlyInGivenPeriod(people, scheduleDictionaryLoadOptions, period, dateTimePeriod, scenario);
		}

		public IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(
			IPerson person,
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			DateOnlyPeriod period,
			IScenario scenario
			)
		{
			var people = new[] {person};
			var dateTimePeriod = period.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());

			return findSchedulesOnlyInGivenPeriod(people, scheduleDictionaryLoadOptions, period, dateTimePeriod, scenario);
		}

	    public IScheduleDictionary FindSchedulesForPersonsOnlyInGivenPeriod(
			IEnumerable<IPerson> persons, 
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, 
			DateOnlyPeriod period, 
			IScenario scenario)
	    {

		    DateTimePeriod scheduleDictionaryPeriod;

		    if (persons.Any())
		    {
			    var timeZones = persons.Select(p => p.PermissionInformation.DefaultTimeZone()).Distinct();
			    var dateTimePeriods = timeZones.Select(period.ToDateTimePeriod).ToArray();
			    scheduleDictionaryPeriod = new DateTimePeriod(
				    dateTimePeriods.Select(x => x.StartDateTime).Min(),
				    dateTimePeriods.Select(x => x.EndDateTime).Max()
				    );
		    }
		    else
		    {
				scheduleDictionaryPeriod = period.ToDateTimePeriod(TimeZoneInfo.Utc);
			}

			return findSchedulesOnlyInGivenPeriod(persons, scheduleDictionaryLoadOptions, period, scheduleDictionaryPeriod, scenario);
	    }

		private IScheduleDictionary findSchedulesOnlyInGivenPeriod(
			IEnumerable<IPerson> people, 
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, 
			DateOnlyPeriod period, 
			DateTimePeriod dictionaryPeriod, 
			IScenario scenario)
		{
			if (scheduleDictionaryLoadOptions == null)
				throw new ArgumentNullException(nameof(scheduleDictionaryLoadOptions));

			var longDateTimePeriod = new DateTimePeriod(dictionaryPeriod.StartDateTime.AddDays(-1),
														dictionaryPeriod.EndDateTime.AddDays(1));

			var retDic = new ReadOnlyScheduleDictionary(scenario, new ScheduleDateTimePeriod(dictionaryPeriod, people),
														new DifferenceEntityCollectionService<IPersistableScheduleData>(), _dataPermissionChecker);

			var uow = _currentUnitOfWork.Current();
			using (TurnoffPermissionScope.For(retDic))
			{
				addPersonAbsences(retDic,
								  _repositoryFactory.CreatePersonAbsenceRepository(uow)
													.Find(people, longDateTimePeriod, scenario));
				var personAssignmentRepository = _repositoryFactory.CreatePersonAssignmentRepository(uow);
				addPersonAssignments(retDic,
									 personAssignmentRepository.Find(people, period, scenario));

				addPersonMeetings(retDic,
								  _repositoryFactory.CreateMeetingRepository(uow).Find(people, period, scenario),
								  true, people);

				if (scheduleDictionaryLoadOptions.LoadNotes)
				{
					addNotes(retDic, _repositoryFactory.CreateNoteRepository(uow).Find(period, people, scenario));
					addPublicNotes(retDic,
								   _repositoryFactory.CreatePublicNoteRepository(uow)
													 .Find(period, people, scenario));
				}
				if (scheduleDictionaryLoadOptions.LoadAgentDayScheduleTags)
					addAgentDayScheduleTags(retDic,
										_repositoryFactory.CreateAgentDayScheduleTagRepository(uow)
														  .Find(period, people, scenario));

				if (scheduleDictionaryLoadOptions.LoadRestrictions)
				{
					addPreferencesDays(retDic,
									   _repositoryFactory.CreatePreferenceDayRepository(uow).Find(period, people));
					addStudentAvailabilityDays(retDic,
											   _repositoryFactory.CreateStudentAvailabilityDayRepository(uow)
																 .Find(period, people));
					addOvertimeAvailability(retDic,
											_repositoryFactory.CreateOvertimeAvailabilityRepository(uow)
															  .Find(period, people));
					if (!scheduleDictionaryLoadOptions.LoadOnlyPreferensesAndHourlyAvailability)
					{
						addPersonAvailabilities(period, retDic, people);
						addPersonRotations(period, retDic, people);
					}
				}
				
				retDic.ScheduleLoadedTime = personAssignmentRepository.GetScheduleLoadedTime();
			}
			retDic.TakeSnapshot();
			return retDic;
		}

	    private void addOvertimeAvailability(IScheduleDictionary retDic, IEnumerable<IOvertimeAvailability> availabilityDays)
        {
            foreach (var availabilityDay in availabilityDays)
            {
                ((ScheduleRange)retDic[availabilityDay.Person]).Add(availabilityDay);
            }
        }
		
        public IScheduleRange ScheduleRangeBasedOnAbsence(DateTimePeriod period, IScenario scenario, IPerson person, IAbsence absence = null)
        {
            IList<IPerson> people = new List<IPerson> {person};
	        var uow = _currentUnitOfWork.Current();

						var personAbsenceRepository = _repositoryFactory.CreatePersonAbsenceRepository(uow);
            ICollection<DateTimePeriod> searchPeriods = personAbsenceRepository.AffectedPeriods(person, scenario, period, absence);
            DateTimePeriod optimizedPeriod = searchPeriods.Count>0 ? 
                new DateTimePeriod(searchPeriods.Min(p => p.StartDateTime), searchPeriods.Max(p => p.EndDateTime).AddDays(1)):
				new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddDays(1));

	        var timeZone = person.PermissionInformation.DefaultTimeZone();
	        var longDateOnlyPeriod = optimizedPeriod.ToDateOnlyPeriod(timeZone);
			longDateOnlyPeriod = new DateOnlyPeriod(longDateOnlyPeriod.StartDate.AddDays(-1),longDateOnlyPeriod.EndDate.AddDays(1));
			var retDic = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(optimizedPeriod), new DifferenceEntityCollectionService<IPersistableScheduleData>(), _dataPermissionChecker);
			var personAssignmentRepository = _repositoryFactory.CreatePersonAssignmentRepository(uow);

            using (TurnoffPermissionScope.For(retDic))
            {
				addPersonAbsences(retDic, personAbsenceRepository.Find(people, optimizedPeriod, scenario));
                addPersonMeetings(retDic, _repositoryFactory.CreateMeetingRepository(uow).Find(people, longDateOnlyPeriod, scenario), true, people);
                var personAssignments = personAssignmentRepository.Find(people, longDateOnlyPeriod, scenario);
                foreach (var personAssignment in personAssignments)
                {
                    IScheduleRange range = retDic[personAssignment.Person];
                    IScheduleDay scheduleDay = range.ScheduledDay(personAssignment.Date);
                    if (scheduleDay.PersonAssignment(false) == null)
                        addPersonAssignments(retDic, new List<IPersonAssignment> {personAssignment});
                }
            }

			retDic.TakeSnapshot();
            return retDic[person];
        }

		public IScheduleDictionary FindSchedulesForPersons(IScenario scenario, IPersonProvider personsProvider, 
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, DateTimePeriod visiblePeriod, 
			IEnumerable<IPerson> visiblePersons, bool extendPeriodBasedOnVisiblePersons)
	    {
		    if (personsProvider == null) throw new ArgumentNullException(nameof(personsProvider));
		    if (scheduleDictionaryLoadOptions == null) throw new ArgumentNullException(nameof(scheduleDictionaryLoadOptions));

			var periodBasedOnSelectedPersons = extendPeriodBasedOnVisiblePersons ? 
				new ScheduleDateTimePeriod(visiblePeriod, visiblePersons) : 
				new ScheduleDateTimePeriod(visiblePeriod);	
			
		    var scheduleDictionary = new ScheduleDictionary(scenario, periodBasedOnSelectedPersons, 
			    new DifferenceEntityCollectionService<IPersistableScheduleData>(), _dataPermissionChecker);
		    var personsInOrganization = personsProvider.GetPersons();

		    var uow = _currentUnitOfWork.Current();
		    using (TurnoffPermissionScope.For(scheduleDictionary))
			{
			    if (personsProvider.DoLoadByPerson)
			    {
					doLoadSchedulesPerPersons(scenario, scheduleDictionaryLoadOptions, scheduleDictionary, periodBasedOnSelectedPersons.LongLoadedDateOnlyPeriod(), visiblePersons);
					doLoadSchedulesPerPersons(scenario, scheduleDictionaryLoadOptions, scheduleDictionary, periodBasedOnSelectedPersons.LongVisibleDateOnlyPeriod(), personsInOrganization.Except(visiblePersons));
			    }
			    else
			    {
					doLoadScheduleForAll(scenario, scheduleDictionary, periodBasedOnSelectedPersons.LongLoadedDateOnlyPeriod(), scheduleDictionaryLoadOptions);		    
				}

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

	    private void doLoadSchedulesPerPersons(IScenario scenario, ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, IScheduleDictionary scheduleDictionary, DateOnlyPeriod longDateOnlyPeriod, IEnumerable<IPerson> personsToLoad)
	    {
		    var uow = _currentUnitOfWork.Current();
			addPersonAbsences(scheduleDictionary, _repositoryFactory.CreatePersonAbsenceRepository(uow).Find(personsToLoad, longDateOnlyPeriod.ToDateTimePeriod(TimeZoneInfo.Utc), scenario), scheduleDictionaryLoadOptions.LoadDaysAfterLeft);
			addPersonAssignments(scheduleDictionary, _repositoryFactory.CreatePersonAssignmentRepository(uow).Find(personsToLoad, longDateOnlyPeriod, scenario), scheduleDictionaryLoadOptions.LoadDaysAfterLeft);
			addPersonMeetings(scheduleDictionary, _repositoryFactory.CreateMeetingRepository(uow).Find(personsToLoad, longDateOnlyPeriod, scenario), true, personsToLoad, scheduleDictionaryLoadOptions.LoadDaysAfterLeft);
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

	    private void doLoadScheduleForAll(IScenario scenario, IScheduleDictionary scheduleDictionary, DateOnlyPeriod dateOnlyPeriod, ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions)
	    {
		    var longPeriod = dateOnlyPeriod.ToDateTimePeriod(TimeZoneInfo.Utc);
		    var uow = _currentUnitOfWork.Current();
		    addPersonAbsences(scheduleDictionary,
			    _repositoryFactory.CreatePersonAbsenceRepository(uow).Find(longPeriod, scenario),
			    scheduleDictionaryLoadOptions.LoadDaysAfterLeft);
		    var personAssignmentRepository = _repositoryFactory.CreatePersonAssignmentRepository(uow);
		    addPersonAssignments(scheduleDictionary, personAssignmentRepository.Find(dateOnlyPeriod, scenario));
		    addPersonMeetings(scheduleDictionary, _repositoryFactory.CreateMeetingRepository(uow).Find(longPeriod, scenario),
			    false, new List<IPerson>());

		    if (scheduleDictionaryLoadOptions.LoadNotes)
		    {
			    addNotes(scheduleDictionary, _repositoryFactory.CreateNoteRepository(uow).Find(longPeriod, scenario));
			    addPublicNotes(scheduleDictionary, _repositoryFactory.CreatePublicNoteRepository(uow).Find(longPeriod, scenario));
		    }
		    if (scheduleDictionaryLoadOptions.LoadAgentDayScheduleTags)
		    {
			    addAgentDayScheduleTags(scheduleDictionary,
				    _repositoryFactory.CreateAgentDayScheduleTagRepository(uow).Find(longPeriod, scenario));
		    }
	    }

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ScheduleDictionary")]
        private static void removeSchedulesOfPersonsNotInOrganization(IScheduleDictionary scheduleDictionary, IList<IPerson> personsInOrganization)
        {
			foreach (var person in personsInOrganization)
			{
				if (!scheduleDictionary[person].Person.Equals(person))
					throw new InvalidDataException("ScheduleDictionary could not be initiated");
			}

			// Clean ScheduleDictionary from all persons not present in PersonsInOrganization
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

		private static void addPersonMeetings(IScheduleDictionary retDic, IEnumerable<IMeeting> meetings, bool onlyAddPersonsInList, IEnumerable<IPerson> addForThesePersons, bool loadDaysAfterLeft = false)
        {
				foreach (IMeeting meeting in meetings)
            {
                foreach (IMeetingPerson meetingPerson in meeting.MeetingPersons)
                {
                    if(onlyAddPersonsInList && !addForThesePersons.Contains(meetingPerson.Person))
                        continue;
                    IList<IPersonMeeting> personMeetings = meeting.GetPersonMeetings(meetingPerson.Person);

                    foreach (IPersonMeeting personMeeting in personMeetings)
                    {
	                    if (loadDaysAfterLeft || !checkIfPersonLeft(meetingPerson.Person, personMeeting.Period))
		                    ((ScheduleRange) retDic[meetingPerson.Person]).Add(personMeeting);
                    }
                }
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
	            ((ScheduleRange) retDic[person.Key]).AddRange(
		            person.Where(pa => loadDaysAfterLeft || !checkIfPersonLeft(person.Key, pa.Date)));
            }
        }

        private static void addPersonAbsences(IScheduleDictionary retDic, IEnumerable<IPersonAbsence> personAbsences, bool loadDaysAfterLeft = false)
        {
	        var absencesByPerson = personAbsences.ToLookup(p => p.Person);
	        foreach (var personAbsence in absencesByPerson)
	        {
		        ((ScheduleRange) retDic[personAbsence.Key]).AddRange(
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
