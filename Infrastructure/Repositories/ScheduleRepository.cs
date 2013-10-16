using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for schedules
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-02-12
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class ScheduleRepository : Repository<IPersistableScheduleData>, IScheduleRepository
    {
        private IRepositoryFactory _repositoryFactory = new RepositoryFactory();

        public ScheduleRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public ScheduleRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }

				public ScheduleRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }

        protected void SetRepositoryFactory(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public IPersistableScheduleData Get(Type concreteType, Guid id)
        {
            return (IPersistableScheduleData) Session.Get(concreteType, id);
        }

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

        /// <summary>
        /// Finds schedule for the specified people only within the given period.
        /// </summary>
        /// <param name="personsProvider"></param>
        /// <param name="scheduleDictionaryLoadOptions"></param>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns>A schedule dictionary that can be used to view schedule, but not to modify anything.</returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-03-31
        /// </remarks>
        public IScheduleDictionary FindSchedulesOnlyInGivenPeriod(IPersonProvider personsProvider, IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, DateOnlyPeriod period, IScenario scenario)
        {
            if(personsProvider == null) throw new ArgumentNullException("personsProvider");
            if (scheduleDictionaryLoadOptions == null) throw new ArgumentNullException("scheduleDictionaryLoadOptions");

						var dateTimePeriod = new DateTimePeriod(new DateTime(period.StartDate.Date.Ticks, DateTimeKind.Utc), new DateTime(period.EndDate.Date.AddDays(1).Ticks, DateTimeKind.Utc));
	        var longDateTimePeriod = new DateTimePeriod(dateTimePeriod.StartDateTime.AddDays(-1), dateTimePeriod.EndDateTime.AddDays(1));

            var people = personsProvider.GetPersons();
						var retDic = new ReadOnlyScheduleDictionary(scenario, new ScheduleDateTimePeriod(dateTimePeriod, people), new DifferenceEntityCollectionService<IPersistableScheduleData>());

            using (TurnoffPermissionScope.For(retDic))
            {
							addPersonAbsences(retDic, _repositoryFactory.CreatePersonAbsenceRepository(UnitOfWork).Find(people, longDateTimePeriod, scenario));
								addPersonAssignments(retDic, _repositoryFactory.CreatePersonAssignmentRepository(UnitOfWork).Find(people, period, scenario));


								addPersonMeetings(retDic, _repositoryFactory.CreateMeetingRepository(UnitOfWork).Find(people, period, scenario), true, people);

                if(scheduleDictionaryLoadOptions.LoadNotes)
                {
									addNotes(retDic, _repositoryFactory.CreateNoteRepository(UnitOfWork).Find(period, people, scenario));
									addPublicNotes(retDic, _repositoryFactory.CreatePublicNoteRepository(UnitOfWork).Find(period, people, scenario));
                }

								addAgentDayScheduleTags(retDic, _repositoryFactory.CreateAgentDayScheduleTagRepository(UnitOfWork).Find(period, people, scenario));

                if(scheduleDictionaryLoadOptions.LoadRestrictions)
                {
                    addPreferencesDays(retDic,
                                       _repositoryFactory.CreatePreferenceDayRepository(UnitOfWork).Find(period, people));
					addStudentAvailabilityDays(retDic,
											   _repositoryFactory.CreateStudentAvailabilityDayRepository(UnitOfWork).Find(period, people));
					addOvertimeAvailability(retDic, _repositoryFactory.CreateOvertimeAvailabilityRepository(UnitOfWork).Find(period, people));
					if (!scheduleDictionaryLoadOptions.LoadOnlyPreferensesAndHourlyAvailability)
					{
						addPersonAvailabilities(longDateTimePeriod, retDic, people);
						addPersonRotations(longDateTimePeriod, retDic, people);
					}
                }
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


        public IScheduleRange ScheduleRangeBasedOnAbsence(DateTimePeriod period, IScenario scenario, IPerson person, IAbsence absence)
        {
            IList<IPerson> people = new List<IPerson> {person};
            ICollection<DateTimePeriod> searchPeriods = _repositoryFactory.CreatePersonAbsenceRepository(UnitOfWork).AffectedPeriods(person, scenario, period, absence);
            DateTimePeriod optimizedPeriod = searchPeriods.Count>0 ? 
                new DateTimePeriod(searchPeriods.Min(p => p.StartDateTime), searchPeriods.Max(p => p.EndDateTime).AddDays(1)):
				new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddDays(1));

	        var timeZone = person.PermissionInformation.DefaultTimeZone();
	        var longDateOnlyPeriod = optimizedPeriod.ToDateOnlyPeriod(timeZone);
			longDateOnlyPeriod = new DateOnlyPeriod(longDateOnlyPeriod.StartDate.AddDays(-1),longDateOnlyPeriod.EndDate.AddDays(1));
            var retDic = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(optimizedPeriod),
                                                               new DifferenceEntityCollectionService
                                                                   <IPersistableScheduleData>());
			var personAssignmentRepository = _repositoryFactory.CreatePersonAssignmentRepository(UnitOfWork);
					
            using(TurnoffPermissionScope.For(retDic))
            {
				addPersonAbsences(retDic, _repositoryFactory.CreatePersonAbsenceRepository(UnitOfWork).Find(people, optimizedPeriod, scenario, absence));
				addPersonMeetings(retDic, _repositoryFactory.CreateMeetingRepository(UnitOfWork).Find(people, longDateOnlyPeriod, scenario), true, people);
				foreach (DateTimePeriod p in searchPeriods)
				{
					var longDateOnlyP = new DateOnlyPeriod(new DateOnly(p.StartDateTime.AddDays(-1)), new DateOnly(p.EndDateTime.AddDays(1)));
					addPersonAssignments(retDic, _repositoryFactory.CreatePersonAssignmentRepository(UnitOfWork).Find(people, longDateOnlyP, scenario));
				}
            }

            return retDic[person];
        }

        public IScheduleDictionary FindSchedulesForPersons(
            IScheduleDateTimePeriod period, 
            IScenario scenario, 
            IPersonProvider personsProvider,
            IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
						IEnumerable<IPerson> visiblePersons)
        {
            if(period == null) throw new ArgumentNullException("period");
            if (personsProvider == null) throw new ArgumentNullException("personsProvider");
            if (scheduleDictionaryLoadOptions == null) throw new ArgumentNullException("scheduleDictionaryLoadOptions");

            var scheduleDictionary = new ScheduleDictionary(scenario, period, new DifferenceEntityCollectionService<IPersistableScheduleData>());
            IList<IPerson> personsInOrganization = personsProvider.GetPersons();
            
            // ugly to be safe to get all
            var loadedPeriod = period.LoadedPeriod();
            var longDateOnlyPeriod = new DateOnlyPeriod(new DateOnly(loadedPeriod.StartDateTime.AddDays(-1)), new DateOnly(loadedPeriod.EndDateTime.AddDays(1)));
						var longPeriod = new DateTimePeriod(loadedPeriod.StartDateTime.AddDays(-1), loadedPeriod.EndDateTime.AddDays(1));

            using(TurnoffPermissionScope.For(scheduleDictionary))
            {
                if (personsProvider.DoLoadByPerson)
                {
                    loadScheduleByPersons(scenario, scheduleDictionary, longPeriod, longDateOnlyPeriod, personsInOrganization);
                    
                    if(scheduleDictionaryLoadOptions.LoadNotes)
                    {
                        addNotes(scheduleDictionary, _repositoryFactory.CreateNoteRepository(UnitOfWork).Find(longDateOnlyPeriod, personsInOrganization, scenario));
                        addPublicNotes(scheduleDictionary, _repositoryFactory.CreatePublicNoteRepository(UnitOfWork).Find(longDateOnlyPeriod, personsInOrganization, scenario));
                    }

                    addAgentDayScheduleTags(scheduleDictionary, _repositoryFactory.CreateAgentDayScheduleTagRepository(UnitOfWork).Find(longDateOnlyPeriod, personsInOrganization, scenario));
                }
                else
                {
									loadScheduleForAll(scenario, scheduleDictionary, longPeriod, longDateOnlyPeriod);

                    if(scheduleDictionaryLoadOptions.LoadNotes)
                    {
                        addNotes(scheduleDictionary, _repositoryFactory.CreateNoteRepository(UnitOfWork).Find(longPeriod, scenario));
                        addPublicNotes(scheduleDictionary, _repositoryFactory.CreatePublicNoteRepository(UnitOfWork).Find(longPeriod, scenario));
                    }

                    addAgentDayScheduleTags(scheduleDictionary, _repositoryFactory.CreateAgentDayScheduleTagRepository(UnitOfWork).Find(longPeriod, scenario));
                }

                if(scheduleDictionaryLoadOptions.LoadRestrictions)
                {
                    addPreferencesDays(scheduleDictionary, _repositoryFactory.CreatePreferenceDayRepository(UnitOfWork).Find(longDateOnlyPeriod, visiblePersons));
					addStudentAvailabilityDays(scheduleDictionary,
													_repositoryFactory.CreateStudentAvailabilityDayRepository(UnitOfWork)
																	.Find(longDateOnlyPeriod, visiblePersons));
                    addOvertimeAvailability(scheduleDictionary, _repositoryFactory.CreateOvertimeAvailabilityRepository(UnitOfWork).Find(longDateOnlyPeriod, visiblePersons));
	                if (!scheduleDictionaryLoadOptions.LoadOnlyPreferensesAndHourlyAvailability)
	                {
		                addPersonAvailabilities(period.VisiblePeriod, scheduleDictionary, personsInOrganization);
		                addPersonRotations(period.VisiblePeriod, scheduleDictionary, visiblePersons);
	                }
                }                              
            }

            // do we need this if personsProvider.DoLoadByPerson???
            removeSchedulesOfPersonsNotInOrganization(scheduleDictionary, personsInOrganization);

            scheduleDictionary.TakeSnapshot();
            return scheduleDictionary;
        }

        private void loadScheduleForAll(IScenario scenario, ScheduleDictionary scheduleDictionary, DateTimePeriod longPeriod, DateOnlyPeriod dateOnlyPeriod)
        {
            addPersonAbsences(scheduleDictionary, _repositoryFactory.CreatePersonAbsenceRepository(UnitOfWork).Find(longPeriod, scenario));
						addPersonAssignments(scheduleDictionary, _repositoryFactory.CreatePersonAssignmentRepository(UnitOfWork).Find(dateOnlyPeriod, scenario));
            addPersonMeetings(scheduleDictionary, _repositoryFactory.CreateMeetingRepository(UnitOfWork).Find(longPeriod, scenario),false,new List<IPerson>());
        }

        private void loadScheduleByPersons(IScenario scenario, ScheduleDictionary scheduleDictionary, DateTimePeriod longPeriod, DateOnlyPeriod longDateOnlyPeriod, IEnumerable<IPerson> persons)
        {
            addPersonAbsences(scheduleDictionary, _repositoryFactory.CreatePersonAbsenceRepository(UnitOfWork).Find(persons, longPeriod, scenario));
						addPersonAssignments(scheduleDictionary, _repositoryFactory.CreatePersonAssignmentRepository(UnitOfWork).Find(persons, longDateOnlyPeriod, scenario));
            addPersonMeetings(scheduleDictionary, _repositoryFactory.CreateMeetingRepository(UnitOfWork).Find(persons, longDateOnlyPeriod, scenario),true,persons);
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
            var personsToRemove = new List<IPerson>();
            foreach (IPerson person in scheduleDictionary.Keys)
            {
                if (!personsInOrganization.Contains(person))
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

		private void addPersonRotations(DateTimePeriod period, IScheduleDictionary retDic, IEnumerable<IPerson> persons)
		{
            var rotations = _repositoryFactory.CreatePersonRotationRepository(UnitOfWork).LoadPersonRotationsWithHierarchyData(persons.ToArray(), period.StartDateTime);

			foreach (var person in persons)
			{
				foreach (var dateTime in period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()).DayCollection())
				{
					var rotationDayRestrictions = person.GetPersonRotationDayRestrictions(rotations, dateTime);
					foreach (var restriction in rotationDayRestrictions)
					{
						var personRotation = new ScheduleDataRestriction(person, restriction, dateTime);
						((ScheduleRange)retDic[personRotation.Person]).Add(personRotation);
					}
				}
			}
		}

        private void addPersonAvailabilities(DateTimePeriod period, IScheduleDictionary retDic, IEnumerable<IPerson> persons)
        {
            var availabilities = _repositoryFactory.CreatePersonAvailabilityRepository(UnitOfWork).LoadPersonAvailabilityWithHierarchyData(persons.ToArray(), period.StartDateTime);

            foreach (var person in persons)
            {
                foreach (var dateTime in period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()).DayCollection())
                {
                    var restriction = person.GetPersonAvailabilityDayRestriction(availabilities, dateTime);

                    if (restriction != null)
                    {
                        var personRestriction = new ScheduleDataRestriction(person, restriction, dateTime);
                        ((ScheduleRange)retDic[personRestriction.Person]).Add(personRestriction);
                    }
                }
            }
        }

        private static void addPersonMeetings(IScheduleDictionary retDic, IEnumerable<IMeeting> meetings, bool onlyAddPersonsInList, IEnumerable<IPerson> addForThesePersons)
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
                        ((ScheduleRange)retDic[meetingPerson.Person]).Add(personMeeting);
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

        private static void addPersonAssignments(IScheduleDictionary retDic, IEnumerable<IPersonAssignment> personAssignments)
        {
            IDictionary<IPerson, IList<IPersonAssignment>> dic = new Dictionary<IPerson, IList<IPersonAssignment>>();
            foreach (IPersonAssignment personAssignment in personAssignments)
            {
                IPerson per = personAssignment.Person;
                if (!dic.ContainsKey(per))
                    dic[per] = new List<IPersonAssignment>();
                dic[per].Add(personAssignment);
            }
            foreach (IPerson person in dic.Keys)
            {
                ((ScheduleRange)retDic[person]).AddRange(dic[person]);
            }
        }

        private static void addPersonAbsences(IScheduleDictionary retDic, IEnumerable<IPersonAbsence> personAbsences)
        {
            foreach (IPersonAbsence personAbsence in personAbsences)
            {
                ((ScheduleRange)retDic[personAbsence.Person]).Add(personAbsence);
            }
        }
    }    
}
