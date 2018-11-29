using System;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Helper
{
    /// <summary>
    /// Tests class TimeZoneHelper
    /// </summary>
    [TestFixture]
    public class TimeZoneHelperTest
    {
        /// <summary>
        /// Verifies the create new date time period with time zone supplied.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-29
        /// </remarks>
        [Test]
        public void VerifyCreateNewDateTimePeriodWithTimeZoneSupplied()
        {
            DateTime utcStartDateTime = DateTime.UtcNow.AddHours(-1);
            DateTime utcEndDateTime = DateTime.UtcNow.AddHours(1);
            DateTime localStartDateTime =
                TimeZoneInfo.ConvertTimeFromUtc(utcStartDateTime,
                                                TimeZoneInfo.Local);
            DateTime localEndDateTime =
                TimeZoneInfo.ConvertTimeFromUtc(utcEndDateTime,
                                                TimeZoneInfo.Local);

            DateTimePeriod expectedDateTimePeriod = new DateTimePeriod(utcStartDateTime, utcEndDateTime);

            Assert.AreEqual(expectedDateTimePeriod,
                            TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(localStartDateTime, localEndDateTime, TimeZoneInfo.Local));
        }
		
        /// <summary>
        /// Verifies the create new date time period with time zone supplied.
        /// Part of SPI 9115 fix.
        /// 2008-03-30 02:15:00 - 2008-03-30 02:15:00
        /// </summary>
        /// <remarks>
        /// Created by: Henry Greijer
        /// Created date: 2010-01-28
        /// </remarks>
        [Test, SetUICulture("en-GB"), SetCulture("en-GB")]
        public void VerifyCreateNewDateTimePeriodCloseToDaylightSavingBreak()
        {
            //2008-03-30 03:15:00
            TimeZoneInfo wetTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

            DateTime localStartDateTime = new DateTime(2008, 3, 30, 2, 0, 0, DateTimeKind.Unspecified);
            DateTime localEndDateTime = new DateTime(2008, 3, 30, 2, 15, 0, DateTimeKind.Unspecified);

            DateTime utcStartDateTime = new DateTime(2008, 3, 30, 1, 0, 0, DateTimeKind.Utc);
            DateTime utcEndDateTime = new DateTime(2008, 3, 30, 1, 0, 0, DateTimeKind.Utc);
            DateTimePeriod expectedDateTimePeriod = new DateTimePeriod(utcStartDateTime, utcEndDateTime);
            DateTimePeriod actualDateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(localStartDateTime, localEndDateTime, wetTimeZoneInfo);
            Assert.AreEqual(expectedDateTimePeriod, actualDateTimePeriod);

            localStartDateTime = new DateTime(2008, 3, 30, 2, 45, 0, DateTimeKind.Unspecified);
            localEndDateTime = new DateTime(2008, 3, 30, 3, 0, 0, DateTimeKind.Unspecified);

            utcStartDateTime = new DateTime(2008, 3, 30, 1, 0, 0, DateTimeKind.Utc);
            utcEndDateTime = new DateTime(2008, 3, 30, 1, 0, 0, DateTimeKind.Utc);
            expectedDateTimePeriod = new DateTimePeriod(utcStartDateTime, utcEndDateTime);
            actualDateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(localStartDateTime, localEndDateTime, wetTimeZoneInfo);
            Assert.AreEqual(expectedDateTimePeriod, actualDateTimePeriod);
        }

        /// <summary>
        /// Verifies the create new date time period with time zone supplied.
        /// Part of SPI 9115 fix.
        /// Timezone Amman (Jordan) and Saudi Culture.
        /// </summary>
        /// <remarks>
        /// Created by: Henry Greijer
        /// Created date: 2010-01-28
        /// </remarks>
        [Test,SetCulture("ar-SA"), SetUICulture("ar-SA")]
        public void VerifyCreateNewDateTimePeriodWithSaudiCultureAndJordanTimeZone()
        {
            TimeZoneInfo jordanTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time");

            DateTime localStartDateTime = DateTime.SpecifyKind(new DateTime(1422, 1, 5, CultureInfo.CurrentCulture.Calendar), DateTimeKind.Unspecified);
            DateTime localEndDateTime = DateTime.SpecifyKind(new DateTime(1422, 12, 25, CultureInfo.CurrentCulture.Calendar), DateTimeKind.Unspecified);

            localStartDateTime = localStartDateTime.AddHours(1); // The date 1422-01-05 12:00 is invalid so add an hour.
            DateTime utcStartDateTime = TimeZoneInfo.ConvertTimeToUtc(localStartDateTime, jordanTimeZoneInfo);
            DateTime utcEndDateTime = TimeZoneInfo.ConvertTimeToUtc(localEndDateTime, jordanTimeZoneInfo);
            DateTimePeriod expectedDateTimePeriod = new DateTimePeriod(utcStartDateTime, utcEndDateTime);
            DateTimePeriod actualDateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(localStartDateTime, localEndDateTime, jordanTimeZoneInfo);
            Assert.AreEqual(expectedDateTimePeriod, actualDateTimePeriod);
        }

        /// <summary>
        /// Verifies the create new date time period with null as time zone.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-29
        /// </remarks>
        [Test]
        public void VerifyCreateNewDateTimePeriodWithNullAsTimeZone()
        {
            DateTime utcStartDateTime = DateTime.UtcNow.AddHours(-1);
            DateTime utcEndDateTime = DateTime.UtcNow.AddHours(1);
			Assert.Throws<ArgumentNullException>(() => TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(utcStartDateTime, utcEndDateTime, null));
        }
		
	    [Test]
	    public void MakeSureGetDaylightChangesIfThereAreSomeForTheYear()
	    {
		    var timezoneInfoList = TimeZoneInfo.GetSystemTimeZones();
		    foreach (var timeZoneInfo in timezoneInfoList)
		    {
			    if (timeZoneInfo.SupportsDaylightSavingTime && timeZoneInfo.IsDaylightSavingTime(DateTime.UtcNow))
			    {
				    TimeZoneHelper.GetDaylightChanges(timeZoneInfo, DateTime.UtcNow.Year).Should().Not.Be(null);
			    }
		    }
	    }
		
	    [Test]
	    public void VerifyGetDaylightChangesReturnCorrectDayLightTime()
	    {

			var swedenTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
			var thisYear = 2015;

			var swedenDaylightSavingTimeRule = TimeZoneHelper.GetDaylightChanges(swedenTimeZone, thisYear);
			swedenDaylightSavingTimeRule.Start.Should().Be(new DateTime(2015, 3, 29, 1, 00, 00));
			swedenDaylightSavingTimeRule.End.Should().Be(new DateTime(2015, 10, 25, 2, 00, 00));
			swedenDaylightSavingTimeRule.Delta.Should().Be(TimeSpan.FromHours(1));

			var brasiliaDaylightSavingTimeRule = TimeZoneHelper.GetDaylightChanges(brasiliaTimeZone, thisYear);
			brasiliaDaylightSavingTimeRule.Start.Should().Be(new DateTime(2015, 10, 18, 2, 59, 59));
			brasiliaDaylightSavingTimeRule.End.Should().Be(new DateTime(2015, 2, 22, 2, 59, 59));
			brasiliaDaylightSavingTimeRule.Delta.Should().Be(TimeSpan.FromHours(1));
	    }
    }
}