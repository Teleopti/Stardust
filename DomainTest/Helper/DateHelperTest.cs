using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Globalization;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Helper
{
	/// <summary>
    /// Test for DateHelper
    /// </summary>
    [TestFixture]
    public class DateHelperTest
    {
        private CultureInfo _cult;
        private readonly DateTime _minSmallDateTime = new DateTime(1900,4,30);
        private readonly DateTime _maxSmallDateTime = DateHelper.MaxSmallDateTime; 
        private string _ok = string.Empty;

		[SetUp]
		public void Setup()
		{
			_cult = CultureInfo.GetCultureInfo("sv-SE");
		}

        [Test]
        public void CheckWeekNumberIsCorrect()
        {
            DateTime date = new DateTime(2007, 11, 21);
            DateTime date2 = new DateTime(2007, 12, 31);
            DateTime date3 = new DateTime(2009, 12, 29);

            Assert.AreEqual(47, DateHelper.WeekNumber(date, _cult));
            Assert.AreEqual(1, DateHelper.WeekNumber(date2, _cult));
            Assert.AreEqual(53, DateHelper.WeekNumber(date3, _cult));
        }

		[Test]
		public void CheckWeekNumberIsCorrectForSpanishCulture()
		{
			_cult = CultureInfo.GetCultureInfo("es-ES");

			DateTime date = new DateTime(2007, 11, 21);
			DateTime date2 = new DateTime(2007, 12, 31);
			DateTime date3 = new DateTime(2009, 12, 29);

			Assert.AreEqual(47, DateHelper.WeekNumber(date, _cult));
			Assert.AreEqual(1, DateHelper.WeekNumber(date2, _cult));
			Assert.AreEqual(53, DateHelper.WeekNumber(date3, _cult));
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyWeekPeriod()
        {
            var start = new DateOnly(2008, 8, 13);
            CultureInfo cult = new CultureInfo("pt-BR");

            var theWeek = DateHelper.GetWeekPeriod(start, cult);

            var weekStart = new DateOnly(2008, 8, 10);
            Assert.AreEqual(weekStart, theWeek.StartDate);
        }

        [Test]
        public void CheckReturnWeekendIsCorrect()
        {
            DateTime dateWorkday = new DateTime(2007, 11, 22);
            DateTime dateWeekend1 = new DateTime(2007, 11, 24);
            DateTime dateWeekend2 = new DateTime(2007, 11, 25);

            Assert.IsFalse(DateHelper.IsWeekend(dateWorkday, _cult));
            Assert.IsTrue(DateHelper.IsWeekend(dateWeekend1, _cult));
            Assert.IsTrue(DateHelper.IsWeekend(dateWeekend2, _cult));
        }

        /// <summary>
        /// Checks the first date of month is returned.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-20
        /// </remarks>
        [Test]
        public void CheckFirstDateOfMonthIsReturned()
        {
            DateTime dateToUse = new DateTime(2007, 1, 6);
            DateTime expected = new DateTime(2007, 1, 1);
            CultureInfo culture = CultureInfo.GetCultureInfo(1053); //Swedish

            Assert.AreEqual(expected, DateHelper.GetFirstDateInMonth(dateToUse, culture));

            expected = new DateTime(2006, 12, 22);
            culture = CultureInfo.GetCultureInfo(1025); //Arabic - Saudi
            Assert.AreEqual(expected, DateHelper.GetFirstDateInMonth(dateToUse, culture));
        }

        /// <summary>
        /// Checks the last date of month is returned.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-20
        /// </remarks>
        [Test]
        public void CheckLastDateOfMonthIsReturned()
        {
            DateTime dateToUse = new DateTime(2007, 1, 6);
            DateTime expected = new DateTime(2007, 1, 31);
            CultureInfo culture = CultureInfo.GetCultureInfo(1053); //Swedish

            Assert.AreEqual(expected, DateHelper.GetLastDateInMonth(dateToUse, culture));

            expected = new DateTime(2007, 1, 19);
            culture = CultureInfo.GetCultureInfo(1025); //Arabic - Saudi
            Assert.AreEqual(expected, DateHelper.GetLastDateInMonth(dateToUse, culture));
        }

        /// <summary>
        /// Checks the first date of week is returned.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-20
        /// </remarks>
        [Test]
        public void CheckFirstDateOfWeekIsReturned()
        {
            DateTime dateToUse = new DateTime(2007, 12, 20);
            DateTime expected = new DateTime(2007, 12, 17);
            CultureInfo culture = CultureInfo.GetCultureInfo(1053); //Swedish

            Assert.AreEqual(expected, DateHelper.GetFirstDateInWeek(dateToUse, culture));
        }

        /// <summary>
        /// Checks the last date of week is returned.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-20
        /// </remarks>
        [Test]
        public void CheckLastDateOfWeekIsReturned()
        {
            DateTime dateToUse = new DateTime(2007, 12, 20);
            DateTime expected = new DateTime(2007, 12, 23);
            CultureInfo culture = CultureInfo.GetCultureInfo(1053); //Swedish

            Assert.AreEqual(expected, DateHelper.GetLastDateInWeek(dateToUse, culture));
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldGetQuarter()
		{
			DateHelper.GetQuarter(1).Should().Be.EqualTo(1);
			DateHelper.GetQuarter(2).Should().Be.EqualTo(1);
			DateHelper.GetQuarter(3).Should().Be.EqualTo(1);
			DateHelper.GetQuarter(4).Should().Be.EqualTo(2);
			DateHelper.GetQuarter(5).Should().Be.EqualTo(2);
			DateHelper.GetQuarter(6).Should().Be.EqualTo(2);
			DateHelper.GetQuarter(7).Should().Be.EqualTo(3);
			DateHelper.GetQuarter(8).Should().Be.EqualTo(3);
			DateHelper.GetQuarter(9).Should().Be.EqualTo(3);
			DateHelper.GetQuarter(10).Should().Be.EqualTo(4);
			DateHelper.GetQuarter(11).Should().Be.EqualTo(4);
			DateHelper.GetQuarter(12).Should().Be.EqualTo(4);
			DateHelper.GetQuarter(13).Should().Be.EqualTo(0);
			DateHelper.GetQuarter(-1).Should().Be.EqualTo(0);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldGetWeekPeriod()
		{
			var result = DateHelper.GetWeekPeriod(new DateOnly(2012, 10, 16), DayOfWeek.Monday);
			result.StartDate.Should().Be.EqualTo(new DateOnly(2012, 10, 15));
			result.EndDate.Should().Be.EqualTo(new DateOnly(2012, 10, 21));
		}

        /// <summary>
        /// Verifies the day of week collection is returned.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-20
        /// </remarks>
        [Test]
        public void VerifyDayOfWeekCollectionIsReturned()
        {
            CultureInfo culture = CultureInfo.GetCultureInfo(1053); //Swedish
            IList<DayOfWeek> weekDays = DateHelper.GetDaysOfWeek(culture);

            Assert.AreEqual(7, weekDays.Count);
            Assert.AreEqual(DayOfWeek.Monday, weekDays[0]);
            Assert.AreEqual(DayOfWeek.Sunday, weekDays[6]);
        }

		[Test]
		public void ShouldGetWeekdayNames() {
			var result = DateHelper.GetWeekdayNames(CultureInfo.CurrentUICulture);

			var expected = DateHelper.GetDaysOfWeek(CultureInfo.CurrentUICulture).Select(d => CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(d));

			Assert.That(result.Count(), Is.EqualTo(7));
			CollectionAssert.AreEqual(expected, result);
		}

        /// <summary>
        /// Verifies that the correct month name is returned.
        /// </summary>
        /// <remarks>
        /// Created by: jonas n
        /// Created date: 2008-02-09
        /// </remarks>
        [Test]
        public void VerifyMonthName()
        {
            DateTime janDate = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime julDate = new DateTime(2007, 7, 31, 0, 0, 0, DateTimeKind.Utc);
            DateTime decDate = new DateTime(2007, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            CultureInfo gbCult = new CultureInfo("en-GB");

            Assert.AreEqual("januari", DateHelper.GetMonthName(janDate, _cult));
            Assert.AreEqual("January", DateHelper.GetMonthName(janDate, gbCult));
            Assert.AreEqual("juli", DateHelper.GetMonthName(julDate, _cult));
            Assert.AreEqual("July", DateHelper.GetMonthName(julDate, gbCult));
            Assert.AreEqual("december", DateHelper.GetMonthName(decDate, _cult));
            Assert.AreEqual("December", DateHelper.GetMonthName(decDate, gbCult));

        }

        /// <summary>
        /// Verify that correct hour:min string is returned
        /// </summary>
        [Test]
        public void CheckHourMinutesString()
        {
            _ok = "01:09";
            Assert.AreEqual(_ok, DateHelper.HourMinutesString(69));

            _ok = "00:00";
            Assert.AreEqual(_ok, DateHelper.HourMinutesString(0));

            _ok = "500:59";
            Assert.AreEqual(_ok, DateHelper.HourMinutesString(30059));
        }

        [Test]
        public void VerifySmallDateTimeMin()
        {
            Assert.AreEqual(_minSmallDateTime,DateHelper.MinSmallDateTime);
            Assert.AreEqual(_maxSmallDateTime,DateHelper.MaxSmallDateTime);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifySplitDateTimePeriod()
        {
            TimeSpan daysToSplitOn = TimeSpan.FromDays(90);

            DateTime startDateTime = new DateTime(2001, 04, 23);
            DateTime endDateTime = new DateTime(2005, 08, 15);
            DateTimePeriod dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTime, endDateTime);
            IEnumerable<DateTimePeriod> dateTimePeriods = DateHelper.SplitDateTimePeriod(dateTimePeriod, daysToSplitOn);

            Assert.AreEqual(18, dateTimePeriods.Count());
            Assert.AreEqual(startDateTime, dateTimePeriods.First().LocalStartDateTime);
            Assert.AreEqual(endDateTime, dateTimePeriods.Last().LocalEndDateTime);

            startDateTime = new DateTime(2001, 04, 23);
            endDateTime = new DateTime(2001, 08, 15);
            dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTime, endDateTime);
            dateTimePeriods = DateHelper.SplitDateTimePeriod(dateTimePeriod, daysToSplitOn);
            Assert.AreEqual(2, dateTimePeriods.Count());

            startDateTime = new DateTime(2008, 01, 01);
            endDateTime = new DateTime(2009, 12, 31);
            dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTime, endDateTime);
            dateTimePeriods = DateHelper.SplitDateTimePeriod(dateTimePeriod, daysToSplitOn);
            Assert.AreEqual(9, dateTimePeriods.Count());
        }

        [Test]
        public void ShouldReturnMinSmallDateTimeInstead()
        {
            var tooSmallDate = _minSmallDateTime.AddDays(-3).LimitMin();
            Assert.AreEqual(_minSmallDateTime, tooSmallDate);
        }

        [Test]
        public void ShouldReturnDateTimeWhenLargerThanMin()
        {
            var tooSmallDate = _minSmallDateTime.AddDays(3).LimitMin();
            Assert.AreEqual(_minSmallDateTime.AddDays(3), tooSmallDate);
        }
    }
}