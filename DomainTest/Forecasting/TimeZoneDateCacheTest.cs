using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class TimeZoneDateCacheTest
    {
        private TimeZoneDateCache _target;
        private DateTime _dateTimeUtc;

        [SetUp]
        public void Setup()
        {
            _dateTimeUtc = new DateTime(2010, 3, 2, 23, 0, 0, DateTimeKind.Utc);
            _target = new TimeZoneDateCache();
        }

        [Test]
        public void VerifyCanGetCachedTimeAndThenTimeFromOtherTimeZone()
        {
            var timeZone1 = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            var timeZone2 = new CccTimeZoneInfo(TimeZoneInfo.GetSystemTimeZones()[2]);

            Assert.AreEqual(new DateTime(2010, 3, 3), _target.GetLocalDateTime(_dateTimeUtc, timeZone1));
            Assert.AreEqual(timeZone2.ConvertTimeFromUtc(_dateTimeUtc,timeZone2).Date,_target.GetLocalDateTime(_dateTimeUtc,timeZone2).Date);
        }
    }
}
