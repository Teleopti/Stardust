using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Time
{
    [TestFixture]
    public class DateOnlyPeriodTest
    {
        private readonly DateOnly _start = new DateOnly(2007, 06, 01);
        private readonly DateOnly _end = new DateOnly(2008, 02, 28);
        private DateOnlyPeriod _period;
        private TimeZoneInfo _timeZoneInfo;

        [SetUp]
        public void TestSetup()
        {
            _period = new DateOnlyPeriod(_start, _end);
            _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_start, _period.StartDate);
            Assert.AreEqual(_end,_period.EndDate);
            Assert.IsNotNull(_period.DateString);
            DateOnlyPeriod period2 = new DateOnlyPeriod(_start, _end);
            Assert.IsTrue(_period.Equals(period2));
        }

		[Test]
		public void ShouldDisplayInfoInStringFormat()
		{
			_period.ArabicSafeDateString().Should().Be.EqualTo(_period.ToString());
		}

        [Test]
        public void VerifyToDateTimePeriod()
        {
            DateTime startDate =  TimeZoneHelper.ConvertToUtc(_start.Date,_timeZoneInfo);
            DateTime endDate = TimeZoneHelper.ConvertToUtc(_end.AddDays(1).Date, _timeZoneInfo);
            DateTimePeriod dateTimePeriod = _period.ToDateTimePeriod(_timeZoneInfo);
            Assert.AreEqual(startDate, dateTimePeriod.StartDateTime);
            Assert.AreEqual(endDate, dateTimePeriod.EndDateTime);

        }
        [Test]
        public void VerifyContains()
        {
            Assert.IsTrue(_period.Contains(_start));
            Assert.IsTrue(_period.Contains(_end));
            Assert.IsFalse(_period.Contains(_start.AddDays(-1)));
            Assert.IsFalse(_period.Contains(_end.AddDays(1)));
        }

        [Test]
        public void VerifyContainsPeriod()
        {
            DateOnly ett = new DateOnly(2000, 1, 1);
            DateOnly tva = new DateOnly(2000, 1, 2);
            DateOnly tre = new DateOnly(2000, 1, 3);
            DateOnly fyra = new DateOnly(2000, 1, 4);

            DateOnlyPeriod periodToCompare = new DateOnlyPeriod(tva, tre);
            Assert.IsFalse(periodToCompare.Contains(new DateOnlyPeriod(ett,tre)));
            Assert.IsFalse(periodToCompare.Contains(new DateOnlyPeriod(tva,fyra)));
            Assert.IsTrue(periodToCompare.Contains(new DateOnlyPeriod(tva,tre)));

        }

        [Test]
        public void VerifyIntersection()
        {
            DateOnlyPeriod period = new DateOnlyPeriod(new DateOnly(2007,09,01),new DateOnly(2007,09,30) );
            Assert.AreEqual(period, _period.Intersection(period));

            period = new DateOnlyPeriod(new DateOnly(2007, 04, 01), new DateOnly(2007, 09, 30));
            DateOnlyPeriod period2 = new DateOnlyPeriod(new DateOnly(2007, 06, 01), new DateOnly(2007, 09, 30));
            DateOnlyPeriod? result = _period.Intersection(period);
            Assert.AreEqual(period2, result.Value);

            period = new DateOnlyPeriod(new DateOnly(2008, 01, 01), new DateOnly(2008, 09, 30));
            period2 = new DateOnlyPeriod(new DateOnly(2008, 01, 01), new DateOnly(2008, 02, 28));
            result = _period.Intersection(period);
            Assert.AreEqual(period2, result.Value);

            period = new DateOnlyPeriod(new DateOnly(2008, 04, 01), new DateOnly(2008, 09, 30));
            result = _period.Intersection(period);
            Assert.IsNull(result);
        }

        [Test]
        public void VerifyEquals()
        {
            DateOnlyPeriod period = new DateOnlyPeriod(_start, _end);
            Assert.IsTrue(_period.Equals(period));

            Assert.IsTrue(_period == period);
            Assert.IsFalse(_period != period);

            object period2 = new DateOnlyPeriod(_start, _end);
            Assert.IsTrue(_period.Equals(period2));
            object dateTimePeriod = new DateTimePeriod();
            Assert.IsFalse(_period.Equals(dateTimePeriod));
        }

        [Test]
        public void VerifyHashCode()
        {
            Assert.IsNotNull(_period.GetHashCode());
        }

        [Test]
        public void VerifyDayCollection()
        {
            var result = _period.DayCollection();
            Assert.AreEqual(273,result.Count);
        }

        [Test]
        public void ShouldHaveSameDayCountAsNumberOfItemsInDayCollection()
        {
            var dayCollectionCount = _period.DayCollection().Count;
            _period.DayCount().Should().Be.EqualTo(dayCollectionCount);
        }

        [Test]
        public void ShouldContainOneDayGivenOneDayPeriod()
        {
            _period = new DateOnlyPeriod(_start,_start);
            _period.DayCount().Should().Be.EqualTo(1);
        }
    }
}
