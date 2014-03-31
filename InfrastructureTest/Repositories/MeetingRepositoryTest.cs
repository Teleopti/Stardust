using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Domain.Common;
using System.Collections.Generic;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests MeetingRepository
    ///</summary>
    [TestFixture]
    [Category("LongRunning")]
    public class MeetingRepositoryTest : RepositoryTest<IMeeting>
    {
        private IPerson _organizer;
        private IPerson _attendee1;
        private IPerson _attendee2;
        private IScenario _scenario;
        private IActivity _activity;

        protected override void ConcreteSetup()
        {
            _organizer = PersonFactory.CreatePerson("organizerPerson");
            _attendee1 = PersonFactory.CreatePerson("attendeePerson1");
            _attendee2 = PersonFactory.CreatePerson("attendeePerson2");
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _activity = new Activity("activity");

            PersistAndRemoveFromUnitOfWork(_organizer);
            PersistAndRemoveFromUnitOfWork(_attendee1);
            PersistAndRemoveFromUnitOfWork(_scenario);
            PersistAndRemoveFromUnitOfWork(_activity);
            PersistAndRemoveFromUnitOfWork(_attendee1);
            PersistAndRemoveFromUnitOfWork(_attendee2);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IMeeting CreateAggregateWithCorrectBusinessUnit()
        {
            IMeetingPerson meetingPerson1 = new MeetingPerson(_attendee1, true);
            IMeetingPerson meetingPerson2 = new MeetingPerson(_attendee2, true);

            Meeting meeting = new Meeting(_organizer, new List<IMeetingPerson> { meetingPerson1, meetingPerson2 }, "subject", "location",
                                "description", _activity, _scenario);
            IRecurrentWeeklyMeeting recurrentWeeklyMeeting = new RecurrentWeeklyMeeting();
            recurrentWeeklyMeeting[DayOfWeek.Tuesday] = true;
            recurrentWeeklyMeeting[DayOfWeek.Thursday] = true;
            meeting.SetRecurrentOption(recurrentWeeklyMeeting);
            meeting.StartDate = new DateOnly(2008,1,1);
            meeting.EndDate = meeting.StartDate;
            meeting.StartTime = TimeSpan.FromHours(10);
            meeting.EndTime = TimeSpan.FromHours(11);
            meeting.TimeZone = (TimeZoneInfo.Utc);

            return meeting;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IMeeting loadedAggregateFromDatabase)
        {
            IMeeting org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.GetDescription(new NoFormatting()), loadedAggregateFromDatabase.GetDescription(new NoFormatting()));
            Assert.AreEqual(org.Scenario, loadedAggregateFromDatabase.Scenario);
            Assert.AreEqual(org.MeetingPersons.Count(), loadedAggregateFromDatabase.MeetingPersons.Count());
            Assert.AreEqual(org.Activity, loadedAggregateFromDatabase.Activity);
            Assert.AreEqual(org.StartDate,loadedAggregateFromDatabase.StartDate);
            Assert.AreEqual(org.EndDate, loadedAggregateFromDatabase.EndDate);
            Assert.AreEqual(org.StartTime, loadedAggregateFromDatabase.StartTime);
            Assert.AreEqual(org.EndTime, loadedAggregateFromDatabase.EndTime);
            Assert.AreEqual(org.TimeZone.Id,loadedAggregateFromDatabase.TimeZone.Id);
            Assert.AreEqual(((IRecurrentWeeklyMeeting)org.MeetingRecurrenceOption).WeekDays.Count(), ((IRecurrentWeeklyMeeting)loadedAggregateFromDatabase.MeetingRecurrenceOption).WeekDays.Count());
        }

        [Test]
        public void VerifyFindByPeriodScenario()
        {
            ICollection<IMeeting> meetingList;
            IMeeting meeting = CreateAggregateWithCorrectBusinessUnit();
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();

            PersistAndRemoveFromUnitOfWork(scenario);
            PersistAndRemoveFromUnitOfWork(meeting);

            MeetingRepository meetingRepository = new MeetingRepository(UnitOfWork);

            meetingList = meetingRepository.Find(new DateTimePeriod(2008, 1, 1, 2009, 1, 2), _scenario);
            Assert.AreEqual(1, meetingList.Count);
            Assert.IsTrue(meetingList.Contains(meeting));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(meetingList.First().Organizer));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(meetingList.First().Organizer.Name));

            meetingList = meetingRepository.Find(new DateTimePeriod(2007, 1, 1, 2007, 1, 2), _scenario);
            Assert.IsTrue(meetingList.Count == 0);

            meetingList = meetingRepository.Find(new DateTimePeriod(2008, 1, 1, 2009, 1, 2), scenario);
            Assert.IsTrue(meetingList.Count == 0);
        }

        [Test]
        public void VerifyFindByPersonsPeriodScenario()
        {
            ICollection<IMeeting> meetingList;
            IMeeting meeting = CreateAggregateWithCorrectBusinessUnit();
            IPerson personNotInMeeting = PersonFactory.CreatePerson("c");
            IList<IPerson> persons = new List<IPerson>();

            persons.Add(_attendee1);
            persons.Add(_attendee2);

            PersistAndRemoveFromUnitOfWork(personNotInMeeting);
            PersistAndRemoveFromUnitOfWork(meeting);

            MeetingRepository meetingRepository = new MeetingRepository(UnitOfWork);
            meetingList = meetingRepository.Find(persons, new DateOnlyPeriod(2008, 1, 1, 2009, 1, 2), _scenario);
            Assert.AreEqual(1, meetingList.Count);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(meetingList.First().Organizer));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(meetingList.First().Organizer.Name));

            persons.Clear();
            persons.Add(personNotInMeeting);
            meetingList = meetingRepository.Find(persons, new DateOnlyPeriod(2008, 1, 1, 2009, 1, 2), _scenario);
            Assert.IsTrue(meetingList.Count == 0);
        }

        [Test]
        public void VerifyChangeRecurringType()
        {
            IMeeting meeting = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(meeting);
            var meetingPersons = new List<IMeetingPerson>(meeting.MeetingPersons);

            MeetingRepository meetingRepository = new MeetingRepository(UnitOfWork);
            var meetingList = meetingRepository.Find(meetingPersons.Select(p => p.Person), new DateOnlyPeriod(2008, 1, 1, 2009, 1, 2), _scenario);
            Assert.AreEqual(1, meetingList.Count);

            meeting = meetingList.First();
            meeting.SetRecurrentOption(new RecurrentMonthlyByWeekMeeting
                                           {
                                               DayOfWeek = DayOfWeek.Friday,
                                               WeekOfMonth = WeekNumber.Last,
                                               IncrementCount = 3
                                           });

            PersistAndRemoveFromUnitOfWork(meeting);
            meetingList = meetingRepository.Find(meetingPersons.Select(p => p.Person), new DateOnlyPeriod(2008, 1, 1, 2009, 1, 2), _scenario);
            Assert.AreEqual(1, meetingList.Count);
            Assert.IsInstanceOf(typeof(IRecurrentMonthlyByWeekMeeting),meetingList.First().MeetingRecurrenceOption);
        }

        [Test]
        public void VerifyFindByPersonsPeriodScenarioWithOrganizer()
        {
            ICollection<IMeeting> meetingList;
            IMeeting meeting = CreateAggregateWithCorrectBusinessUnit();
            IPerson personNotInMeeting = PersonFactory.CreatePerson("d");
            IList<IPerson> persons = new List<IPerson>();

            persons.Add(_organizer);

            PersistAndRemoveFromUnitOfWork(personNotInMeeting);
            PersistAndRemoveFromUnitOfWork(meeting);

            MeetingRepository meetingRepository = new MeetingRepository(UnitOfWork);
            meetingList = meetingRepository.Find(persons, new DateOnlyPeriod(2008, 1, 1, 2009, 1, 2), _scenario);
            Assert.AreEqual(1, meetingList.Count);

            persons.Clear();
            persons.Add(personNotInMeeting);
            meetingList = meetingRepository.Find(persons, new DateOnlyPeriod(2008, 1, 1, 2009, 1, 2), _scenario);
            Assert.IsTrue(meetingList.Count == 0);
        }

        [Test]
        public void VerifyLoadAggregate()
        {
            IMeeting meeting = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(meeting);

            meeting = new MeetingRepository(UnitOfWork).LoadAggregate(meeting.Id.GetValueOrDefault());
            Assert.IsNotNull(meeting);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(meeting.Organizer));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(meeting.Organizer.Name));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(meeting.Scenario));
        }

        [Test]
        public void VerifyLoadAggregateWhenNoHit()
        {
            var meetingNotFound = new MeetingRepository(UnitOfWork).LoadAggregate(Guid.NewGuid());
            Assert.IsNull(meetingNotFound);
        }

        [Test]
        public void ShouldCreateRepositoryWithUnitOfWorkFactory()
        {
            IMeetingRepository meetingRepository = new MeetingRepository(UnitOfWorkFactory.Current);
            Assert.IsNotNull(meetingRepository);
        }

		[Test]
		public void ShouldHandleNoChangesInChangeTracker()
		{
			IMeeting meeting = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(meeting);

			Meeting meetingLoaded = (Meeting)new MeetingRepository(UnitOfWork).LoadAggregate(meeting.Id.GetValueOrDefault());
			var changeTracker = new MeetingChangeTracker();
			changeTracker.TakeSnapshot((IMeeting)meetingLoaded.BeforeChanges());
			var changes = changeTracker.CustomChanges(meetingLoaded, DomainUpdateType.Update);
			changes.Count().Should().Be.EqualTo(0);
		}

        [Test]
        public void ShouldReturnMeetingsWithOriginalId()
        {
            IMeeting meeting = CreateAggregateWithCorrectBusinessUnit();
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            PersistAndRemoveFromUnitOfWork(meeting);
            PersistAndRemoveFromUnitOfWork(scenario);
            meeting = new MeetingRepository(UnitOfWork).LoadAggregate(meeting.Id.GetValueOrDefault());

            var clonedMeeting = meeting.NoneEntityClone();
            clonedMeeting.OriginalMeetingId = meeting.Id.GetValueOrDefault();
            clonedMeeting.SetScenario(scenario);
            PersistAndRemoveFromUnitOfWork(clonedMeeting);

            var linkedMeetings = new MeetingRepository(UnitOfWork).FindMeetingsWithTheseOriginals(new List<IMeeting>{meeting},scenario);
            Assert.That(linkedMeetings.Count().Equals(1));
            Assert.That(linkedMeetings[0].OriginalMeetingId.Equals(meeting.Id.GetValueOrDefault()));
        }

        protected override Repository<IMeeting> TestRepository(IUnitOfWork unitOfWork)
        {
            return new MeetingRepository(unitOfWork);
        }
    }
}
