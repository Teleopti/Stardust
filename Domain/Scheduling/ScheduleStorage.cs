using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
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

		public IScheduleDictionary FindSchedulesForPersons(
		    IScheduleDateTimePeriod period,
			    IScenario scenario,
			    IPersonProvider personsProvider,
			    IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			    IEnumerable<IPerson> visiblePersons)
	    {
		    if (period == null) throw new ArgumentNullException(nameof(period));
		    if (personsProvider == null) throw new ArgumentNullException(nameof(personsProvider));
		    if (scheduleDictionaryLoadOptions == null) throw new ArgumentNullException(nameof(scheduleDictionaryLoadOptions));

		    var scheduleDictionary = new ScheduleDictionary(scenario, period,
			    new DifferenceEntityCollectionService<IPersistableScheduleData>(), _dataPermissionChecker);
		    var personsInOrganization = personsProvider.GetPersons();

		    var uow = _currentUnitOfWork.Current();
		    using (TurnoffPermissionScope.For(scheduleDictionary))
		    {
			    if (personsProvider.DoLoadByPerson)
			    {
				    LoadSchedulesByPersons(scenario, scheduleDictionaryLoadOptions, scheduleDictionary, period, personsInOrganization, visiblePersons);
			    }
			    else
			    {
				    LoadScheduleForAll(scenario, scheduleDictionaryLoadOptions, scheduleDictionary, period, visiblePersons);
			    }

			    if (scheduleDictionaryLoadOptions.LoadRestrictions)
			    {
					var loadedPeriod = period.LoadedPeriod();
					var longDateOnlyPeriod = new DateOnlyPeriod(new DateOnly(loadedPeriod.StartDateTime.AddDays(-1)), new DateOnly(loadedPeriod.EndDateTime.AddDays(1)));
					addPreferencesDays(scheduleDictionary,
					    _repositoryFactory.CreatePreferenceDayRepository(uow).Find(longDateOnlyPeriod, visiblePersons));
				    addStudentAvailabilityDays(scheduleDictionary,
					    _repositoryFactory.CreateStudentAvailabilityDayRepository(uow)
						    .Find(longDateOnlyPeriod, visiblePersons));
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

	    protected virtual void LoadSchedulesByPersons(IScenario scenario, IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, IScheduleDictionary scheduleDictionary, IScheduleDateTimePeriod period, IEnumerable<IPerson> personsInOrganization, IEnumerable<IPerson> selectedPersons)
	    {
		    DoLoadSchedulesPerPersons(scenario, scheduleDictionaryLoadOptions, scheduleDictionary, period.LongLoadedDateOnlyPeriod(), personsInOrganization);
	    }

	    protected void DoLoadSchedulesPerPersons(IScenario scenario, IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, IScheduleDictionary scheduleDictionary, DateOnlyPeriod longDateOnlyPeriod, IEnumerable<IPerson> personsToLoad)
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

	    protected virtual void LoadScheduleForAll(IScenario scenario, IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, IScheduleDictionary scheduleDictionary, IScheduleDateTimePeriod period, IEnumerable<IPerson> selectedPersons)
	    {
		    DoLoadScheduleForAll(scenario, scheduleDictionary, period.LongLoadedDateOnlyPeriod(), scheduleDictionaryLoadOptions);
	    }

	    protected void DoLoadScheduleForAll(IScenario scenario, IScheduleDictionary scheduleDictionary, DateOnlyPeriod dateOnlyPeriod, IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions)
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
