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
            Assert.AreEqual(5, _baseDateTime.ToInterval(5).Minute,  "3 to 5, Rounds up");
            Assert.AreEqual(0, _baseDateTime.ToInterval(5, IntervalRounding.Down).Minute);

            Assert.AreEqual(0, _baseDateTime.ToInterval(10).Minute,  "3 to 10, Rounds down");
            Assert.AreEqual(15, _baseDateTime.ToInterval(15,IntervalRounding.Up).Minute);
           
            Assert.AreEqual(3, _baseDateTime.ToInterval(1).Minute, "3 to 3, Rounds to minute");
            Assert.AreEqual(3, _baseDateTime.ToInterval(TimeSpan.FromMinutes(1)).Minute, "3 to 3, Rounds to minute");
          
        }


        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyResolutionIsGreaterThanZero()
        {
            _baseDateTime.ToInterval(0);
        }


        [Test]
        public void VerifyHourIsChangedWhenRoundingUpToNearestHour()
        {
            int originalHour = _baseDateTime.Hour;
            _baseDateTime = _baseDateTime.AddMinutes(55);
            Assert.AreEqual(originalHour + 1, _baseDateTime.ToInterval(60).Hour);
            Assert.AreEqual(originalHour + 1, _baseDateTime.ToInterval(TimeSpan.FromHours(1)).Hour);
        }

        [Test]
        public void VerifyDateTimeKindIsTheSame()
        {
            Assert.AreEqual(DateTimeKind.Utc, _baseDateTime.ToInterval(1).Kind);
            _baseDateTime = DateTime.SpecifyKind(_baseDateTime,DateTimeKind.Local);
            Assert.AreEqual(DateTimeKind.Local, _baseDateTime.ToInterval(1).Kind);
        }

        [Test, SetCulture("sv-SE")]
        public void ShouldShowShortDateTimeStringWithDays()
        {
            var dateString = DateTime.MinValue.Add(TimeSpan.FromHours(25)).ToShortTimeStringWithDays();
            Assert.AreEqual("01:00+1", dateString);
        }
    }
}
