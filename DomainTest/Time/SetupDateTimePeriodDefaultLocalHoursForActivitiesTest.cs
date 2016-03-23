using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

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
                _target = new SetupDateTimePeriodDefaultLocalHoursForActivities(_scheduleDay, new CurrentTeleoptiPrincipal(new ThreadPrincipalContext()));
                var expect = new DateTimePeriod(expectedStartTime, expectedEndTime);
                Assert.AreEqual(expect, _target.Period);
            }

        }

        [Test]
        public void VerifyDefaultPeriodWithKnownShiftEndTime()
        {
            var startTime = new DateTime(2010, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            var endTime = new DateTime(2010, 01, 01, 23, 59, 59, DateTimeKind.Utc);

            var expectedStartTime = new DateTime(2010, 01, 01, 14, 30, 0, DateTimeKind.Utc);
            var expectedEndTime = new DateTime(2010, 01, 01, 15, 45, 0, DateTimeKind.Utc);

            var period = new DateTimePeriod(2010, 1, 1, 2010, 1, 2);
            var dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(period.LocalStartDateTime.Add(new TimeSpan(14, 30, 0)), period.LocalStartDateTime.Add(new TimeSpan(15, 45, 0)));

            using (_mockRepository.Record())
            {
                Expect.Call(_scheduleDay.Period).Return(new DateTimePeriod(startTime, endTime)).Repeat.Twice();
                Expect.Call(_scheduleDay.TimeZone).Return(TimeZoneInfoFactory.StockholmTimeZoneInfo());
            }
            using (_mockRepository.Playback())
            {
                _target = new SetupDateTimePeriodDefaultLocalHoursForActivities(_scheduleDay, new CurrentTeleoptiPrincipal(new ThreadPrincipalContext()), dateTimePeriod);
                var expect = new DateTimePeriod(expectedStartTime, expectedEndTime);
                Assert.AreEqual(expect, _target.Period);
            }
        }
    }
 }