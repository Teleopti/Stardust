using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.ScheduleTagging
{
    [TestFixture]
    public class ScheduleDayTagExtractorTest
    {
        private ScheduleDayTagExtractor _target;
        private MockRepository _mocks;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IList<IScheduleDay> _scheduleDays;
        private IScheduleTag _scheduleTag1;
        private IScheduleTag _scheduleTag2;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
            _scheduleTag1 = new Domain.Scheduling.ScheduleTagging.ScheduleTag();
            _scheduleTag2 = new Domain.Scheduling.ScheduleTagging.ScheduleTag();
            _scheduleDays = new List<IScheduleDay> { _scheduleDay1, _scheduleDay2 };
            _target = new ScheduleDayTagExtractor(_scheduleDays);
        }

        [Test]
        public void ShouldExtractAllTaggedDays()
        {
            using(_mocks.Record())
            {
                Expect.Call(_scheduleDay1.ScheduleTag()).Return(_scheduleTag1);
                Expect.Call(_scheduleDay2.ScheduleTag()).Return(NullScheduleTag.Instance);
            }

            using(_mocks.Playback())
            {
                var extractedDays = _target.All();
                Assert.AreEqual(1, extractedDays.Count);
                Assert.AreEqual(_scheduleDay1, extractedDays[0]);
            }
        }

        [Test]
        public void ShouldExtractDaysWithTag()
        {
            using(_mocks.Record())
            {
                Expect.Call(_scheduleDay1.ScheduleTag()).Return(_scheduleTag1);
                Expect.Call(_scheduleDay2.ScheduleTag()).Return(_scheduleTag2);
            }

            using(_mocks.Playback())
            {
                var extractedDays = _target.Tag(_scheduleTag1);
                Assert.AreEqual(1, extractedDays.Count);
                Assert.AreEqual(_scheduleDay1, extractedDays[0]);
            }
        }
    }
}
