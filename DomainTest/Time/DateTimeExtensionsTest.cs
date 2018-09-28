using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.DomainTest.Time
{
    [TestFixture]
    public class DateTimeExtensionsTest
    {
        private DateTime _baseDateTime;

        [SetUp]
        public void Setup()
        {
            _baseDateTime = new DateTime(2001, 1, 1, 8, 3, 3, DateTimeKind.Utc);
        }

        [Test]
        public void VerifyCanConvertToInterval()
        {
            Assert.AreEqual(0, _baseDateTime.ToInterval(5, IntervalRounding.Down).Minute);

            Assert.AreEqual(15, _baseDateTime.ToInterval(15,IntervalRounding.Up).Minute);
           
            Assert.AreEqual(3, _baseDateTime.ToInterval(TimeSpan.FromMinutes(1)).Minute, "3 to 3, Rounds to minute");
          
        }

        [Test]
        public void VerifyHourIsChangedWhenRoundingUpToNearestHour()
        {
            int originalHour = _baseDateTime.Hour;
            _baseDateTime = _baseDateTime.AddMinutes(55);
            Assert.AreEqual(originalHour + 1, _baseDateTime.ToInterval(TimeSpan.FromHours(1)).Hour);
        }

        [Test, SetCulture("sv-SE")]
        public void ShouldShowShortDateTimeStringWithDays()
        {
            var dateString = DateTime.MinValue.Add(TimeSpan.FromHours(25)).ToShortTimeStringWithDays();
            Assert.AreEqual("01:00 +1", dateString);
        }
    }
}
