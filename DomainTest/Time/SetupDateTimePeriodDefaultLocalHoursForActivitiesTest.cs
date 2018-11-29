using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Time
{
    [TestFixture]
	public class SetupDateTimePeriodDefaultLocalHoursForActivitiesTest
    {
        private ISetupDateTimePeriod _target;
        private MockRepository _mockRepository;
        private IScheduleDay _scheduleDay;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _scheduleDay = _mockRepository.StrictMock<IScheduleDay>();
        }

        [Test]
        public void VerifyDefaultPeriod()
        {
            var startTime = new DateTime(2010, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            var endTime = new DateTime(2010, 01, 01, 23, 59, 59, DateTimeKind.Utc);

            var expectedStartTime = new DateTime(2010, 01, 01, 8, 0, 0, DateTimeKind.Utc);
            var expectedEndTime = new DateTime(2010, 01, 01, 17, 0, 0, DateTimeKind.Utc);

            using (_mockRepository.Record())
            {
                Expect.Call(_scheduleDay.Period).Return(new DateTimePeriod(startTime, endTime)).Repeat.Twice();
                Expect.Call(_scheduleDay.TimeZone).Return(TimeZoneInfoFactory.StockholmTimeZoneInfo());
            }
            using (_mockRepository.Playback())
            {
                _target = new SetupDateTimePeriodDefaultLocalHoursForActivities(_scheduleDay, new SpecificTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo()));
                var expect = new DateTimePeriod(expectedStartTime, expectedEndTime);
                Assert.AreEqual(expect, _target.Period);
            }
        }
    }
 }