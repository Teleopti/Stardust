using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.DomainTest.Helper;

namespace Teleopti.Ccc.DomainTest.Scheduling.Meetings
{
    [TestFixture]
    public class MeetingPeriodSorterTest
    {
        IList<MeetingPeriod> _meetingPeriods;

        [SetUp]
        public void Setup()
        {
            _meetingPeriods = new List<MeetingPeriod>();
        }

        [Test]
        public void VerifySort()
        {
            MeetingPeriod meetingPeriod1 = new MeetingPeriod();
            MeetingPeriod meetingPeriod2 = new MeetingPeriod();
            MeetingPeriod meetingPeriod3 = new MeetingPeriod();

            meetingPeriod1.Value = 300d;
            meetingPeriod2.Value = 200d;
            meetingPeriod3.Value = 100d;

            _meetingPeriods.Add(meetingPeriod2);
            _meetingPeriods.Add(meetingPeriod3);
            _meetingPeriods.Add(meetingPeriod1);

            ((List<MeetingPeriod>)_meetingPeriods).Sort(new MeetingPeriodSorter());

            Assert.AreEqual(meetingPeriod1, _meetingPeriods[0]);
            Assert.AreEqual(meetingPeriod2, _meetingPeriods[1]);
            Assert.AreEqual(meetingPeriod3, _meetingPeriods[2]);
        }
    }
}