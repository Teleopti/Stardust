using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Meetings
{
    [TestFixture]
    public class MeetingPeriodTest
    {
        MeetingPeriod _target;

        [SetUp]
        public void Setup()
        {
            _target = new MeetingPeriod();
        }

        [Test]
        public void VerifyProperties()
        {
            IList<IMeetingPerson> meetingPersons = new List<IMeetingPerson>();
            meetingPersons.Add(new MeetingPerson(new Person(), true));

            _target.Value = 3;
            _target.AddMeetingPerson(new MeetingPerson(new Person(), true));
            _target.AddMeetingPersons(meetingPersons);
            _target.Period = new DateTimePeriod(2000,1,1,2001,1,1);

            Assert.AreEqual(3, _target.Value);
            Assert.AreEqual(2, _target.MeetingPersons.Count);
            Assert.AreEqual(new DateTimePeriod(2000,1,1,2001,1,1), _target.Period);
        }
    }
}