using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.DomainTest.Scheduling.Tracking;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class MeetingPayloadTest
    {
        private MeetingPayload _target;
        private IPerson _person;
        private string _subject;
        private string _location;
        private string _description;
        private IActivity _activity;
        private IScenario _scenario;
        private IMeeting _meeting;
        private ITracker _tracker;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson();
            _subject = "subject";
            _location = "location";
            _description = "description";
            _tracker = new TestTrackerOne();
            _activity = ActivityFactory.CreateActivity("activity");
            _activity.Tracker = _tracker;
            _activity.InContractTime = true;
            _activity.InWorkTime = true;
            _activity.InPaidTime = true;
            _activity.SetId(Guid.NewGuid());
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _meeting = new Meeting(_person, new List<IMeetingPerson> {new MeetingPerson(_person, false)}, _subject,
                                   _location, _description, _activity, _scenario);
            _meeting.SetId(Guid.NewGuid());
            _target = new MeetingPayload(_meeting);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
            Assert.AreSame(_meeting, _target.Meeting);
            Assert.IsTrue(_target.InContractTime);
            Assert.AreEqual(_activity.Tracker, _target.Tracker);
            Assert.AreEqual(_activity.InWorkTime, _target.InWorkTime);
            Assert.AreEqual(_activity.InPaidTime, _target.InPaidTime);
            Assert.AreEqual(_meeting.Id, _target.Id);
        }


        [Test, ExpectedException(typeof(NotImplementedException))]
        public void VerifyCannotSetTracker()
        {
            _target.Tracker = null;
        }

        [Test, ExpectedException(typeof(NotImplementedException))]
        public void VerifyCannotSetInWorkTime()
        {
            _target.InWorkTime = false;
        }

        [Test, ExpectedException(typeof(NotImplementedException))]
        public void VerifyCannotSetInPaidTime()
        {
            _target.InPaidTime = false;
        }

        [Test, ExpectedException(typeof(NotImplementedException))]
        public void VerifyCannotSetInContractTime()
        {
            _target.InContractTime = false;
        }

    }
}
