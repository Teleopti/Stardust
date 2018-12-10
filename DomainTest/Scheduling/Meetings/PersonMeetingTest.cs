using System;
using System.Collections.Generic;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Meetings
{
    [TestFixture]
    public class PersonMeetingTest
    {
        private IPersonMeeting _target;
        private IList<IMeetingPerson> _persons;
        private IMeetingPerson _meetingPerson;
        private String _subject;
        private String _location;
        private String _description;
        private IActivity _activity;
        private DateTimePeriod _period;
        private IScenario _scenario;
        private IPerson _person;
        private IMeeting _meeting;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson();
            _meetingPerson = new MeetingPerson(_person, false);
            _persons = new List<IMeetingPerson>();
            _persons.Add(_meetingPerson);
            _subject = "subject";
            _location = "location";
            _description = "description";
            _activity = ActivityFactory.CreateActivity("activity");
            _period = new DateTimePeriod(2008, 1, 1, 2008, 1, 2);
            _scenario = ScenarioFactory.CreateScenarioAggregate();

            _meeting = new Meeting(_person, _persons, _subject, _location, _description, _activity, _scenario);
            _target = new PersonMeeting(_meeting, _meetingPerson, _period);
        }

		[Test]
		public void MeetingsShouldBeConsideredToBeIncludedInDayBeforeAsWell()
		{
			/* to include meeting in night shifts
			 * I was trying to write a test in extractedschedule instead, 
			 * but I couldn't figure out how to properly create a personmeeting 
			 * instance so I gave it up :(
			 */
			var dayBefore = new DateOnlyAsDateTimePeriod(new DateOnly(2007, 12, 31), TimeZoneInfo.Utc);
			_target.BelongsToPeriod(dayBefore)
				.Should().Be.True();
		}

        [Test]
        public void MeetingsShouldBeConsideredToBeIncludedInDayBeforeAsWell2()
        {
	        var dayBefore = new DateOnlyAsDateTimePeriod(new DateOnly(2007, 12, 31), TimeZoneInfo.Utc);
            _target.BelongsToPeriod(new DateOnlyPeriod(dayBefore.DateOnly, dayBefore.DateOnly))
                .Should().Be.True();
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual(_person, _target.Person);
            Assert.AreEqual(_meetingPerson.Optional, _target.Optional);
            Assert.AreEqual(_target.BelongsToMeeting, _meeting);
        }

        [Test]
        public void VerifyClone()
        {
            PersonMeeting clone = (PersonMeeting)_target.Clone();
            Assert.AreEqual(_target.Person, clone.Person);

            clone = (PersonMeeting)_target.NoneEntityClone();

            Assert.AreEqual(_target.Person, clone.Person);
        }

        [Test]
        public void VerifyBelongsToScenario()
        {
            Assert.IsTrue(_target.BelongsToScenario(_scenario));
            Assert.IsFalse(_target.BelongsToScenario(ScenarioFactory.CreateScenarioAggregate()));
            Assert.IsFalse(_target.BelongsToScenario(null));
        }

        [Test]
        public void VerifyToLayer()
        {
            var layer = _target.ToLayer();
            Assert.AreEqual(_activity,layer.Payload);
            Assert.AreEqual(_period,layer.Period);
        }

		[Test]
		public void ShouldGetHashCodeFromPersonAndBelongsToMeeting()
		{
			Assert.AreEqual(_person.GetHashCode() ^ _target.BelongsToMeeting.GetHashCode() ^ _target.Period.GetHashCode(), _target.GetHashCode());
		}

		[Test]
		public void ShouldBeEqualToSelf()
		{
			Assert.IsTrue(_target.Equals(_target));
		}
    }
}
