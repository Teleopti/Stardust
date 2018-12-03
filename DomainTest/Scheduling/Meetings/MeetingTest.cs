using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using SharpTestsEx;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Meetings
{
    [TestFixture]
    public class MeetingTest
    {
        private IMeeting _target;
        private IList<IMeetingPerson> _persons;
        private IMeetingPerson _meetingPerson;
        private String _subject;
        private String _location;
        private String _description;
        private IActivity _activity;
        private IScenario _scenario;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson();
            _meetingPerson = new MeetingPerson(_person, false);
            _persons = new List<IMeetingPerson> {_meetingPerson};
        	_subject = "subject";
            _location = "location";
            _description = "description";
            _activity = ActivityFactory.CreateActivity("activity");
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _target = new Meeting(PersonFactory.CreatePerson(), _persons, _subject, _location, _description, _activity, _scenario)
                      	{StartDate = new DateOnly(2009, 10, 13)};
        	_target.EndDate = _target.StartDate;
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target.Organizer);
            Assert.IsTrue(_target.MeetingPersons.Count() == 1);
            Assert.AreEqual(_target, _meetingPerson.Parent);
			Assert.IsTrue(_target.GetSubject(new NoFormatting()) == _subject);
			Assert.IsTrue(_target.GetLocation(new NoFormatting()) == _location);
			Assert.IsTrue(_target.GetDescription(new NoFormatting()) == _description);
            Assert.AreSame(_activity, _target.Activity);
            Assert.AreSame(_scenario, _target.Scenario);
            Assert.AreEqual(_target.StartDate,_target.GetRecurringDates()[0]);
            Assert.AreEqual(TimeSpan.FromHours(1),_target.MeetingDuration());
        }

        [Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType(), true));
        }

        [Test]
        public void VerifyProperties()
        {
            _target.Organizer = PersonFactory.CreatePerson();
            _target.Subject = "su";
            _target.Location = "lo";
            _target.Description = "de";
            IActivity activity = ActivityFactory.CreateActivity("act");
            _target.Activity = activity;
            _target.StartTime = new TimeSpan(8, 0, 0);
			Assert.That(_target.EndTime, Is.EqualTo(TimeSpan.FromHours(9)));

            _target.EndTime = new TimeSpan(10, 0, 0);

            Assert.IsNotNull(_target.Organizer);
			Assert.IsTrue(_target.GetSubject(new NoFormatting()) == "su");
			Assert.IsTrue(_target.GetLocation(new NoFormatting()) == "lo");
			Assert.IsTrue(_target.GetDescription(new NoFormatting()) == "de");
            Assert.AreSame(activity, _target.Activity);

            Assert.AreEqual(_target.StartTime, new TimeSpan(8, 0, 0));
            Assert.AreEqual(_target.EndTime, new TimeSpan(10, 0, 0));

            _target.StartTime = TimeSpan.FromHours(9);
			Assert.That(_target.EndTime, Is.EqualTo(TimeSpan.FromHours(11)));
        }

        [Test]
        public void VerifyClearMeetingPersons()
        {
            _target.AddMeetingPerson(new MeetingPerson(PersonFactory.CreatePerson(), true));
            Assert.IsTrue(_target.MeetingPersons.Count() > 0);
            _target.ClearMeetingPersons();
            Assert.IsTrue(_target.MeetingPersons.Count() == 0);
        }

        [Test]
        public void RemovePerson()
        {
            _target.ClearMeetingPersons();
            _target.AddMeetingPerson(_meetingPerson);
            _target.AddMeetingPerson(new MeetingPerson(PersonFactory.CreatePerson(), true));
            _target.RemovePerson(_meetingPerson.Person);
            Assert.IsTrue(_target.MeetingPersons.Count() == 1);
        }

        [Test]
        public void AddMeetingPerson()
        {
            _target.ClearMeetingPersons();
            _target.AddMeetingPerson(new MeetingPerson(PersonFactory.CreatePerson(), true));
            _target.AddMeetingPerson(new MeetingPerson(PersonFactory.CreatePerson(), false));
            Assert.IsTrue(_target.MeetingPersons.Count() == 2);
            Assert.AreEqual(_target, _target.MeetingPersons.First().Parent);
        }

        [Test]
        public void VerifyMeetingPersonListIsCopied()
        {
            Assert.AreEqual(1, _target.MeetingPersons.Count());
            _persons.Clear();
            Assert.AreEqual(1, _target.MeetingPersons.Count());
        }

        [Test]
        public void VerifyClone()
        {
            _target.SetId(Guid.NewGuid());
            var clone = (IMeeting)_target.Clone();
            
            Assert.AreEqual(_target.MeetingPersons.Count(), clone.MeetingPersons.Count());
            Assert.AreEqual(_target.Activity, clone.Activity);
            Assert.AreEqual(_target.Scenario, clone.Scenario);
            Assert.IsTrue(clone.Id.HasValue);

            clone = _target.EntityClone();

            Assert.AreEqual(_target.MeetingPersons.Count(), clone.MeetingPersons.Count());
            Assert.AreEqual(_target.Activity, clone.Activity);
            Assert.AreEqual(_target.Scenario, clone.Scenario);
            Assert.IsTrue(clone.Id.HasValue);

            clone = _target.NoneEntityClone();

            Assert.AreEqual(_target.MeetingPersons.Count(), clone.MeetingPersons.Count());
            Assert.AreEqual(_target.Activity, clone.Activity);
            Assert.AreEqual(_target.Scenario, clone.Scenario);
            Assert.IsFalse(clone.Id.HasValue);
        }

        [Test]
        public void VerifyCanGetPersonMeeting()
        {
            var meetingPerson = new MeetingPerson(_person, true);

            _target.ClearMeetingPersons();
            _target.AddMeetingPerson(meetingPerson);

            IList<IPersonMeeting> personMeetings = _target.GetPersonMeetings(new DateTimePeriod(2009,10,12,2009,10,14), _person);
            Assert.AreEqual(1, personMeetings.Count);

            personMeetings = _target.GetPersonMeetings(new DateTimePeriod(2009, 10, 12, 2009, 10, 14), PersonFactory.CreatePerson());
            Assert.AreEqual(0, personMeetings.Count);

			personMeetings = _target.GetPersonMeetings(new DateTimePeriod(2009, 10, 10, 2009, 10, 12), _person);
			Assert.AreEqual(0, personMeetings.Count);
		}


        [Test]
        public void VerifyCanRemoveRecurrence()
        {
            _target.SetRecurrentOption(new RecurrentWeeklyMeeting{IncrementCount = 4});
            _target.EndDate = _target.StartDate.AddDays(4);
            _target.RemoveRecurrentOption();
            Assert.IsInstanceOf(typeof(IRecurrentDailyMeeting),_target.MeetingRecurrenceOption);
            Assert.AreEqual(1, _target.MeetingRecurrenceOption.IncrementCount);
            Assert.AreEqual(_target.StartDate,_target.EndDate);
        }

		[Test]
		public void ShouldReturnCustomChangesWithoutResetTheBeforeChange()
		{
			var changes = ((IProvideCustomChangeInfo) _target).CustomChanges(DomainUpdateType.Update);
			changes.Count().Should().Be.GreaterThanOrEqualTo(1);

			changes = ((IProvideCustomChangeInfo)_target).CustomChanges(DomainUpdateType.Update);
			changes.Count().Should().Be.GreaterThanOrEqualTo(1);
		}

		[Test]
		public void ShouldNotGoOverMidnight()
		{
			_target.StartDate = new DateOnly(2013,3,20);
			_target.StartTime = TimeSpan.FromHours(8);
			_target.EndTime = TimeSpan.FromHours(10);

			Assert.AreEqual(_target.StartTime, new TimeSpan(8, 0, 0));
			Assert.AreEqual(_target.EndTime, new TimeSpan(10, 0, 0));

			_target.StartTime = TimeSpan.FromHours(23);
			Assert.That(_target.EndTime, Is.EqualTo(new TimeSpan(23,59,0)));
		}
    }
}