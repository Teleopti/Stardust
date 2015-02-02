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

		readonly DateTime _monday = new DateTime(2015, 1, 5);
		readonly DateTime _tuesday = new DateTime(2015, 1, 6);
		readonly DateTime _wendnesday = new DateTime(2015, 1, 7);
		readonly DateTime _thursday = new DateTime(2015, 1, 8);
		readonly DateTime _friday = new DateTime(2015, 1, 9);
		readonly DateTime _saturday = new DateTime(2015, 1, 10);
		readonly DateTime _sunday = new DateTime(2015, 1, 11);

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

		[Test]
		public void ShouldBeWeekendOnFridaySaturDay()
		{
			var weekendDays = new Weekend { Monday = false, Tuesday = false, Wendnesday = false, Thursday = false, Friday = true, Saturday = true, Sunday = false };

			checkWeekend(CultureInfo.GetCultureInfo(15361), weekendDays);	// Bahrain
			checkWeekend(CultureInfo.GetCultureInfo(2117), weekendDays);	// Bangladesh
			checkWeekend(CultureInfo.GetCultureInfo(3073), weekendDays);	// Egypt
			checkWeekend(CultureInfo.GetCultureInfo(2049), weekendDays);	// Iraq
			checkWeekend(CultureInfo.GetCultureInfo(1037), weekendDays);	// Israel
			checkWeekend(CultureInfo.GetCultureInfo(11265), weekendDays);	// Jordan
			checkWeekend(CultureInfo.GetCultureInfo(13313), weekendDays);	// Kuwait
			checkWeekend(CultureInfo.GetCultureInfo(4097), weekendDays);	// Libya
			checkWeekend(CultureInfo.GetCultureInfo(1125), weekendDays);	// Maldives
			checkWeekend(CultureInfo.GetCultureInfo(1086), weekendDays);	// Malaysia
			checkWeekend(CultureInfo.GetCultureInfo(1121), weekendDays);	// Nepal
			checkWeekend(CultureInfo.GetCultureInfo(8193), weekendDays);	// Oman
			checkWeekend(CultureInfo.GetCultureInfo(16385), weekendDays);	// Qatar
			checkWeekend(CultureInfo.GetCultureInfo(1025), weekendDays);	// SaudiArabia
			checkWeekend(CultureInfo.GetCultureInfo(10241), weekendDays);	// Syria
			checkWeekend(CultureInfo.GetCultureInfo(14337), weekendDays);	// Uae
			checkWeekend(CultureInfo.GetCultureInfo(9217), weekendDays);	// Yemen
		}

		[Test]
		public void ShouldBeWeekendOnFriday()
		{
			var weekendDays = new Weekend { Monday = false, Tuesday = false, Wendnesday = false, Thursday = false, Friday = true, Saturday = false, Sunday = false };

			checkWeekend(CultureInfo.GetCultureInfo(1164), weekendDays);	// Afghanistan
			checkWeekend(CultureInfo.GetCultureInfo(1065), weekendDays);	// Iran	
		}

		[Test]
		public void ShouldBeWeekendOnFridaySunday()
		{
			var weekendDays = new Weekend { Monday = false, Tuesday = false, Wendnesday = false, Thursday = false, Friday = true, Saturday = false, Sunday = true };

			checkWeekend(CultureInfo.GetCultureInfo(2110), weekendDays);	// Brunei Darussalam
		}

		[Test]
		public void ShouldBeWeekendOnSunday()
		{
			var weekendDays = new Weekend { Monday = false, Tuesday = false, Wendnesday = false, Thursday = false, Friday = false, Saturday = false, Sunday = true };

			checkWeekend(CultureInfo.GetCultureInfo(2058), weekendDays);	//Mexico
			checkWeekend(CultureInfo.GetCultureInfo(1054), weekendDays);	//Thailand
		}

		[Test]
		public void ShouldBeWeekendOnSaturdaySunday()
		{
			var weekendDays = new Weekend { Monday = false, Tuesday = false, Wendnesday = false, Thursday = false, Friday = false, Saturday = true, Sunday = true };

			checkWeekend(CultureInfo.GetCultureInfo(1053), weekendDays);	//Sweden
			checkWeekend(CultureInfo.GetCultureInfo(1044), weekendDays);	//Norway
			checkWeekend(CultureInfo.GetCultureInfo(2057), weekendDays);	//United Kingdom
			checkWeekend(CultureInfo.GetCultureInfo(1033), weekendDays);	//United States
			checkWeekend(CultureInfo.GetCultureInfo(1049), weekendDays);	//Russia
			checkWeekend(CultureInfo.GetCultureInfo(1030), weekendDays);	//Denmark
			checkWeekend(CultureInfo.GetCultureInfo(1031), weekendDays);	//Germany
			checkWeekend(CultureInfo.GetCultureInfo(1035), weekendDays);	//Finland
			checkWeekend(CultureInfo.GetCultureInfo(3079), weekendDays);	//Austria
		}


		private void checkWeekend(CultureInfo culture, Weekend weekend)
		{
			Assert.AreEqual(DateHelper.IsWeekend(_monday, culture), weekend.Monday);
			Assert.AreEqual(DateHelper.IsWeekend(_tuesday, culture), weekend.Tuesday);
			Assert.AreEqual(DateHelper.IsWeekend(_wendnesday, culture), weekend.Wendnesday);
			Assert.AreEqual(DateHelper.IsWeekend(_thursday, culture), weekend.Thursday);
			Assert.AreEqual(DateHelper.IsWeekend(_friday, culture), weekend.Friday);
			Assert.AreEqual(DateHelper.IsWeekend(_saturday, culture), weekend.Saturday);
			Assert.AreEqual(DateHelper.IsWeekend(_sunday, culture), weekend.Sunday);
		}

		private struct Weekend
		{
			public bool Monday;
			public bool Tuesday;
			public bool Wendnesday;
			public bool Thursday;
			public bool Friday;
			public bool Saturday;
			public bool Sunday;
		}
    }
}