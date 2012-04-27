﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Forecasting
{
    [TestFixture]
    public class OpenHourHelperTest
    {
        private TimePeriod period1;
        private TimePeriod period2;
        private TimeSpan start;
        private TimeSpan end;
        private string inCorrectStart = "0433:00 - 02:00";
        private string inCorrectEnd = "04:00 - 02:900";
        private string openHour = "12:00 - 17:01";
        private int _resolution = 15;

        [SetUp]
        public void Setup()
        {
            period1 = new TimePeriod(new TimeSpan(2, 0, 0), new TimeSpan(16, 0, 0));
            period2 = new TimePeriod(new TimeSpan(18, 0, 0), new TimeSpan(23, 0, 0));
        }

        [Test, SetCulture("sv-SE")]
        public void CanReturnCorrectOpenHourString()
        {
            period1 = new TimePeriod(new TimeSpan(2, 0, 0), new TimeSpan(16, 0, 0));
            period2 = new TimePeriod(new TimeSpan(18, 0, 0), new TimeSpan(23, 0, 0));
            List<TimePeriod> periods = new List<TimePeriod>();
            periods.Add(period1);
            periods.Add(period2);
            string expectedString = "02:00 - 16:00; 18:00 - 23:00";
            Assert.AreEqual(expectedString, OpenHourHelper.OpenHoursToString(periods));
        }

        [Test]
        public void CanCheckOpenHourString()
        {
            string correct = "04:00 - 02:00";

            Assert.IsFalse(OpenHourHelper.CheckOpenHourString(inCorrectStart, out start, out end));
            Assert.IsFalse(OpenHourHelper.CheckOpenHourString(inCorrectEnd, out start, out end));
            Assert.IsTrue(OpenHourHelper.CheckOpenHourString(correct, out start, out end));
        }
        [Test]
        public void CanGetOpenHourPeriod()
        {
            start = new TimeSpan(4, 0, 0);
            TimeSpan endNextDay = new TimeSpan(1, 2, 0, 0);
            end = new TimeSpan(16, 0, 0);
            period1 = new TimePeriod(start, endNextDay);
            period2 = new TimePeriod(start, end);
            string correct = "04:00 - 02:00";
            string correct2 = "04:00 - 16:00";

            Assert.AreEqual(period1, OpenHourHelper.OpenHourPeriod(correct, TimeSpan.Zero));
            Assert.AreEqual(period2, OpenHourHelper.OpenHourPeriod(correct2, TimeSpan.Zero));
        }

        [Test]
        public void CanGetOpenHourPeriodWithMidnightBreakOffset()
        {
            start = new TimeSpan(4, 0, 0);
            TimeSpan endNextDay = new TimeSpan(1, 2, 0, 0);
            end = new TimeSpan(1, 4, 0, 0);
            period1 = new TimePeriod(start, endNextDay);
            period2 = new TimePeriod(start.Add(TimeSpan.FromHours(20)), end);
            string correct = "04:00 - 02:00";
            string correct2 = "00:00 - 04:00";

            Assert.AreEqual(period1, OpenHourHelper.OpenHourPeriod(correct, TimeSpan.FromHours(4)));
            Assert.AreEqual(period2, OpenHourHelper.OpenHourPeriod(correct2, TimeSpan.FromHours(4)));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CanReturnErrorWhenStringFormatIsWrongEnd()
        {
            OpenHourHelper.OpenHourPeriod(inCorrectEnd, TimeSpan.Zero);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CanReturnErrorWhenStringFormatIsWrongStart()
        {
            OpenHourHelper.OpenHourPeriod(inCorrectStart, TimeSpan.Zero);
        }

        [Test]
        public void VerifyOpenHourFromString()
        {
            TimePeriod expectedPeriod = new TimePeriod("12:00 - 17:00");
            Assert.AreEqual(expectedPeriod, OpenHourHelper.OpenHourFromString(openHour, _resolution, TimeSpan.Zero));
        }

        [Test]
        public void VerifyOpenHourFromStringNextDay()
        {
            Assert.AreEqual(new TimePeriod(TimeSpan.FromHours(12), TimeSpan.FromHours(41)),
                            OpenHourHelper.OpenHourFromString("12:00 - 17:00 +1", _resolution, TimeSpan.Zero));
        }

        /// <summary>
        /// Verifies the open hour from invalid string with single value.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-07
        /// </remarks>
        [Test]
        public void VerifyOpenHourFromInvalidStringWithSingleValue()
        {
            Assert.IsFalse(OpenHourHelper.CheckOpenHourString("12:00", out start, out end));
            Assert.AreEqual(TimeSpan.Zero, start);
            Assert.AreEqual(TimeSpan.Zero, end);
        }
    }
}
