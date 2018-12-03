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
	public class SetupDateTimePeriodDefaultLocalHoursForAbsenceTest
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
            DateTime startTime = new DateTime(2010, 01, 02, 0, 0, 0, DateTimeKind.Utc);
            DateTime endTime = new DateTime(2010, 01, 02, 23, 59, 59, DateTimeKind.Utc);

            DateTime expectedStartTime = new DateTime(2010, 01, 02, 0, 0, 0, DateTimeKind.Utc);
            DateTime expectedEndTime = new DateTime(2010, 01, 02, 23, 59, 0, DateTimeKind.Utc);

            using (_mockRepository.Record())
            {
                Expect.Call(_scheduleDay.Period).Return(new DateTimePeriod(startTime, endTime)).Repeat.Twice();
                Expect.Call(_scheduleDay.TimeZone).Return(TimeZoneInfoFactory.StockholmTimeZoneInfo());
            }
            using (_mockRepository.Playback())
            {
                _target = new SetupDateTimePeriodDefaultLocalHoursForAbsence(_scheduleDay, new SpecificTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo()));
                DateTimePeriod expect = new DateTimePeriod(expectedStartTime, expectedEndTime);
                Assert.AreEqual(expect, _target.Period);
            }

        }
    }
}