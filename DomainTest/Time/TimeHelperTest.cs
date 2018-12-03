using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Globalization;
using System.Threading;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Time
{
    /// <summary>
    /// Tests for the TimeHelper class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-14
    /// </remarks>
    [TestFixture]
    public class TimeHelperTest
    {
        private TimeSpan timeValue;

	    [Test]
	    public void ShouldInterpret24HoursAs24HoursBug44951()
	    {
			string timeAsText = "24:00:00";
		    Assert.IsTrue(TimeHelper.TryParseLongHourStringDefaultInterpretation(timeAsText, TimeSpan.MaxValue, out timeValue, TimeFormatsType.HoursMinutesSeconds, false));
		    Assert.That(timeValue, Is.EqualTo(TimeSpan.FromHours(24)));
		}

        [Test]
        public void CanUseDefaultInterpretationParseAsMinutes()
        {
            string timeAsText = "15";
            Assert.IsTrue(TimeHelper.TryParseLongHourStringDefaultInterpretation(timeAsText, TimeSpan.FromHours(24), out timeValue, TimeFormatsType.HoursMinutes, true));
            Assert.That(timeValue, Is.EqualTo(new TimeSpan(0, 15, 0)));

            timeAsText = "0:15";
            Assert.IsTrue(TimeHelper.TryParseLongHourStringDefaultInterpretation(timeAsText, TimeSpan.FromHours(24), out timeValue, TimeFormatsType.HoursMinutes, true));
            Assert.AreEqual(new TimeSpan(0, 15, 0), timeValue);

            timeAsText = "1:15";
            Assert.IsTrue(TimeHelper.TryParseLongHourStringDefaultInterpretation(timeAsText, TimeSpan.FromHours(24), out timeValue, TimeFormatsType.HoursMinutes, true));
            Assert.AreEqual(new TimeSpan(1, 15, 0), timeValue);

            timeAsText = "15";
            Assert.IsTrue(TimeHelper.TryParseLongHourStringDefaultInterpretation(timeAsText, TimeSpan.FromHours(24), out timeValue, TimeFormatsType.HoursMinutes, false));
            Assert.That(timeValue, Is.EqualTo(new TimeSpan(15, 0, 0)));

            timeAsText = "256204779";
            Assert.IsFalse(TimeHelper.TryParseLongHourStringDefaultInterpretation(timeAsText, TimeSpan.FromHours(24), out timeValue, TimeFormatsType.HoursMinutes, false));
            Assert.That(timeValue, Is.EqualTo(new TimeSpan(24, 0, 0)));

            timeAsText = "256204779";
            Assert.IsFalse(TimeHelper.TryParseLongHourStringDefaultInterpretation(timeAsText, TimeSpan.FromHours(24), out timeValue, TimeFormatsType.HoursMinutes, true));
            Assert.That(timeValue, Is.EqualTo(new TimeSpan(24, 0, 0)));
        }
        
        [Test]
        public void CanParseTimeFromText()
        {
            string timeAsText = "2pm";

            Assert.IsTrue(TimeHelper.TryParse(timeAsText, out timeValue));
            Assert.AreEqual(new TimeSpan(14, 0, 0), timeValue);
        }

        [Test]
        public void CanParseTimeFromTextWithPlusOne()
        {
            string timeAsText = "2pm +1";

            Assert.IsTrue(TimeHelper.TryParse(timeAsText, out timeValue));
            Assert.AreEqual(new TimeSpan(38, 0, 0), timeValue);
        }

        [Test]
        public void CanParseTimeFromTextWithPlusOneNoSpacing()
        {
            string timeAsText = "2pm+1";

            Assert.IsTrue(TimeHelper.TryParse(timeAsText, out timeValue));
            Assert.AreEqual(new TimeSpan(38, 0, 0), timeValue);
        }

        [Test]
        public void CanParseTimeFromTextAsInteger()
        {
            string timeAsText = "20";

            Assert.IsTrue(TimeHelper.TryParse(timeAsText, out timeValue));
            Assert.AreEqual(new TimeSpan(20, 0, 0), timeValue);

            timeAsText = "090";

            Assert.IsTrue(TimeHelper.TryParse(timeAsText, out timeValue));
            Assert.AreEqual(new TimeSpan(9, 0, 0), timeValue);
        }

        /// <summary>
        /// Determines whether this instance [can parse time from text as integer zero].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-07
        /// </remarks>
        [Test]
        public void CanParseTimeFromTextAsIntegerZero()
        {
            string timeAsText = "00:00";

            Assert.IsTrue(TimeHelper.TryParse(timeAsText, out timeValue));
            Assert.AreEqual(new TimeSpan(0, 0, 0), timeValue);
        }

        /// <summary>
        /// Determines whether this instance [can parse time from text as large integer].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-14
        /// </remarks>
        [Test]
        public void CanParseTimeFromTextAsLargeInteger()
        {
            string timeAsText = "1906";

            Assert.IsTrue(TimeHelper.TryParse(timeAsText, out timeValue));
            Assert.AreEqual(new TimeSpan(19, 6, 0), timeValue);
        }

        /// <summary>
        /// Determines whether this instance [can parse time from text with PM in the middle].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-07
        /// </remarks>
        [Test]
        public void CanParseTimeFromTextWithPMInTheMiddle()
        {
            string timeAsText = "1pm06";

            Assert.IsTrue(TimeHelper.TryParse(timeAsText, out timeValue));
            Assert.AreEqual(new TimeSpan(13, 6, 0), timeValue);
        }

        /// <summary>
        /// Determines whether this instance [can parse time from text with point].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-07
        /// </remarks>
        [Test, SetCulture("en-US")]
        public void CanParseTimeFromTextWithPoint()
        {
            string timeAsText = "19.06";

            Assert.IsTrue(TimeHelper.TryParse(timeAsText, out timeValue));
            Assert.AreEqual(new TimeSpan(19, 6, 0), timeValue);
        }

        /// <summary>
        /// Cannots the parse time from text as dumb string.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-14
        /// </remarks>
        [Test]
        public void CannotParseTimeFromTextAsDumbString()
        {
            string timeAsText = "asdf";

            Assert.IsFalse(TimeHelper.TryParse(timeAsText, out timeValue));
        }

        /// <summary>
        /// Verifies the try parse time periods with all cultures.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-12
        /// </remarks>
        [Test]
        public void VerifyTryParseTimePeriodsWithAllCultures()
        {
            foreach (CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                Thread.CurrentThread.CurrentCulture = cultureInfo;

                IList<TimePeriod> result;
                Assert.IsTrue(TimeHelper.TryParseTimePeriods("08:00 - 17:00; ;; 17:40 - 22:00", out result));
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual(
                    new TimePeriod(
                        TimeSpan.FromHours(8d), 
                        TimeSpan.FromHours(17d)), 
                        result[0]);
                Assert.AreEqual(
                    new TimePeriod(
                        TimeSpan.FromHours(17d).Add(TimeSpan.FromMinutes(40d)), 
                        TimeSpan.FromHours(22d)), 
                        result[1]);
            }
        }

        /// <summary>
        /// Verifies the try parse invalid time periods with all cultures returns false.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-13
        /// </remarks>
        [Test]
        public void VerifyTryParseInvalidTimePeriodsWithAllCulturesReturnsFalse()
        {
            foreach (CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                Thread.CurrentThread.CurrentCulture = cultureInfo;

                IList<TimePeriod> result;
                Assert.IsFalse(TimeHelper.TryParseTimePeriods("08:00 / 17:00; ;; 17:40 - 22:00", out result));
            }
        }

        [Test]
        public void VerifyFitToDefaultResolutionRoundDown()
        {
            int defaultResolution = 30;
            timeValue = new TimeSpan(2, 15, 0);
            TimeSpan expectedSpan = new TimeSpan(2, 0, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolutionRoundDown(timeValue, defaultResolution));
        }

        [Test]
        public void VerifyFitToDefaultResolutionRoundDownToHalfHour()
        {
            int defaultResolution = 30;
            timeValue = new TimeSpan(2, 45, 0);
            TimeSpan expectedSpan = new TimeSpan(2, 30, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolutionRoundDown(timeValue, defaultResolution));
        }

        [Test]
        public void VerifyFitToDefaultResolution()
        {
            int defaultResolution = 10;
            timeValue = new TimeSpan(2, 15, 0);
            TimeSpan expectedSpan = new TimeSpan(2, 20, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));
            timeValue = new TimeSpan(2, 20, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));
        }

        [Test]
        public void VerifyFitToDefaultResolutionWithFourHours()
        {
            int defaultResolution = 240;
            timeValue = new TimeSpan(2, 15, 0);
            TimeSpan expectedSpan = new TimeSpan(4, 0, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));
            timeValue = new TimeSpan(4, 0, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));
        }

        [Test]
        public void VerifyFitToDefaultResolutionDoesConsiderExtraTics()
        {
            int defaultResolution = 10;
            timeValue = new TimeSpan(2, 21, 0);
            TimeSpan expectedSpan = new TimeSpan(2, 20, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));
            timeValue = new TimeSpan(0, 2, 21, 1, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));
            timeValue = new TimeSpan(0, 2, 21, 0, 1);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));
            timeValue = new TimeSpan(0, 2, 21, 0, 0).Add(TimeSpan.FromTicks(1));
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));
        }

        [Test]
        public void VerifyFitToDefaultResolutionRoundsCorrect()
        {
            int defaultResolution = 30;
            timeValue = new TimeSpan(2, 14, 59);
            TimeSpan expectedSpan = new TimeSpan(2, 0, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));
            timeValue = new TimeSpan(2, 15, 1);
            expectedSpan = new TimeSpan(2, 30, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));
            timeValue = new TimeSpan(2, 15, 0);
            expectedSpan = new TimeSpan(2, 30, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));
            timeValue = new TimeSpan(2, 43, 0);
            expectedSpan = new TimeSpan(2, 30, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));
            timeValue = new TimeSpan(2, 46, 0);
            expectedSpan = new TimeSpan(3, 0, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));

            defaultResolution = 15;
            timeValue = new TimeSpan(2, 46, 0);
            expectedSpan = new TimeSpan(2, 45, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));

            timeValue = new TimeSpan(2, 56, 0);
            expectedSpan = new TimeSpan(3, 0, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));

            timeValue = new TimeSpan(2, 2, 0);
            expectedSpan = new TimeSpan(2, 0, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));

            timeValue = new TimeSpan(2, 12, 0);
            expectedSpan = new TimeSpan(2, 15, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));

            timeValue = new TimeSpan(2, 16, 0);
            expectedSpan = new TimeSpan(2, 15, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));

            timeValue = new TimeSpan(2, 25, 23);
            expectedSpan = new TimeSpan(2, 30, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));

            timeValue = new TimeSpan(2, 0, 0);
            expectedSpan = new TimeSpan(2, 0, 0);
            Assert.AreEqual(expectedSpan, TimeHelper.FitToDefaultResolution(timeValue, defaultResolution));
        }

        [Test]
        public void VerifyCanParseLongHourTimeStringFormatHourMinutesSeconds()
        {
            timeValue = new TimeSpan();
            Assert.IsTrue(TimeHelper.TryParseLongHourString("2:1", out timeValue, TimeFormatsType.HoursMinutesSeconds));
            Assert.AreEqual(new TimeSpan(2, 1, 0), timeValue);
            Assert.IsTrue(TimeHelper.TryParseLongHourString("2:22", out timeValue, TimeFormatsType.HoursMinutesSeconds));
            Assert.AreEqual(new TimeSpan(2, 22, 0), timeValue);
            Assert.IsTrue(TimeHelper.TryParseLongHourString("25:15", out timeValue, TimeFormatsType.HoursMinutesSeconds));
            Assert.AreEqual(new TimeSpan(25, 15, 0), timeValue);
            Assert.IsTrue(TimeHelper.TryParseLongHourString("1", out timeValue, TimeFormatsType.HoursMinutesSeconds));
            Assert.AreEqual(new TimeSpan(1, 0, 0), timeValue);
            Assert.IsTrue(TimeHelper.TryParseLongHourString("0:1", out timeValue, TimeFormatsType.HoursMinutesSeconds));
            Assert.AreEqual(new TimeSpan(0, 1, 0), timeValue);
            Assert.IsFalse(TimeHelper.TryParseLongHourString("25:60", out timeValue, TimeFormatsType.HoursMinutesSeconds));
            Assert.AreEqual(new TimeSpan(), timeValue);
            Assert.IsFalse(TimeHelper.TryParseLongHourString("abracadabra", out timeValue, TimeFormatsType.HoursMinutesSeconds));
            Assert.AreEqual(new TimeSpan(), timeValue);
            Assert.IsTrue(TimeHelper.TryParseLongHourString("1:1:1", out timeValue, TimeFormatsType.HoursMinutesSeconds));
            Assert.AreEqual(new TimeSpan(1,1,1), timeValue);
            Assert.IsFalse(TimeHelper.TryParseLongHourString("1:we", out timeValue, TimeFormatsType.HoursMinutesSeconds));
            Assert.AreEqual(new TimeSpan(), timeValue);

            Assert.IsFalse(TimeHelper.TryParseLongHourString("1:0:3:3", out timeValue, TimeFormatsType.HoursMinutesSeconds));
            Assert.IsFalse(TimeHelper.TryParseLongHourString("1:e:3", out timeValue, TimeFormatsType.HoursMinutesSeconds));
            Assert.IsFalse(TimeHelper.TryParseLongHourString("q:2:3", out timeValue, TimeFormatsType.HoursMinutesSeconds));
            Assert.IsFalse(TimeHelper.TryParseLongHourString("1:2:e", out timeValue, TimeFormatsType.HoursMinutesSeconds));
            Assert.IsFalse(TimeHelper.TryParseLongHourString("1:2:60", out timeValue, TimeFormatsType.HoursMinutesSeconds));
            Assert.IsFalse(TimeHelper.TryParseLongHourString("wer:2", out timeValue, TimeFormatsType.HoursMinutesSeconds));
            Assert.IsTrue(TimeHelper.TryParseLongHourString("60", out timeValue, TimeFormatsType.HoursMinutesSeconds));
        }

        [Test]
        public void VerifyCanParseLongHourTimeStringFormatHourMinutes()
        {
            timeValue = new TimeSpan();
            Assert.IsTrue(TimeHelper.TryParseLongHourString("2:1", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.AreEqual(new TimeSpan(2, 1, 0), timeValue);
            Assert.IsTrue(TimeHelper.TryParseLongHourString("2:22", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.AreEqual(new TimeSpan(2, 22, 0), timeValue);
            Assert.IsTrue(TimeHelper.TryParseLongHourString("25:15", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.AreEqual(new TimeSpan(25, 15, 0), timeValue);
            Assert.IsTrue(TimeHelper.TryParseLongHourString("1", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.AreEqual(new TimeSpan(1, 0, 0), timeValue);
            Assert.IsTrue(TimeHelper.TryParseLongHourString("0:1", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.AreEqual(new TimeSpan(0, 1, 0), timeValue);
            Assert.IsTrue(TimeHelper.TryParseLongHourString("02:01", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.AreEqual(new TimeSpan(2, 1, 0), timeValue);
            Assert.IsTrue(TimeHelper.TryParseLongHourString("2:01", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.AreEqual(new TimeSpan(2, 1, 0), timeValue);
            Assert.IsTrue(TimeHelper.TryParseLongHourString("02:1", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.AreEqual(new TimeSpan(2, 1, 0), timeValue);

            Assert.IsTrue(TimeHelper.TryParseLongHourString("-2", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.AreEqual(new TimeSpan(-2, 0, 0), timeValue);
            Assert.IsTrue(TimeHelper.TryParseLongHourString("-2:1", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.That(timeValue, Is.EqualTo(new TimeSpan(-2, -1, 0)));
            Assert.IsTrue(TimeHelper.TryParseLongHourString("-02:01", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.That(timeValue, Is.EqualTo(new TimeSpan(-2, -1, 0)));
            Assert.IsTrue(TimeHelper.TryParseLongHourString("0", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.AreEqual(new TimeSpan(0, 0, 0), timeValue);
            Assert.IsTrue(TimeHelper.TryParseLongHourString("-0", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.AreEqual(new TimeSpan(0, 0, 0), timeValue);

            Assert.IsFalse(TimeHelper.TryParseLongHourString("25:60", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.AreEqual(new TimeSpan(), timeValue);
            Assert.IsFalse(TimeHelper.TryParseLongHourString("abracadabra", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.AreEqual(new TimeSpan(), timeValue);
            Assert.IsFalse(TimeHelper.TryParseLongHourString("1:1:1", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.AreEqual(new TimeSpan(), timeValue);
            Assert.IsFalse(TimeHelper.TryParseLongHourString("1:we", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.AreEqual(new TimeSpan(), timeValue);

            Assert.IsFalse(TimeHelper.TryParseLongHourString("1:2.2", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.IsFalse(TimeHelper.TryParseLongHourString("1.2:2", out timeValue, TimeFormatsType.HoursMinutes));
            Assert.IsTrue(TimeHelper.TryParseLongHourString("60", out timeValue, TimeFormatsType.HoursMinutes));
            TimeHelper.GetLongHourMinuteSecondTimeString(new TimeSpan(0, 1, 2), CultureInfo.CurrentCulture);
        }


        [Test]
        public void VerifyGetLongHourMinuteSecondTimeString()
        {
            timeValue = new TimeSpan(0, 1, 2, 3);
			CultureInfo ci = CultureInfo.GetCultureInfo(1033);
            Assert.AreEqual("1:02:03", TimeHelper.GetLongHourMinuteSecondTimeString(timeValue, ci));
            timeValue = new TimeSpan(0, 0, 20, 30);
            Assert.AreEqual("0:20:30", TimeHelper.GetLongHourMinuteSecondTimeString(timeValue, ci));
            timeValue = new TimeSpan(0, 0, 20, 60);
            Assert.AreEqual("0:21:00", TimeHelper.GetLongHourMinuteSecondTimeString(timeValue, ci));
            timeValue = new TimeSpan(0, 0, -20, -60);
            Assert.AreEqual("-0:21:00", TimeHelper.GetLongHourMinuteSecondTimeString(timeValue, ci));
            timeValue = TimeSpan.FromMinutes(-90);
            Assert.AreEqual("-1:30:00", TimeHelper.GetLongHourMinuteSecondTimeString(timeValue, ci));
        }

        [Test]
        public void VerifyGetLongHourMinuteSecondTimeStringRoundingProblemIsRemoved()
        {
            
            CultureInfo ci = CultureInfo.GetCultureInfo(1033);
            timeValue = new TimeSpan(0, 9, 30, 0);
            Assert.AreEqual("9:30:00", TimeHelper.GetLongHourMinuteSecondTimeString(timeValue, ci));
            timeValue = new TimeSpan(0, 9, 30, 30);
            Assert.AreEqual("9:30:30", TimeHelper.GetLongHourMinuteSecondTimeString(timeValue, ci));
            timeValue = new TimeSpan(0, 9, 30, 30, 500);
            Assert.AreEqual("9:30:31", TimeHelper.GetLongHourMinuteSecondTimeString(timeValue, ci));
            timeValue = new TimeSpan(1, 12, 0, 0, 0);
            Assert.AreEqual("36:00:00", TimeHelper.GetLongHourMinuteSecondTimeString(timeValue, ci));
        }

        [Test]
        public void VerifyGetLongHourMinuteTimeString()
        {
            timeValue = new TimeSpan(0, 1, 2, 3);
            CultureInfo ci = CultureInfo.CurrentCulture;
            Assert.AreEqual("1:02", TimeHelper.GetLongHourMinuteTimeString(timeValue, ci));
            timeValue = new TimeSpan(0, 0, 20, 30);
            Assert.AreEqual("0:21", TimeHelper.GetLongHourMinuteTimeString(timeValue, ci));
            timeValue = new TimeSpan(0, 0, 20, 60);
            Assert.AreEqual("0:21", TimeHelper.GetLongHourMinuteTimeString(timeValue, ci));
            timeValue = new TimeSpan(0, 0, -20, -60);
            Assert.AreEqual("-0:21", TimeHelper.GetLongHourMinuteTimeString(timeValue, ci));
            timeValue = TimeSpan.FromHours(-36.5);
            Assert.AreEqual("-36:30", TimeHelper.GetLongHourMinuteTimeString(timeValue, ci));
        }

        [Test]
        public void VerifyGetLongHourMinuteTimeStringRoundingProblemIsRemoved()
        {
            CultureInfo ci = CultureInfo.GetCultureInfo(1033);
            timeValue = new TimeSpan(0, 9, 30, 0);
            Assert.AreEqual("9:30", TimeHelper.GetLongHourMinuteTimeString(timeValue, ci));
            timeValue = new TimeSpan(0, 9, 30, 30);
            Assert.AreEqual("9:31", TimeHelper.GetLongHourMinuteTimeString(timeValue, ci));
            timeValue = new TimeSpan(0, 9, 30, 30, 500);
            Assert.AreEqual("9:31", TimeHelper.GetLongHourMinuteTimeString(timeValue, ci));
            timeValue = new TimeSpan(0, 9, 30, 29, 500);
            Assert.AreEqual("9:30", TimeHelper.GetLongHourMinuteTimeString(timeValue, ci));
            timeValue = new TimeSpan(1, 12, 0, 0, 0);
            Assert.AreEqual("36:00", TimeHelper.GetLongHourMinuteTimeString(timeValue, ci));
        }
        [Test]
        public void SecondsCannotBeOverFiftyNine()
        {
            CultureInfo ci = CultureInfo.GetCultureInfo(1033);
            timeValue = new TimeSpan(0, 1, 0, 0).Subtract(new TimeSpan(1));
            Assert.AreEqual("1:00:00", TimeHelper.GetLongHourMinuteSecondTimeString(timeValue, ci));
        }
        [Test]
        public void MinutesCannotBeOverFiftyNine()
        {
            CultureInfo ci = CultureInfo.GetCultureInfo(1033);
            timeValue = new TimeSpan(0, 2, 0, 0).Subtract(new TimeSpan(1));
            Assert.AreEqual("2:00:00", TimeHelper.GetLongHourMinuteSecondTimeString(timeValue, ci));
        }

        [Test]
        public void VerifyTimeOfDayFromTimeSpan()
        {
            TimeSpan timeSpan = new TimeSpan(8,0,0);
            CultureInfo info = CultureInfo.GetCultureInfo("sv-SE");
            Thread.CurrentThread.CurrentCulture = info;
            string timeString = TimeHelper.TimeOfDayFromTimeSpan(timeSpan);
            Assert.AreEqual("08:00", timeString);

            info = CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = info;
            timeString = TimeHelper.TimeOfDayFromTimeSpan(timeSpan);
            Assert.AreEqual("8:00 AM", timeString);

            timeSpan = new TimeSpan(1,8, 0, 0);
            timeString = TimeHelper.TimeOfDayFromTimeSpan(timeSpan);
            Assert.AreEqual("8:00 AM +1", timeString);
            
        }

        [Test]
        public void ShouldHandleNegativeTimeCorrect()
        {
            var info = CultureInfo.GetCultureInfo("sv-SE");
            Thread.CurrentThread.CurrentCulture = info;
            TimeHelper.TryParseLongHourStringDefaultInterpretation("-08:1", TimeSpan.FromHours(24), out timeValue, TimeFormatsType.HoursMinutes, false);
            Assert.That(timeValue, Is.EqualTo(new TimeSpan(-8, -1, 0)));
        }

		[Test]
		public void ShouldExtractDaysFromTimeOfDayTimeSpan()
		{
			var time = TimeSpan.FromDays(3).Add(TimeSpan.FromHours(2));

			var result = TimeHelper.ParseTimeOfDayFromTimeSpan(time);

			result.TimeOfDay.Should().Be(TimeSpan.FromHours(2));
			result.Days.Should().Be(3);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test, SetCulture("en-US")]
        public void ShouldReturnFalseOnUnitedStatesCultureUsing24HourClock()
        {
            Assert.IsFalse(TimeHelper.CurrentCultureUsing24HourClock());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test, SetCulture("sv-SE")]
        public void ShouldReturnTrueOnSwedishCultureUsing24HourClock()
        {
            Assert.IsTrue(TimeHelper.CurrentCultureUsing24HourClock());
        }

		[Test]
		public void ShouldHandleSmallNegativeTimeCorrect()
		{
			var info = CultureInfo.GetCultureInfo("sv-SE");
			Thread.CurrentThread.CurrentCulture = info;
			TimeHelper.TryParseLongHourString("-0:30",  out timeValue, TimeFormatsType.HoursMinutes);
			Assert.That(timeValue, Is.EqualTo(new TimeSpan(0, -30, 0)));
		}

		[Test]
		public void ShouldHandleLittleBiggerNegativeTimeCorrect()
		{
			var info = CultureInfo.GetCultureInfo("sv-SE");
			Thread.CurrentThread.CurrentCulture = info;
			TimeHelper.TryParseLongHourString("-1:20", out timeValue, TimeFormatsType.HoursMinutes);
			Assert.That(timeValue, Is.EqualTo(new TimeSpan(-1, -20, 0)));
		}
	}
}
