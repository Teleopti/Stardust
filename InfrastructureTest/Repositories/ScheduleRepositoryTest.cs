using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("LongRunning")]
    public class ScheduleRepositoryTest
    {
        private IPersonAbsenceRepository _absRep;
        private IPersonAssignmentRepository _assRep;
        private IMeetingRepository _meetingRepository;
        private INoteRepository _notesRepository;
        private IPublicNoteRepository _publicNoteRepository;
        private IAgentDayScheduleTagRepository _agentDayScheduleTagRepository;
        private IPersonRotationRepository _rotationRep;
        private IPersonAvailabilityRepository _availabilityRep;
        private IPersonRepository _personRep;
        private MockRepository _mocks;
        private IScenario _scenario;
        private IScheduleRepository _target;
        private IPreferenceDayRepository _prefDayRep;
        private IStudentAvailabilityDayRepository _availabilityDayRep;
        private IOvertimeAvailabilityRepository _overtimeAvailabilityRepository;
        private ICollection<IPersonAssignment> _assignments;
        private ICollection<IPersonAbsence> _absences;
        private ICollection<IMeeting> _meetings;
        private IUnitOfWork _unitOfWork;
        private IRepositoryFactory _repositoryFactory;
        private IList<IPreferenceDay> _prefDays;
        private IList<IPersonRotation> _rotations;
        private IList<IPersonAvailability> _availabilities;
        private IList<INote> _notes;
        private IList<IPublicNote> _publicNotes;
        private IList<IAgentDayScheduleTag> _agentDayScheduleTags;
        private IScheduleDateTimePeriod _schedPeriod;
        private IList<IStudentAvailabilityDay> _studentAvailabilityDays;
				private DateOnlyPeriod _searchPeriod;
				private DateTimePeriod _longPeriod;
        private DateOnlyPeriod _longDateOnlyPeriod;
        private IList<IOvertimeAvailability> _overtimeAvailbilityDays;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _repositoryFactory = _mocks.DynamicMock<IRepositoryFactory>();
            _unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            
            CreateRelatedRepositories();
            SetupExpectationsForRelatedRepositories();

            _target = new ScheduleRepositoryForTest(_unitOfWork,_repositoryFactory);

            CreateBasicStuff();
            CreateEmptyLists();
        }

        private void CreateBasicStuff()
        {
					_longPeriod = new DateTimePeriod(1999, 12, 31, 2001, 1, 3);
					_longDateOnlyPeriod = new DateOnlyPeriod(1999, 12, 31, 2001, 1, 3);
					_searchPeriod = new DateOnlyPeriod(2000,1,1,2001,1,2);
						_schedPeriod = new ScheduleDateTimePeriod(new DateTimePeriod(2000,1,1,2001,1,2));
            _scenario = ScenarioFactory.CreateScenarioAggregate();
        }

        private void CreateEmptyLists()
        {
            _assignments = new List<IPersonAssignment>();
            _absences = new List<IPersonAbsence>();
            _meetings = new List<IMeeting>();

            _prefDays = new List<IPreferenceDay>();
            _rotations = new List<IPersonRotation>();
            _availabilities = new List<IPersonAvailability>();

            _notes = new List<INote>();
            _publicNotes = new List<IPublicNote>();
            _agentDayScheduleTags = new List<IAgentDayScheduleTag>();

            _studentAvailabilityDays = new List<IStudentAvailabilityDay>();

            _overtimeAvailbilityDays = new List<IOvertimeAvailability>();
        }

        private void CreateRelatedRepositories()
        {
            _absRep = _mocks.StrictMock<IPersonAbsenceRepository>();
            _assRep = _mocks.StrictMock<IPersonAssignmentRepository>();
            _availabilityRep = _mocks.StrictMock<IPersonAvailabilityRepository>();
            _rotationRep = _mocks.StrictMock<IPersonRotationRepository>();
            _prefDayRep = _mocks.StrictMock<IPreferenceDayRepository>();
            _notesRepository = _mocks.StrictMock<INoteRepository>();
            _publicNoteRepository = _mocks.StrictMock<IPublicNoteRepository>();
            _agentDayScheduleTagRepository = _mocks.StrictMock<IAgentDayScheduleTagRepository>();
            _availabilityDayRep = _mocks.StrictMock<IStudentAvailabilityDayRepository>();
            _overtimeAvailabilityRepository = _mocks.StrictMock<IOvertimeAvailabilityRepository>();
            _personRep = _mocks.StrictMock<IPersonRepository>();
            _meetingRepository = _mocks.StrictMock<IMeetingRepository>();
        }

        private void SetupExpectationsForRelatedRepositories()
        {
            Expect.Call(_repositoryFactory.CreatePersonAbsenceRepository(_unitOfWork)).Return(_absRep).Repeat.Any();
            Expect.Call(_repositoryFactory.CreatePersonAssignmentRepository(_unitOfWork)).Return(_assRep).Repeat.Any();
            Expect.Call(_repositoryFactory.CreatePersonAvailabilityRepository(_unitOfWork)).Return(_availabilityRep).Repeat.Any();
            Expect.Call(_repositoryFactory.CreatePersonRotationRepository(_unitOfWork)).Return(_rotationRep).Repeat.Any();
            Expect.Call(_repositoryFactory.CreatePreferenceDayRepository(_unitOfWork)).Return(_prefDayRep).Repeat.Any();
            Expect.Call(_repositoryFactory.CreateNoteRepository(_unitOfWork)).Return(_notesRepository).Repeat.Any();
            Expect.Call(_repositoryFactory.CreatePublicNoteRepository(_unitOfWork)).Return(_publicNoteRepository).Repeat.Any();
            Expect.Call(_repositoryFactory.CreateAgentDayScheduleTagRepository(_unitOfWork)).Return(_agentDayScheduleTagRepository).Repeat.Any();
            Expect.Call(_repositoryFactory.CreateStudentAvailabilityDayRepository(_unitOfWork)).Return(_availabilityDayRep).Repeat.Any();
            Expect.Call(_repositoryFactory.CreateOvertimeAvailabilityRepository(_unitOfWork))
                  .Return(_overtimeAvailabilityRepository)
                  .Repeat.Any();
            Expect.Call(_repositoryFactory.CreatePersonRepository(_unitOfWork)).Return(_personRep).Repeat.Any();
            Expect.Call(_repositoryFactory.CreateMeetingRepository(_unitOfWork)).Return(_meetingRepository).Repeat.Any();
        }

        [Test]
        public void VerifyCanLoadAllBasedOnPeriodAndScenario()
        {
            IList<IPerson> persons = new List<IPerson>();
            IPersonProvider personsProvider = new PersonsInOrganizationProvider(_personRep, _longDateOnlyPeriod);
            IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, true);

            IScheduleDictionary retDic;

            //setup fake objects
            IPerson per1 = PersonFactory.CreatePerson("sdvfbvv");
            IPerson per2 = PersonFactory.CreatePerson("bdfbvdfbd");
            persons.Add(per1);
            persons.Add(per2);

            var visiblePeople = new List<IPerson>(persons);

            var period = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
            IPersonAssignment pAss1 = AddPersonAssignment(per1, period);
            IPersonAssignment pAss2 = AddPersonAssignment(per2, period);

            IPersonAbsence pAbs = AddAbsence(per1);

            AddMeeting(per1);
            AddPreference(per1);
            AddNote(per1);
            AddPublicNote(per1);
            AddAgentDayScheduleTag(per1);
            
            using (_mocks.Record())
            {
                ExpectScheduleLoadForAll(persons,visiblePeople);
            }
            using (_mocks.Playback())
            {
                retDic = _target.FindSchedulesForPersons(_schedPeriod, _scenario, personsProvider, scheduleDictionaryLoadOptions, visiblePeople);
            }
            Assert.AreEqual(2, retDic.Count);
            Assert.IsTrue(retDic[per1].Contains(pAss1));
            Assert.IsTrue(retDic[per2].Contains(pAss2));
            Assert.IsTrue(retDic[per1].Contains(pAbs));
            Assert.IsTrue(retDic[per1].Contains(_prefDays[0]));
            Assert.IsTrue(retDic[per1].Contains(_notes[0]));
            Assert.IsTrue(retDic[per1].Contains(_agentDayScheduleTags[0]));
           
            Assert.IsNotNull(((ScheduleRange)retDic[per1]).Snapshot);
            Assert.IsNotNull(((ScheduleRange)retDic[per2]).Snapshot);
            Assert.IsTrue(retDic[per1].ScheduledDay(new DateOnly(2000,6,1)).PersonMeetingCollection().Count == 1);
        }

        private void AddMeeting(IPerson person)
        {
            var activity = ActivityFactory.CreateActivity("for test");

            IMeetingPerson meetingPerson = new MeetingPerson(person, true);
            Meeting meeting = new Meeting(person, new List<IMeetingPerson> {meetingPerson}, "subject", "location", "description", activity, _scenario);
            meeting.StartDate = new DateOnly(2000, 6, 1);
            meeting.EndDate = new DateOnly(2000, 6, 1);
            meeting.StartTime = TimeSpan.FromHours(8);
            meeting.EndTime = TimeSpan.FromHours(9);
            _meetings.Add(meeting);
        }

        private IPersonAssignment AddPersonAssignment(IPerson person, DateTimePeriod period)
        {
            IPersonAssignment pAss1 =
                PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, person, period);
            _assignments.Add(pAss1);
            return pAss1;
        }

        private IPersonAbsence AddAbsence(IPerson per1)
        {
            IPersonAbsence pAbs =
                PersonAbsenceFactory.CreatePersonAbsence(per1, _scenario, new DateTimePeriod(2000, 1, 3, 2000, 1, 4));
            _absences.Add(pAbs);
            return pAbs;
        }

        [Test]
        public void VerifyCanLoadBasedOnPersonsAndPeriodAndScenario()
        {
            IList<IPerson> visiblePeople = new List<IPerson>();
            IList<IPerson> peopleInOrganization = new List<IPerson>();
            IScheduleDictionary retDic;

            //setup fake objects
            IPerson person1 = PersonFactory.CreatePerson("sdvfbvv");
            visiblePeople.Add(person1);
            IPerson person2 = PersonFactory.CreatePerson("yyyyyy");
            IPerson person3 = PersonFactory.CreatePerson("xxxxxx");
            peopleInOrganization.Add(person1);
            peopleInOrganization.Add(person2);
            peopleInOrganization.Add(person3);
            IPersonProvider personsProvider = new PersonsInOrganizationProvider(peopleInOrganization) {DoLoadByPerson = true};
            IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, true);

			person3.TerminatePerson(new DateOnly(2000, 1, 8), new PersonAccountUpdaterDummy());
			var pAss1 = AddPersonAssignment(person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
            var pAss2 = AddPersonAssignment(person3, new DateTimePeriod(2000, 1, 5, 2000, 1, 6));
			var pAss3 = AddPersonAssignment(person3, new DateTimePeriod(2000, 1, 9, 2000, 1, 10));
			var pAbs = AddAbsence(person1);

            AddMeeting(person1);
            AddPreference(person1);
            AddNote(person1);
            AddPublicNote(person1);
            AddAgentDayScheduleTag(person1);

            using (_mocks.Record())
            {
                ExpectScheduleLoadByPerson(visiblePeople, peopleInOrganization);
            }
            using (_mocks.Playback())
            {
                retDic = _target.FindSchedulesForPersons(_schedPeriod, _scenario, personsProvider, scheduleDictionaryLoadOptions, visiblePeople);
            }
            Assert.AreEqual(3, retDic.Count);
            Assert.IsTrue(retDic[person1].Contains(pAss1));
            Assert.IsTrue(retDic[person1].Contains(pAbs));
            Assert.IsTrue(retDic[person1].Contains(_prefDays[0]));
            Assert.IsTrue(retDic[person1].Contains(_notes[0]));
            Assert.IsTrue(retDic[person3].Contains(pAss2));
            Assert.IsFalse(retDic[person3].Contains(pAss3));
            Assert.IsTrue(retDic[person1].Contains(_agentDayScheduleTags[0]));

            Assert.IsNotNull(((ScheduleRange)retDic[person1]).Snapshot);
            Assert.IsTrue(retDic[person1].ScheduledDay(new DateOnly(2000, 6, 1)).PersonMeetingCollection().Count == 1);
        }

		[Test]
		public void VerifyCanLoadBasedOnPersonsAndPeriodAndScenarioEvenAgentLeft()
		{
			IList<IPerson> visiblePeople = new List<IPerson>();
			IList<IPerson> peopleInOrganization = new List<IPerson>();
			IScheduleDictionary retDic;

			//setup fake objects
			IPerson person1 = PersonFactory.CreatePerson("sdvfbvv");
			visiblePeople.Add(person1);
			IPerson person2 = PersonFactory.CreatePerson("yyyyyy");
			IPerson person3 = PersonFactory.CreatePerson("xxxxxx");
			peopleInOrganization.Add(person1);
			peopleInOrganization.Add(person2);
			peopleInOrganization.Add(person3);
			IPersonProvider personsProvider = new PersonsInOrganizationProvider(peopleInOrganization) { DoLoadByPerson = true };
			IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, true)
			{
				LoadDaysAfterLeft = true
			};

			person3.TerminatePerson(new DateOnly(2000,1,8), new PersonAccountUpdaterDummy());
			var pAss1 = AddPersonAssignment(person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
			var pAss2 = AddPersonAssignment(person3, new DateTimePeriod(2000, 1, 5, 2000, 1, 6));
			var pAss3 = AddPersonAssignment(person3, new DateTimePeriod(2000, 1, 9, 2000, 1, 10));
			var pAbs = AddAbsence(person1);

			AddMeeting(person1);
			AddPreference(person1);
			AddNote(person1);
			AddPublicNote(person1);
			AddAgentDayScheduleTag(person1);

			using (_mocks.Record())
			{
				ExpectScheduleLoadByPerson(visiblePeople, peopleInOrganization);
			}
			using (_mocks.Playback())
			{
				retDic = _target.FindSchedulesForPersons(_schedPeriod, _scenario, personsProvider, scheduleDictionaryLoadOptions, visiblePeople);
			}
			Assert.AreEqual(3, retDic.Count);
			Assert.IsTrue(retDic[person1].Contains(pAss1));
			Assert.IsTrue(retDic[person1].Contains(pAbs));
			Assert.IsTrue(retDic[person1].Contains(_prefDays[0]));
			Assert.IsTrue(retDic[person1].Contains(_notes[0]));
			Assert.IsTrue(retDic[person3].Contains(pAss2));
			Assert.IsTrue(retDic[person3].Contains(pAss3));
			Assert.IsTrue(retDic[person1].Contains(_agentDayScheduleTags[0]));

			Assert.IsNotNull(((ScheduleRange)retDic[person1]).Snapshot);
			Assert.IsTrue(retDic[person1].ScheduledDay(new DateOnly(2000, 6, 1)).PersonMeetingCollection().Count == 1);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void ExpectScheduleLoadByPerson(IEnumerable<IPerson> visiblePeople, IEnumerable<IPerson> peopleInOrganization)
        {
            Expect.Call(_absRep.Find(peopleInOrganization, _longPeriod, _scenario))
                .Return(_absences);
						Expect.Call(_assRep.Find(peopleInOrganization, _longDateOnlyPeriod, _scenario))
                .Return(_assignments);
            Expect.Call(_meetingRepository.Find(peopleInOrganization, _longDateOnlyPeriod, _scenario))
                .Return(_meetings);
            Expect.Call(_notesRepository.Find(_longDateOnlyPeriod, peopleInOrganization, _scenario))
                .Return(_notes);
            Expect.Call(_publicNoteRepository.Find(_longDateOnlyPeriod, peopleInOrganization, _scenario))
                .Return(_publicNotes);
            Expect.Call(_agentDayScheduleTagRepository.Find(_longDateOnlyPeriod, peopleInOrganization, _scenario)).Return(_agentDayScheduleTags);
            Expect.Call(_rotationRep.LoadPersonRotationsWithHierarchyData(null, DateTime.MinValue))
                .Constraints(Rhino.Mocks.Constraints.List.ContainsAll(visiblePeople), Rhino.Mocks.Constraints.Is.Equal(_schedPeriod.VisiblePeriod.StartDateTime))
                .Return(_rotations);
            Expect.Call(_availabilityRep.LoadPersonAvailabilityWithHierarchyData(null, DateTime.MinValue))
                .Constraints(Rhino.Mocks.Constraints.List.ContainsAll(visiblePeople), Rhino.Mocks.Constraints.Is.Equal(_schedPeriod.VisiblePeriod.StartDateTime))
                .Return(_availabilities);
						Expect.Call(_prefDayRep.Find(_longDateOnlyPeriod, visiblePeople)).Return(_prefDays);
            Expect.Call(_availabilityDayRep.Find(_longDateOnlyPeriod, visiblePeople)).Return(_studentAvailabilityDays);
						Expect.Call(_overtimeAvailabilityRepository.Find(_longDateOnlyPeriod, visiblePeople))
                  .Return(_overtimeAvailbilityDays);
        }

        private void AddPublicNote(IPerson person)
        {
            _publicNotes.Add(new PublicNote(person, new DateOnly(2000, 1, 2), _scenario, "Public ukhg"));
        }

        private void AddAgentDayScheduleTag(IPerson person)
        {
            _agentDayScheduleTags.Add(new AgentDayScheduleTag(person, new DateOnly(2000, 1, 2),_scenario, new ScheduleTag{Description =  "description"} ));
        }

        private void AddNote(IPerson person1)
        {
            _notes.Add(new Note(person1, new DateOnly(2000, 1, 2), _scenario, "ukhg"));
        }

        private void AddPreference(IPerson person)
        {
            _prefDays.Add(new PreferenceDay(person, new DateOnly(2000, 1, 10), new PreferenceRestriction()));
        }

        [Test]
        public void VerifyCanLoadReadOnlyScheduleBasedOnPersonsAndPeriodAndScenario()
        {
					 var searchPeriod = new DateOnlyPeriod(2000, 1, 1, 2001, 1, 2);
	        var longPeriod = new DateTimePeriod(1999, 12, 31, 2001, 1, 4);

            IList<IPerson> visiblePeople = new List<IPerson>();
            IScheduleDictionary retDic;

            //setup fake objects
            IPerson person1 = PersonFactory.CreatePerson("sdvfbvv");
            visiblePeople.Add(person1);
            
            var pAss1 = AddPersonAssignment(person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
            var pAbs = AddAbsence(person1);
            
            AddNote(person1);
            AddPublicNote(person1);
            AddAgentDayScheduleTag(person1);

            using (_mocks.Record())
            {
                ExpectScheduleLoadReadOnlyByPerson(visiblePeople, longPeriod);
            }
            using (_mocks.Playback())
            {
				retDic = _target.FindSchedulesForPersonsOnlyInGivenPeriod(visiblePeople, new ScheduleDictionaryLoadOptions(true, true), searchPeriod, _scenario);
            }
            Assert.AreEqual(1, retDic.Count);
            Assert.IsTrue(retDic[person1].Contains(pAss1));
            Assert.IsTrue(retDic[person1].Contains(pAbs));
            Assert.IsTrue(retDic[person1].Contains(_notes[0]));
            Assert.IsTrue(retDic[person1].Contains(_agentDayScheduleTags[0]));
            
            Assert.IsNotNull(((ScheduleRange)retDic[person1]).Snapshot);

            retDic.GetType().Name.Should().Contain("ReadOnly");
        }

        private void ExpectScheduleLoadReadOnlyByPerson(IList<IPerson> visiblePeople, DateTimePeriod longPeriod)
        {
            Expect.Call(_absRep.Find(visiblePeople, longPeriod, _scenario))
                .Return(_absences);
						Expect.Call(_assRep.Find(visiblePeople, _searchPeriod, _scenario))
                .Return(_assignments);
            Expect.Call(_meetingRepository.Find(visiblePeople, _searchPeriod, _scenario))
                .Return(_meetings);
						Expect.Call(_notesRepository.Find(_searchPeriod, visiblePeople, _scenario))
                .Return(_notes);
            Expect.Call(_publicNoteRepository.Find(_searchPeriod, visiblePeople, _scenario))
                .Return(_publicNotes);
						Expect.Call(_agentDayScheduleTagRepository.Find(_searchPeriod, visiblePeople, _scenario)).Return(_agentDayScheduleTags);
            Expect.Call(_rotationRep.LoadPersonRotationsWithHierarchyData(null, DateTime.MinValue))
                .Constraints(Rhino.Mocks.Constraints.List.ContainsAll(visiblePeople), Rhino.Mocks.Constraints.Is.Equal(longPeriod.StartDateTime))
                .Return(_rotations);
            Expect.Call(_availabilityRep.LoadPersonAvailabilityWithHierarchyData(null, DateTime.MinValue))
                .Constraints(Rhino.Mocks.Constraints.List.ContainsAll(visiblePeople), Rhino.Mocks.Constraints.Is.Equal(longPeriod.StartDateTime))
                .Return(_availabilities);
						Expect.Call(_prefDayRep.Find(_searchPeriod, visiblePeople)).Return(_prefDays);
						Expect.Call(_availabilityDayRep.Find(_searchPeriod, visiblePeople)).Return(_studentAvailabilityDays);
						Expect.Call(_overtimeAvailabilityRepository.Find(_searchPeriod, visiblePeople))
                  .Return( _overtimeAvailbilityDays);
        }

        [Test]
        public void VerifyAvailabilityRestrictionOnSchedulePart()
        {
            IList<IPerson> persons = new List<IPerson>();
            
            IPersonProvider personsProvider = new PersonsInOrganizationProvider(_personRep, _longDateOnlyPeriod);
            IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, true);

            IScheduleDictionary retDic;

            //setup fake objects
            IPerson per1 = PersonFactory.CreatePerson("wdfsdef");
            persons.Add(per1);

            AddAvailability(per1);
            AddPersonAssignment(per1, new DateTimePeriod(2000, 2, 1, 2000, 2, 2));
            
            var visiblePeople = new List<IPerson>(persons);

            using (_mocks.Record())
            {
                ExpectScheduleLoadForAll(persons,visiblePeople);
            }
            using (_mocks.Playback())
            {
                retDic = _target.FindSchedulesForPersons(_schedPeriod, _scenario, personsProvider, scheduleDictionaryLoadOptions, visiblePeople);
            }
            IAvailabilityRestriction personAvailability = per1.GetPersonAvailabilityDayRestriction(_availabilities,new DateOnly(2000,2,1));
            var coll = retDic[per1].ScheduledDay(new DateOnly(2000,2,1)).PersonRestrictionCollection();

            Assert.AreSame(personAvailability, ((IScheduleDataRestriction)coll[0]).Restriction);
        }

        private void AddAvailability(IPerson person)
        {
            IAvailabilityRotation rotationBase = new AvailabilityRotation("Availability", 7);
            IAvailabilityRestriction availabilityDayRestriction = new AvailabilityRestriction();
            availabilityDayRestriction.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(0, 7, 0, 0), null);
            rotationBase.AvailabilityDays[0].Restriction = availabilityDayRestriction;
            var availabilityStartDate = new DateOnly(2000, 2, 1);
            
            IPersonAvailability availability = new PersonAvailability(person, rotationBase, availabilityStartDate);
            _availabilities.Add(availability);

            //IOvertimeAvailability overtimeAvailability = new OvertimeAvailability(person, availabilityStartDate, TimeSpan.FromHours(8), TimeSpan.FromHours(10));
            //_availabilities.Add(overtimeAvailability );
        }

        [Test]
        public void VerifyRotationRestrictionOnSchedulePart()
        {   
            IList<IPerson> persons = new List<IPerson>();

            IPersonProvider personsProvider = new PersonsInOrganizationProvider(_personRep, _longDateOnlyPeriod);
            IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, true);

            IScheduleDictionary retDic;

            //setup fake objects
            IPerson per1 = PersonFactory.CreatePerson("sdfpojsdpofjk");
            persons.Add(per1);

            AddRotation(per1);
            AddPersonAssignment(per1, new DateTimePeriod(2000, 2, 1, 2000, 2, 2));
            
            var visiblePeople = new List<IPerson>(persons);
            using (_mocks.Record())
            {
                ExpectScheduleLoadForAll(persons, visiblePeople);
            }
            using (_mocks.Playback())
            {
                retDic = _target.FindSchedulesForPersons(_schedPeriod, _scenario, personsProvider, scheduleDictionaryLoadOptions, visiblePeople);
            }
            var coll = retDic[per1].ScheduledDay(new DateOnly(2000,2,1)).PersonRestrictionCollection();
            Assert.AreEqual(1, coll.Count);
        }

        private void ExpectScheduleLoadForAll(IList<IPerson> persons, IList<IPerson> visiblePeople)
        {
            Expect.Call(_absRep.Find(_longPeriod, _scenario))
                .Return(_absences);
						Expect.Call(_assRep.Find(_longDateOnlyPeriod, _scenario))
                .Return(_assignments);
            Expect.Call(_meetingRepository.Find(_longPeriod, _scenario))
                .Return(_meetings);
            Expect.Call(_rotationRep.LoadPersonRotationsWithHierarchyData(null, DateTime.MinValue))
                .Constraints(Rhino.Mocks.Constraints.List.ContainsAll(visiblePeople), Rhino.Mocks.Constraints.Is.Equal(_schedPeriod.VisiblePeriod.StartDateTime))
                .Return(_rotations);
            Expect.Call(_availabilityRep.LoadPersonAvailabilityWithHierarchyData(null, DateTime.MinValue))
                .Constraints(Rhino.Mocks.Constraints.List.ContainsAll(visiblePeople), Rhino.Mocks.Constraints.Is.Equal(_schedPeriod.VisiblePeriod.StartDateTime))
                .Return(_availabilities);
            Expect.Call(_notesRepository.Find(_longPeriod, _scenario))
                .Return(_notes);
            Expect.Call(_publicNoteRepository.Find(_longPeriod, _scenario))
                .Return(_publicNotes);

            Expect.Call(_agentDayScheduleTagRepository.Find(_longPeriod, _scenario)).Return(_agentDayScheduleTags);
           
            Expect.Call(_prefDayRep.Find(_longDateOnlyPeriod, visiblePeople)).Return(_prefDays);
            Expect.Call(_availabilityDayRep.Find(_longDateOnlyPeriod, visiblePeople)).Return(_studentAvailabilityDays);
            Expect.Call(_overtimeAvailabilityRepository.Find(_longDateOnlyPeriod, visiblePeople))
                 .Return(_overtimeAvailbilityDays);
            Expect.Call(_personRep.FindPeopleInOrganization(_longDateOnlyPeriod, true))
                .Return(persons).Repeat.Once();
        }

        private void AddRotation(IPerson per1)
        {
            IRotation rotationBase = new Rotation("Rotation", 7);
            var startTimeLimitation = new StartTimeLimitation(new TimeSpan(0, 17, 0, 0), null);
            rotationBase.RotationDays[0].RestrictionCollection[0].StartTimeLimitation = startTimeLimitation;
            var availabilityStartDate = new DateOnly(2000, 2, 1);

            IPersonRotation rotation = new PersonRotation(per1, rotationBase, availabilityStartDate, 0);
            _rotations.Add(rotation);
        }

        [Test]
        public void VerifyProperties()
        {
            _target = new ScheduleRepository(UnitOfWorkFactory.Current);
            Assert.IsNotNull(_target);
        }

        [Test]
        public void CanFindSchedulesAbsencePersonAndPeriod()
        {
            IScheduleRange range;
            IAbsence absenceToLookFor = AbsenceFactory.CreateAbsence("for test");
            IPerson person = PersonFactory.CreatePerson("645");
            IList<IPerson> people = new List<IPerson> { person };
           
            var period1 = new DateTimePeriod(2000,2,1,2000,2,10);
            var longPeriod1 = new DateOnlyPeriod(2000, 1, 31, 2000, 3, 11);

            var period2 = new DateTimePeriod(2000, 3, 1, 2000, 3, 10);
            var period3 = new DateTimePeriod(2000, 2, 1, 2000, 3, 11);
           
            ICollection<DateTimePeriod> absencePeriods = new List<DateTimePeriod> {period1,period2};

            IPersonAssignment personAssignment1 = AddPersonAssignment(person,new DateTimePeriod(2000, 2, 1, 2000, 2, 2));
            IPersonAssignment personAssignment2 = AddPersonAssignment(person,new DateTimePeriod(2000, 3, 2, 2000, 3, 3));

            using (_mocks.Record())
            {
                Expect.Call(_absRep.AffectedPeriods(person, _scenario, _longPeriod, absenceToLookFor)).Return(absencePeriods);

				Expect.Call(_absRep.Find(people, period3, _scenario)).Return(_absences);
                Expect.Call(_assRep.Find(people, longPeriod1, _scenario)).Return(_assignments);
                Expect.Call(_meetingRepository.Find(people, longPeriod1, _scenario)).Return(_meetings);
            }
            using (_mocks.Playback())
            {
                range = _target.ScheduleRangeBasedOnAbsence(_longPeriod, _scenario, person, absenceToLookFor);
            }
            Assert.AreEqual(person,range.Person,"Range is on person");
            Assert.IsTrue(range.Contains(personAssignment1));
            Assert.IsTrue(range.Contains(personAssignment2));
        }

	    [Test]
	    public void VerifyEndDateTimeIsNotLongerThanNecessary()
	    {
		    IScheduleRange range;
		    IAbsence absenceToLookFor = AbsenceFactory.CreateAbsence("for test");
		    IPerson person = PersonFactory.CreatePerson("66567");
		    IList<IPerson> people = new List<IPerson> {person};

		    var searchPeriod = new DateTimePeriod(2000, 1, 1, 2200, 1, 1);
		    var period1 = new DateTimePeriod(2000, 2, 1, 2000, 2, 10);
            var longPeriod1 = new DateOnlyPeriod(2000, 1, 31, 2001, 3, 11);
		    var period2 = new DateTimePeriod(2000, 3, 1, 2001, 3, 10);
		    var period3 = new DateTimePeriod(2000, 2, 1, 2001, 3, 11);
		    //Returnvalues:
		    ICollection<DateTimePeriod> absencePeriods = new List<DateTimePeriod> {period1, period2};

		    using (_mocks.Record())
		    {
			    Expect.Call(_absRep.AffectedPeriods(person, _scenario, searchPeriod, absenceToLookFor)).Return(absencePeriods);

				Expect.Call(_absRep.Find(people, period3, _scenario)).Return(_absences);
                Expect.Call(_assRep.Find(people, longPeriod1, _scenario)).Return(_assignments);
			    Expect.Call(_meetingRepository.Find(people, longPeriod1, _scenario)).Return(_meetings);
		    }
		    using (_mocks.Playback())
		    {
			    range = _target.ScheduleRangeBasedOnAbsence(searchPeriod, _scenario, person, absenceToLookFor);
		    }
		    Assert.IsTrue(period2.EndDateTime.AddMonths(2) >= range.Period.EndDateTime,
		                  "Make sure the period is not too long, because the projection service will be slow");
		    Assert.IsTrue(period2.EndDateTime < range.Period.EndDateTime);
	    }

	    [Test]
        public void VerifyReturnsEmptyRangeIfAbsenceCollectionIsEmpty()
        {
            IAbsence absenceToLookFor = AbsenceFactory.CreateAbsence("for test");
            IPerson person = PersonFactory.CreatePerson("gfsegfwegtwer");
			var period = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
            ICollection<DateTimePeriod> absencePeriods = new List<DateTimePeriod>();
			var searchPeriod = new DateTimePeriod(2000, 1, 1, 2001, 1, 1);
            var people = new List<IPerson> { person };
            using (_mocks.Record())
            {
                Expect.Call(_absRep.AffectedPeriods(person, _scenario, searchPeriod, absenceToLookFor)).Return(absencePeriods);
                Expect.Call(_absRep.Find(people, period, _scenario)).Return(_absences);
				Expect.Call(_meetingRepository.Find(people, new DateOnlyPeriod(1999, 12, 31, 2000, 1, 2), _scenario)).Return(_meetings);
                Expect.Call(_assRep.Find(people, new DateOnlyPeriod(1999, 12, 31, 2000, 1, 2), _scenario)).Return(_assignments);
            }
            using (_mocks.Playback())
            {
				IScheduleRange range = _target.ScheduleRangeBasedOnAbsence(searchPeriod, _scenario, person, absenceToLookFor);
                Assert.IsNotNull(range);
            }
        }

        [Test]
        public void VerifyLoadScheduleDataAggregatePersonAssignment()
        {
            var g = Guid.NewGuid();
            var ret = PersonAssignmentFactory.CreatePersonAssignment(PersonFactory.CreatePerson());
            using(_mocks.Record())
            {
                Expect.Call(_assRep.LoadAggregate(g))
                    .Return(ret);
            }
            using(_mocks.Playback())
            {
                Assert.AreSame(ret, _target.LoadScheduleDataAggregate(typeof (IPersonAssignment), g));
            }
        }

        [Test]
        public void VerifyLoadScheduleDataAggregatePersonAbsence()
        {
            var g = Guid.NewGuid();
            var ret = PersonAbsenceFactory.CreatePersonAbsence(PersonFactory.CreatePerson(),_scenario, new DateTimePeriod());
            using (_mocks.Record())
            {
                Expect.Call(_absRep.LoadAggregate(g))
                    .Return(ret);
            }
            using (_mocks.Playback())
            {
                Assert.AreSame(ret, _target.LoadScheduleDataAggregate(typeof(IPersonAbsence), g));
            }
        }

        [Test]
        public void VerifyLoadScheduleDataAggregatePreferenceDay()
        {
            var category = ShiftCategoryFactory.CreateShiftCategory("for test");
            var g = Guid.NewGuid();
            var ret = new PreferenceDay(PersonFactory.CreatePerson(), new DateOnly(2010, 3, 9),
                                        new PreferenceRestriction {ShiftCategory = category});
            using (_mocks.Record())
            {
                Expect.Call(_prefDayRep.LoadAggregate(g))
                    .Return(ret);
            }
            using (_mocks.Playback())
            {
                Assert.AreSame(ret, _target.LoadScheduleDataAggregate(typeof(IPreferenceDay), g));
            }
        }

        [Test]
        public void VerifyLoadScheduleDataAggregateNote()
        {
            var g = Guid.NewGuid();
            var ret = new Note(PersonFactory.CreatePerson(), new DateOnly(2010, 4, 27), _scenario, "note");

            using (_mocks.Record())
            {
                Expect.Call(_notesRepository.LoadAggregate(g)).Return(ret);
            }

            using (_mocks.Playback())
            {
                Assert.AreSame(ret, _target.LoadScheduleDataAggregate(typeof(INote), g));
            }
        }

        [Test]
        public void VerifyLoadScheduleDataAggregatePublicNote()
        {
            var g = Guid.NewGuid();
            var ret = new PublicNote(PersonFactory.CreatePerson(), new DateOnly(2010, 4, 27), _scenario, "note");

            using (_mocks.Record())
            {
                Expect.Call(_publicNoteRepository.LoadAggregate(g)).Return(ret);
            }

            using (_mocks.Playback())
            {
                Assert.AreSame(ret, _target.LoadScheduleDataAggregate(typeof(IPublicNote), g));
            }
        }

        [Test]
        public void VerifyLoadScheduleDataAggregateAgentDayScheduleTag()
        {
            var g = Guid.NewGuid();
            var ret = new AgentDayScheduleTag(PersonFactory.CreatePerson(), new DateOnly(2010, 4, 27), _scenario, new ScheduleTag{Description = "description"});

            using (_mocks.Record())
            {
                Expect.Call(_agentDayScheduleTagRepository.LoadAggregate(g)).Return(ret);
            }

            using (_mocks.Playback())
            {
                Assert.AreSame(ret, _target.LoadScheduleDataAggregate(typeof(IAgentDayScheduleTag), g));
            }
        }

        [Test]
        public void VerifyLoadScheduleDataAggregateStudentAvailability()
        {
            var g = Guid.NewGuid();
            var ret = new StudentAvailabilityDay(PersonFactory.CreatePerson(), new DateOnly(2010, 4, 27), new List<IStudentAvailabilityRestriction>());

            using (_mocks.Record())
            {
                Expect.Call(_availabilityDayRep.LoadAggregate(g)).Return(ret);
            }

            using (_mocks.Playback())
            {
                Assert.AreSame(ret, _target.LoadScheduleDataAggregate(typeof(IStudentAvailabilityDay), g));
            }
        }

        [Test]
        public void VerifyLoadScheduleDataAggregateThrowsIfNotIPersistableScheduleData()
        {
            Assert.Throws<ArgumentException>(() => _target.LoadScheduleDataAggregate(typeof (Person), Guid.NewGuid()));
            Assert.Throws<ArgumentException>(() => _target.LoadScheduleDataAggregate(typeof (object), Guid.NewGuid()));
        }

        [Test]
        public void VerifyLoadScheduleDataAggregateThrowsIfMissingRepositoryInImplementation()
        {
            Assert.Throws<NotImplementedException>(
                () => _target.LoadScheduleDataAggregate(typeof (IPersistableScheduleData), Guid.NewGuid()));
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionOnNullPeriod()
        {
            var personProvider = _mocks.StrictMock<IPersonProvider>();
            var scheduleDictionaryLoadOptions = _mocks.StrictMock<IScheduleDictionaryLoadOptions>();
            var persons = new List<IPerson>();
            _target.FindSchedulesForPersons(null, _scenario, personProvider, scheduleDictionaryLoadOptions, persons);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionOnNullPersonProvider()
        {
            var scheduleDictionaryLoadOptions = _mocks.StrictMock<IScheduleDictionaryLoadOptions>();
            var persons = new List<IPerson>();
            _target.FindSchedulesForPersons(_schedPeriod, _scenario, null, scheduleDictionaryLoadOptions, persons);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionOnNullScheduleDictionaryLoadOptions()
        {
            var personProvider = _mocks.StrictMock<IPersonProvider>();
            var persons = new List<IPerson>();
            _target.FindSchedulesForPersons(_schedPeriod, _scenario, personProvider, null, persons);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionOnNullPersonProviderInGivenPeriod()
        {
            var scheduleDictionaryLoadOptions = _mocks.StrictMock<IScheduleDictionaryLoadOptions>();
			_target.FindSchedulesForPersonsOnlyInGivenPeriod(null, scheduleDictionaryLoadOptions, _longDateOnlyPeriod, _scenario);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionOnNullScheduleDictionaryLoadOptionsInGivenPeriod()
        {
			_target.FindSchedulesForPersonsOnlyInGivenPeriod(new IPerson[]{}, null, _longDateOnlyPeriod, _scenario);
        }
    }

    internal class ScheduleRepositoryForTest : ScheduleRepository
    {
        public ScheduleRepositoryForTest(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory) : base(unitOfWork)
        {
            SetRepositoryFactory(repositoryFactory);
        }
    }
}
