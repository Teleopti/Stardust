using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class DaylightSavingTimeHelperTest
    {
        [Test]
        public void ShouldGetUtcStartEndTimeOfASpecificDay()
        {
            var currentDate = new DateTime(2012, 10, 1);
            var timeZone = (TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
            var startTime = DaylightSavingTimeHelper.GetUtcStartTimeOfOneDay(currentDate, timeZone);
            var endTime = DaylightSavingTimeHelper.GetUtcEndTimeOfOneDay(currentDate, timeZone);
            Assert.That(startTime, Is.EqualTo(new DateTime(2012, 9, 30, 16, 0, 0, DateTimeKind.Utc)));
            Assert.That(endTime, Is.EqualTo(new DateTime(2012, 10, 1, 16, 0, 0, DateTimeKind.Utc)));

			currentDate = new DateTime(2012, 10, 28);
            timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            startTime = DaylightSavingTimeHelper.GetUtcStartTimeOfOneDay(currentDate, timeZone);
            endTime = DaylightSavingTimeHelper.GetUtcEndTimeOfOneDay(currentDate, timeZone);
            Assert.That(startTime, Is.EqualTo(new DateTime(2012, 10, 27, 22, 0, 0, DateTimeKind.Utc)));
            Assert.That(endTime, Is.EqualTo(new DateTime(2012, 10, 28, 23, 0, 0, DateTimeKind.Utc)));

			currentDate = new DateTime(2012, 10, 21);
            timeZone = (TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
            startTime = DaylightSavingTimeHelper.GetUtcStartTimeOfOneDay(currentDate, timeZone);
            endTime = DaylightSavingTimeHelper.GetUtcEndTimeOfOneDay(currentDate, timeZone);
            Assert.That(startTime, Is.EqualTo(new DateTime(2012, 10, 21, 3, 0, 0, DateTimeKind.Utc)));
            Assert.That(endTime, Is.EqualTo(new DateTime(2012, 10, 22, 2, 0, 0, DateTimeKind.Utc)));

			currentDate = new DateTime(2012, 3, 25);
            timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            startTime = DaylightSavingTimeHelper.GetUtcStartTimeOfOneDay(currentDate, timeZone);
            endTime = DaylightSavingTimeHelper.GetUtcEndTimeOfOneDay(currentDate, timeZone);
            Assert.That(startTime, Is.EqualTo(new DateTime(2012, 3, 24, 23, 0, 0, DateTimeKind.Utc)));
            Assert.That(endTime, Is.EqualTo(new DateTime(2012, 3, 25, 22, 0, 0, DateTimeKind.Utc)));

			currentDate = new DateTime(2012, 2, 25);
            timeZone = (TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
            startTime = DaylightSavingTimeHelper.GetUtcStartTimeOfOneDay(currentDate, timeZone);
            endTime = DaylightSavingTimeHelper.GetUtcEndTimeOfOneDay(currentDate, timeZone);
            Assert.That(startTime, Is.EqualTo(new DateTime(2012, 2, 25, 2, 0, 0, DateTimeKind.Utc)));
            Assert.That(endTime, Is.EqualTo(new DateTime(2012, 2, 26, 3, 0, 0, DateTimeKind.Utc)));
        }
    }
}
