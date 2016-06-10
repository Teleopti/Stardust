﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class ScheduleStorage : IScheduleStorage
    {
	    private readonly IRepositoryFactory _repositoryFactory;
	    private readonly ICurrentUnitOfWork _currentUnitOfWork;
	    private readonly IPersistableScheduleDataPermissionChecker _dataPermissionChecker;

	    public ScheduleStorage(ICurrentUnitOfWork currentUnitOfWork, IRepositoryFactory repositoryFactory, IPersistableScheduleDataPermissionChecker dataPermissionChecker)
	    {
		    _currentUnitOfWork = currentUnitOfWork;
					_repositoryFactory = repositoryFactory;
		    _dataPermissionChecker = dataPermissionChecker;
	    }

		public IUnitOfWork UnitOfWork
		{
			get
			{
				return _currentUnitOfWork.Current();
			}
		}

	    public void Add(IPersistableScheduleData scheduleData)
	    {
		    UnitOfWork.Session().SaveOrUpdate(scheduleData);
	    }

	    public void Remove(IPersistableScheduleData scheduleData)
	    {
		    UnitOfWork.Session().Delete(scheduleData);
	    }

	    public IPersistableScheduleData Get(Type concreteType, Guid id)
        {
            return (IPersistableScheduleData)UnitOfWork.Session().Get(concreteType, id);
        }

			//todo: Fixa lazy-problem utanför! inte ladda andra rötter här inne!
        public IPersistableScheduleData LoadScheduleDataAggregate(Type scheduleDataType, Guid id)
        {
            if (typeof(IPersonAssignment).IsAssignableFrom(scheduleDataType))
            {
                return _repositoryFactory.CreatePersonAssignmentRepository(UnitOfWork).LoadAggregate(id);
            }
            if (typeof(IPersonAbsence).IsAssignableFrom(scheduleDataType))
            {
                return _repositoryFactory.CreatePersonAbsenceRepository(UnitOfWork).LoadAggregate(id);
            }
            if (typeof(IPreferenceDay).IsAssignableFrom(scheduleDataType))
            {
                return _repositoryFactory.CreatePreferenceDayRepository(UnitOfWork).LoadAggregate(id);
            }
            if (typeof(INote).IsAssignableFrom(scheduleDataType))
            {
                return _repositoryFactory.CreateNoteRepository(UnitOfWork).LoadAggregate(id);
            }
            if (typeof(IStudentAvailabilityDay).IsAssignableFrom(scheduleDataType))
            {
                return _repositoryFactory.CreateStudentAvailabilityDayRepository(UnitOfWork).LoadAggregate(id);
            }
            if (typeof(IPublicNote).IsAssignableFrom(scheduleDataType))
            {
                return _repositoryFactory.CreatePublicNoteRepository(UnitOfWork).LoadAggregate(id);
            }
            if(typeof(IAgentDayScheduleTag).IsAssignableFrom(scheduleDataType))
            {
                return _repositoryFactory.CreateAgentDayScheduleTagRepository(UnitOfWork).LoadAggregate(id);
            }
            if(typeof(IOvertimeAvailability).IsAssignableFrom(scheduleDataType))
            {
                return _repositoryFactory.CreateOvertimeAvailabilityRepository(UnitOfWork).LoadAggregate(id);
            }
            if (!typeof(IPersistableScheduleData).IsAssignableFrom(scheduleDataType))
                throw new ArgumentException("Only IPersistableScheduleData types are allowed");
            throw new NotImplementedException("Missing repository definition for type " + scheduleDataType);
        }

		public IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(
			IPerson person,
			IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
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
			IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
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
			IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, 
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
			IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, 
			DateOnlyPeriod period, 
			DateTimePeriod dictionaryPeriod, 
			IScenario scenario)
		{
			if (scheduleDictionaryLoadOptions == null)
				throw new ArgumentNullException("scheduleDictionaryLoadOptions");

			var longDateTimePeriod = new DateTimePeriod(dictionaryPeriod.StartDateTime.AddDays(-1),
														dictionaryPeriod.EndDateTime.AddDays(1));

			var retDic = new ReadOnlyScheduleDictionary(scenario, new ScheduleDateTimePeriod(dictionaryPeriod, people),
														new DifferenceEntityCollectionService<IPersistableScheduleData>(), _dataPermissionChecker);
			
			using (TurnoffPermissionScope.For(retDic))
			{
				addPersonAbsences(retDic,
								  _repositoryFactory.CreatePersonAbsenceRepository(UnitOfWork)
													.Find(people, longDateTimePeriod, scenario));
				var personAssignmentRepository = _repositoryFactory.CreatePersonAssignmentRepository(UnitOfWork);
				addPersonAssignments(retDic,
									 personAssignmentRepository.Find(people, period, scenario));

				addPersonMeetings(retDic,
								  _repositoryFactory.CreateMeetingRepository(UnitOfWork).Find(people, period, scenario),
								  true, people);

				if (scheduleDictionaryLoadOptions.LoadNotes)
				{
					addNotes(retDic, _repositoryFactory.CreateNoteRepository(UnitOfWork).Find(period, people, scenario));
					addPublicNotes(retDic,
								   _repositoryFactory.CreatePublicNoteRepository(UnitOfWork)
													 .Find(period, people, scenario));
				}
				if (scheduleDictionaryLoadOptions.LoadAgentDayScheduleTags)
					addAgentDayScheduleTags(retDic,
										_repositoryFactory.CreateAgentDayScheduleTagRepository(UnitOfWork)
														  .Find(period, people, scenario));

				if (scheduleDictionaryLoadOptions.LoadRestrictions)
				{
					addPreferencesDays(retDic,
									   _repositoryFactory.CreatePreferenceDayRepository(UnitOfWork).Find(period, people));
					addStudentAvailabilityDays(retDic,
											   _repositoryFactory.CreateStudentAvailabilityDayRepository(UnitOfWork)
																 .Find(period, people));
					addOvertimeAvailability(retDic,
											_repositoryFactory.CreateOvertimeAvailabilityRepository(UnitOfWork)
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
            var personAbsenceRepository = _repositoryFactory.CreatePersonAbsenceRepository(UnitOfWork);
            ICollection<DateTimePeriod> searchPeriods = personAbsenceRepository.AffectedPeriods(person, scenario, period, absence);
            DateTimePeriod optimizedPeriod = searchPeriods.Count>0 ? 
                new DateTimePeriod(searchPeriods.Min(p => p.StartDateTime), searchPeriods.Max(p => p.EndDateTime).AddDays(1)):
				new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddDays(1));

	        var timeZone = person.PermissionInformation.DefaultTimeZone();
	        var longDateOnlyPeriod = optimizedPeriod.ToDateOnlyPeriod(timeZone);
			longDateOnlyPeriod = new DateOnlyPeriod(longDateOnlyPeriod.StartDate.AddDays(-1),longDateOnlyPeriod.EndDate.AddDays(1));
			var retDic = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(optimizedPeriod), new DifferenceEntityCollectionService<IPersistableScheduleData>(), _dataPermissionChecker);
			var personAssignmentRepository = _repositoryFactory.CreatePersonAssignmentRepository(UnitOfWork);

            using (TurnoffPermissionScope.For(retDic))
            {
				addPersonAbsences(retDic, personAbsenceRepository.Find(people, optimizedPeriod, scenario));
                addPersonMeetings(retDic, _repositoryFactory.CreateMeetingRepository(UnitOfWork).Find(people, longDateOnlyPeriod, scenario), true, people);
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

	    public IScheduleDictionary FindSchedulesForPersons(
		    DateTimePeriod period,
			    IScenario scenario,
			    IPersonProvider personsProvider,
			    IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			    IEnumerable<IPerson> visiblePersons)
	    {
		    return FindSchedulesForPersons(
			    new ScheduleDateTimePeriod(period),
			    scenario,
			    personsProvider,
			    scheduleDictionaryLoadOptions,
			    visiblePersons);
	    }

	    public IScheduleDictionary FindSchedulesForPersons(
		    IScheduleDateTimePeriod period,
			    IScenario scenario,
			    IPersonProvider personsProvider,
			    IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			    IEnumerable<IPerson> visiblePersons)
	    {
		    if (period == null) throw new ArgumentNullException("period");
		    if (personsProvider == null) throw new ArgumentNullException("personsProvider");
		    if (scheduleDictionaryLoadOptions == null) throw new ArgumentNullException("scheduleDictionaryLoadOptions");

		    var scheduleDictionary = new ScheduleDictionary(scenario, period,
			    new DifferenceEntityCollectionService<IPersistableScheduleData>(), _dataPermissionChecker);
		    IList<IPerson> personsInOrganization = personsProvider.GetPersons();

		    // ugly to be safe to get all
		    var loadedPeriod = period.LoadedPeriod();
		    var longDateOnlyPeriod = new DateOnlyPeriod(new DateOnly(loadedPeriod.StartDateTime.AddDays(-1)),
			    new DateOnly(loadedPeriod.EndDateTime.AddDays(1)));
		    var longPeriod = new DateTimePeriod(loadedPeriod.StartDateTime.AddDays(-1), loadedPeriod.EndDateTime.AddDays(1));

		    using (TurnoffPermissionScope.For(scheduleDictionary))
		    {
			    if (personsProvider.DoLoadByPerson)
			    {
				    loadScheduleByPersons(scenario, scheduleDictionary, longPeriod, longDateOnlyPeriod, personsInOrganization,
					    scheduleDictionaryLoadOptions.LoadDaysAfterLeft);

				    if (scheduleDictionaryLoadOptions.LoadNotes)
				    {
					    addNotes(scheduleDictionary,
						    _repositoryFactory.CreateNoteRepository(UnitOfWork).Find(longDateOnlyPeriod, personsInOrganization, scenario));
					    addPublicNotes(scheduleDictionary,
						    _repositoryFactory.CreatePublicNoteRepository(UnitOfWork)
							    .Find(longDateOnlyPeriod, personsInOrganization, scenario));
				    }

					if (scheduleDictionaryLoadOptions.LoadAgentDayScheduleTags)
						addAgentDayScheduleTags(scheduleDictionary,
					    _repositoryFactory.CreateAgentDayScheduleTagRepository(UnitOfWork)
						    .Find(longDateOnlyPeriod, personsInOrganization, scenario));
			    }
			    else
			    {
				    loadScheduleForAll(scenario, scheduleDictionary, longPeriod, longDateOnlyPeriod,
					    scheduleDictionaryLoadOptions.LoadDaysAfterLeft);

				    if (scheduleDictionaryLoadOptions.LoadNotes)
				    {
					    addNotes(scheduleDictionary, _repositoryFactory.CreateNoteRepository(UnitOfWork).Find(longPeriod, scenario));
					    addPublicNotes(scheduleDictionary,
						    _repositoryFactory.CreatePublicNoteRepository(UnitOfWork).Find(longPeriod, scenario));
				    }
					 if(scheduleDictionaryLoadOptions.LoadAgentDayScheduleTags)
						addAgentDayScheduleTags(scheduleDictionary,
							_repositoryFactory.CreateAgentDayScheduleTagRepository(UnitOfWork).Find(longPeriod, scenario));
			    }

			    if (scheduleDictionaryLoadOptions.LoadRestrictions)
			    {
				    addPreferencesDays(scheduleDictionary,
					    _repositoryFactory.CreatePreferenceDayRepository(UnitOfWork).Find(longDateOnlyPeriod, visiblePersons));
				    addStudentAvailabilityDays(scheduleDictionary,
					    _repositoryFactory.CreateStudentAvailabilityDayRepository(UnitOfWork)
						    .Find(longDateOnlyPeriod, visiblePersons));
				    addOvertimeAvailability(scheduleDictionary,
					    _repositoryFactory.CreateOvertimeAvailabilityRepository(UnitOfWork).Find(longDateOnlyPeriod, visiblePersons));
				    if (!scheduleDictionaryLoadOptions.LoadOnlyPreferensesAndHourlyAvailability)
				    {
					    addPersonAvailabilities(longDateOnlyPeriod, scheduleDictionary, personsInOrganization);
					    addPersonRotations(longDateOnlyPeriod, scheduleDictionary, visiblePersons);
				    }
			    }
		    }

		    // do we need this if personsProvider.DoLoadByPerson???
		    removeSchedulesOfPersonsNotInOrganization(scheduleDictionary, personsInOrganization);

		    scheduleDictionary.TakeSnapshot();
		    return scheduleDictionary;
	    }

	    private void loadScheduleForAll(IScenario scenario, ScheduleDictionary scheduleDictionary, DateTimePeriod longPeriod, DateOnlyPeriod dateOnlyPeriod, bool loadDaysAfterLeft)
        {
			addPersonAbsences(scheduleDictionary, _repositoryFactory.CreatePersonAbsenceRepository(UnitOfWork).Find(longPeriod, scenario), loadDaysAfterLeft);
			addPersonAssignments(scheduleDictionary, _repositoryFactory.CreatePersonAssignmentRepository(UnitOfWork).Find(dateOnlyPeriod, scenario));
            addPersonMeetings(scheduleDictionary, _repositoryFactory.CreateMeetingRepository(UnitOfWork).Find(longPeriod, scenario),false,new List<IPerson>());
        }

        private void loadScheduleByPersons(IScenario scenario, ScheduleDictionary scheduleDictionary, DateTimePeriod longPeriod, DateOnlyPeriod longDateOnlyPeriod, IEnumerable<IPerson> persons, bool loadDaysAfterLeft)
        {
			addPersonAbsences(scheduleDictionary, _repositoryFactory.CreatePersonAbsenceRepository(UnitOfWork).Find(persons, longPeriod, scenario), loadDaysAfterLeft);
			addPersonAssignments(scheduleDictionary, _repositoryFactory.CreatePersonAssignmentRepository(UnitOfWork).Find(persons, longDateOnlyPeriod, scenario), loadDaysAfterLeft);
			addPersonMeetings(scheduleDictionary, _repositoryFactory.CreateMeetingRepository(UnitOfWork).Find(persons, longDateOnlyPeriod, scenario), true, persons, loadDaysAfterLeft);
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
            foreach (var preferenceDay in preferenceDays)
            {
                ((ScheduleRange)retDic[preferenceDay.Person]).Add(preferenceDay);
            }
        }

        private static void addStudentAvailabilityDays(IScheduleDictionary retDic, IEnumerable<IStudentAvailabilityDay> availabilityDays)
        {
            foreach (var availabilityDay in availabilityDays)
            {
                ((ScheduleRange)retDic[availabilityDay.Person]).Add(availabilityDay);
            }
        }

		private void addPersonRotations(DateOnlyPeriod period, IScheduleDictionary retDic, IEnumerable<IPerson> persons)
		{
			var people = persons.ToArray();
			var rotations = _repositoryFactory.CreatePersonRotationRepository(UnitOfWork)
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
		        _repositoryFactory.CreatePersonAvailabilityRepository(UnitOfWork)
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
            foreach (INote note in notes)
            {
                ((ScheduleRange)retDic[note.Person]).Add(note);
            }
        }

        private static void addPublicNotes(IScheduleDictionary retDic, IEnumerable<IPublicNote> notes)
        {
            foreach (IPublicNote note in notes)
            {
                ((ScheduleRange)retDic[note.Person]).Add(note);
            }
        }

        private static void addAgentDayScheduleTags(IScheduleDictionary retDic, IEnumerable<IAgentDayScheduleTag> tags)
        {
            foreach (var tag in tags)
            {
                ((ScheduleRange)retDic[tag.Person]).Add(tag);
            }
        }

		private static void addPersonAssignments(IScheduleDictionary retDic, IEnumerable<IPersonAssignment> personAssignments, bool loadDaysAfterLeft = false)
        {
            IDictionary<IPerson, IList<IPersonAssignment>> dic = new Dictionary<IPerson, IList<IPersonAssignment>>();
            foreach (IPersonAssignment personAssignment in personAssignments)
            {
                IPerson per = personAssignment.Person;
	            IList<IPersonAssignment> list;
	            if (!dic.TryGetValue(per, out list))
                    dic[per] = list = new List<IPersonAssignment>();
	            if (loadDaysAfterLeft || !checkIfPersonLeft(per, personAssignment.Period))
		            list.Add(personAssignment);
            }
            foreach (IPerson person in dic.Keys)
            {
				((ScheduleRange)retDic[person]).AddRange(dic[person]);
            }
        }

        private static void addPersonAbsences(IScheduleDictionary retDic, IEnumerable<IPersonAbsence> personAbsences, bool loadDaysAfterLeft = false)
        {
            foreach (IPersonAbsence personAbsence in personAbsences)
            {
				if (loadDaysAfterLeft || !checkIfPersonLeft(personAbsence.Person, personAbsence.Period))
		            ((ScheduleRange) retDic[personAbsence.Person]).Add(personAbsence);
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
    }    
}
