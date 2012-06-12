using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Time
{
    /// <summary>
    /// Tests TimePeriod struct
    /// </summary>
    [TestFixture]
    public class TimePeriodTest
    {
        private TimePeriod _per;
        private TimeSpan _start;
        private TimeSpan _end;

        [SetUp]
        public void Setup()
        {
            _start = TimeFactory.CreateTimeSpan(10);
            _end = TimeFactory.CreateTimeSpan(12);
            _per = new TimePeriod(_start, _end);
        }

        /// <summary>
        /// Verifies that start time and end time are set using create.
        /// </summary>
        [Test]
        public void VerifyStartTimeAndEndTimeAreSetUsingCreate()
        {
            Assert.AreEqual(_start, _per.StartTime);
            Assert.AreEqual(_end, _per.EndTime);
        }

        [Test]
        public void VerifyHourMinuteConstructor()
        {
            _per = new TimePeriod(10,0,12,45);
            Assert.AreEqual(_per, new TimePeriod(new TimeSpan(10,0,0), new TimeSpan(12,45,0)));
        }

        /// <summary>
        /// Verifies the equals method works.
        /// </summary>
        [Test]
        public void VerifyEqualsWork()
        {
            Assert.IsTrue(_per.Equals(TimeFactory.CreateTimePeriod(10, 12)));
            Assert.IsFalse(
                _per.Equals(
                    new TimePeriod(TimeFactory.CreateTimeSpan(10),
                                   TimeFactory.CreateTimeSpan(12).Add(new TimeSpan(1)))));
            Assert.IsFalse(new TimePeriod().Equals(null));
            Assert.AreEqual(_per, (object) _per);
            Assert.IsFalse(new TimeSpan().Equals(3));
        }

        /// <summary>
        /// Verifies that overloaded operators work.
        /// </summary>
        [Test]
        public void VerifyOverloadedOperatorsWork()
        {
            int startHour = 10;
            int endHour = 12;
            TimePeriod per2 = new TimePeriod();
            Assert.IsTrue(_per == TimeFactory.CreateTimePeriod(startHour, endHour));
            Assert.IsTrue(_per != per2);
        }

        /// <summary>
        /// Verifies gethashcode works.
        /// </summary>
        [Test]
        public void VerifyGetHashCodeWorks()
        {
            IDictionary<TimePeriod, int> dic = new Dictionary<TimePeriod, int>();
            dic[_per] = 5;
            Assert.AreEqual(5, dic[_per]);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyStartTimeMustOccurBeforeEndTime()
        {
            new TimePeriod(new TimeSpan(3), new TimeSpan(2));
        }

        /// <summary>
        /// Checks if ToString method formats TimePeriod correctly.
        /// </summary>
        [Test]
        public void ToStringFormatsCorrectly()
        {
            TimeSpan start = new TimeSpan(2, 0, 0);
            TimeSpan end = new TimeSpan(12, 0, 0);
            TimePeriod per = new TimePeriod(start, end);
            DateTime dtStart = new DateTime(start.Ticks);
            DateTime dtEnd = new DateTime(end.Ticks);
            string expectation = string.Concat(dtStart.ToLongTimeString(), "-", dtEnd.ToLongTimeString());
            Assert.AreEqual(expectation, per.ToString());
        }
        /// <summary>
        /// Checks if ToShortTimeString method formats TimePeriod correctly.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.TimePeriod.ToShortTimeString"), Test]
        public void ToShortTimeStringFormatsCorrectly()
        {
            TimePeriod per = new TimePeriod(_start, _end);
            DateTime dtStart = new DateTime(_start.Ticks);
            DateTime dtEnd = new DateTime(_end.Ticks);
            string expectation = string.Concat(dtStart.ToShortTimeString(), " - ", dtEnd.ToShortTimeString());
            Assert.AreEqual(expectation, per.ToShortTimeString());
        }

        [Test]
        public void VerifyContainsPart()
        {
            //This is 13:00 - 03:00 (day after)
            TimeSpan start = new TimeSpan(0,13, 0,0);
            TimeSpan end = new TimeSpan(0,22, 0, 0);

            TimePeriod target = new TimePeriod(start, end);

            TimeSpan beforeStartEarlier = new TimeSpan(12,0,0);
            TimeSpan beforeStartLater = new TimeSpan(12,30,0);
           
            TimeSpan inPeriodEarlier = new TimeSpan(14,0,0);
            TimeSpan inPeriodLater = new TimeSpan(15,0,0);
            
            TimeSpan afterEndEarlier = new TimeSpan(1,4,0,0);
            TimeSpan afterEndLater = new TimeSpan(1,9,0,0);


            TimePeriod containsPeriod = new TimePeriod(inPeriodEarlier, inPeriodLater);
            TimePeriod startEarlierFinishInPeriod = new TimePeriod(beforeStartEarlier, inPeriodLater);
            TimePeriod startInFinishLaterPeriod = new TimePeriod(inPeriodEarlier, afterEndLater);
            TimePeriod startEarlierFinishLaterPeriod = new TimePeriod(beforeStartLater, afterEndLater);
            TimePeriod startEarlierFinishAtStartPeriod = new TimePeriod(beforeStartLater, start);
            TimePeriod startAtEndFinishLaterPeriod = new TimePeriod(end, afterEndEarlier);
            TimePeriod startEarlierFinishAtEndPeriod = new TimePeriod(beforeStartLater, end);
            TimePeriod startAtStartFinishLaterPeriod = new TimePeriod(start, afterEndLater);
            TimePeriod startLaterFinishLaterPeriod = new TimePeriod(afterEndEarlier, afterEndLater);
            TimePeriod startEarlierFinishEarlierPeriod =
                new TimePeriod(beforeStartEarlier, beforeStartLater);
            TimePeriod equalsPeriod = new TimePeriod(start, end);

            Assert.IsTrue(target.ContainsPart(startEarlierFinishInPeriod));
            Assert.IsTrue(target.ContainsPart(startInFinishLaterPeriod));
            Assert.IsTrue(target.ContainsPart(containsPeriod));
            Assert.IsTrue(target.ContainsPart(equalsPeriod));
            Assert.IsTrue(target.ContainsPart(startEarlierFinishLaterPeriod));
            Assert.IsTrue(target.ContainsPart(startEarlierFinishAtStartPeriod));
            Assert.IsTrue(target.ContainsPart(startAtEndFinishLaterPeriod));
            Assert.IsTrue(target.ContainsPart(startEarlierFinishAtEndPeriod));
            Assert.IsTrue(target.ContainsPart(startAtStartFinishLaterPeriod));
            Assert.IsFalse(target.ContainsPart(startEarlierFinishEarlierPeriod));
            Assert.IsFalse(target.ContainsPart(startLaterFinishLaterPeriod));

            
        }

        [Test]
        public void VerifyContainsTimeSpan()
        {
            TimeSpan start = new TimeSpan(13, 0, 0);
            TimeSpan end = new TimeSpan(22, 0, 0);
            TimePeriod period = new TimePeriod(start, end);

            Assert.IsFalse(period.Contains(new TimeSpan(12, 59, 59)));
            Assert.IsTrue(period.Contains(new TimeSpan(13, 0, 0)));
            Assert.IsTrue(period.Contains(new TimeSpan(21, 59, 59)));
            Assert.IsFalse(period.Contains(new TimeSpan(22, 0, 0)));
        }

        [Test]
        public void VerifyIntersects()
        {
            TimePeriod t1 = new TimePeriod(new TimeSpan(12, 0, 0), new TimeSpan(13, 0, 0));
            TimePeriod t2 = new TimePeriod(new TimeSpan(13, 0, 0), new TimeSpan(14, 0, 0));
            TimePeriod t3 = new TimePeriod(new TimeSpan(14, 0, 0), new TimeSpan(15, 0, 0));
            Assert.IsFalse(t1.Intersect(t2));
            Assert.IsFalse(t3.Intersect(t2));
            Assert.IsFalse(t2.Intersect(t1));
            Assert.IsFalse(t2.Intersect(t3));

            t1 = new TimePeriod(new TimeSpan(12, 0, 0), new TimeSpan(13, 0, 1));
            t2 = new TimePeriod(new TimeSpan(13, 0, 0), new TimeSpan(14, 0, 1));
            Assert.IsTrue(t1.Intersect(t2));
            Assert.IsTrue(t3.Intersect(t2));
            Assert.IsTrue(t2.Intersect(t1));
            Assert.IsTrue(t2.Intersect(t3));

            t1 = new TimePeriod(new TimeSpan(12, 0, 0), new TimeSpan(13, 0, 0));
            t2 = new TimePeriod(new TimeSpan(12, 0, 0), new TimeSpan(13, 0, 0));
            Assert.IsTrue(t1.Intersect(t2));

            t1 = new TimePeriod(new TimeSpan(12, 0, 0), new TimeSpan(16, 0, 0));
            t2 = new TimePeriod(new TimeSpan(13, 0, 0), new TimeSpan(14, 0, 0));
            Assert.IsTrue(t1.Intersect(t2));
            Assert.IsTrue(t2.Intersect(t1));
        }

        [Test]
        public void VerifyIntersection()
        {
            TimePeriod t1 = new TimePeriod(new TimeSpan(12, 0, 0), new TimeSpan(13, 0, 0));
            TimePeriod t2 = new TimePeriod(new TimeSpan(13, 0, 0), new TimeSpan(14, 0, 0));
            Assert.IsNull(t1.Intersection(t2));
            Assert.IsNull(t2.Intersection(t1));

            TimePeriod t = new TimePeriod(new TimeSpan(12, 59, 59), new TimeSpan(13, 0, 1));
            Assert.AreEqual(new TimeSpan(12, 59, 59), t1.Intersection(t).Value.StartTime);
            Assert.AreEqual(new TimeSpan(13, 0, 0), t1.Intersection(t).Value.EndTime);
            Assert.AreEqual(new TimeSpan(13, 0, 0), t2.Intersection(t).Value.StartTime);
            Assert.AreEqual(new TimeSpan(13, 0, 1), t2.Intersection(t).Value.EndTime);

            t = new TimePeriod(new TimeSpan(12, 15, 0), new TimeSpan(12, 45, 0));
            Assert.AreEqual(new TimeSpan(12, 15, 0), t1.Intersection(t).Value.StartTime);
            Assert.AreEqual(new TimeSpan(12, 45, 0), t1.Intersection(t).Value.EndTime);
            Assert.AreEqual(new TimeSpan(12, 15, 0), t.Intersection(t1).Value.StartTime);
            Assert.AreEqual(new TimeSpan(12, 45, 0), t.Intersection(t1).Value.EndTime);

            t1 = new TimePeriod(new TimeSpan(12, 0, 0), new TimeSpan(13, 0, 0));
            t2 = new TimePeriod(new TimeSpan(12, 0, 0), new TimeSpan(13, 0, 0));
            Assert.AreEqual(1, t1.Intersection(t2).Value.SpanningTime().TotalHours);

            t1 = new TimePeriod(new TimeSpan(12, 0, 0), new TimeSpan(16, 0, 0));
            t2 = new TimePeriod(new TimeSpan(13, 0, 0), new TimeSpan(14, 0, 0));
            Assert.AreEqual(1, t1.Intersection(t2).Value.SpanningTime().TotalHours);
            Assert.AreEqual(1, t2.Intersection(t1).Value.SpanningTime().TotalHours);
        }

        [Test]
        public void VerifyContainsTimePeriod()
        {
            TimeSpan start = new TimeSpan(0, 13, 0, 0);
            TimeSpan end = new TimeSpan(0, 22, 0, 0);

            //TimePeriod period = new TimePeriod(2006, 1, 1, 2007, 1, 1);
            TimePeriod period = new TimePeriod(start, end);

            Assert.IsFalse(period.Contains(new TimePeriod(new TimeSpan(0, 12, 25, 0), new TimeSpan(0, 12, 45, 0))));//new DateTime(2005, 12, 31, 23, 59, 59, DateTimeKind.Utc)));
            Assert.IsTrue(period.Contains(new TimePeriod(new TimeSpan(0, 14, 25, 0), new TimeSpan(0, 16, 45, 0))));
            Assert.IsTrue(period.Contains(new TimePeriod(new TimeSpan(0, 21, 19, 0), new TimeSpan(0, 21, 34, 0))));
            Assert.IsFalse(period.Contains(new TimePeriod(new TimeSpan(0, 21, 19, 0), new TimeSpan(0, 24, 34, 0))));
        }

     
        [Test]
        public void VerifyTryParse()
        {
            Assert.IsFalse(TimePeriod.TryParse("", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8:00-17:00", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8-17", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8-2", out _per));;
        }

        /// <summary>
        /// Verifies the try parse with invalid text.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-13
        /// </remarks>
        [Test]
        public void VerifyTryParseWithInvalidText()
        {
            Assert.IsFalse(TimePeriod.TryParse("8:00-17:00-18:00-19:00", out _per));
            Assert.IsFalse(TimePeriod.TryParse("asdf-17", out _per));
            Assert.IsFalse(TimePeriod.TryParse("8-asdf", out _per));
        }

        [Test]
        public void TestConstructor()
        {
            _per = new TimePeriod("8:30-17:25");
            Assert.IsNotNull(_per);

            Assert.AreEqual(8, _per.StartTime.Hours);
            Assert.AreEqual(30, _per.StartTime.Minutes);

            Assert.AreEqual(17, _per.EndTime.Hours);
            Assert.AreEqual(25, _per.EndTime.Minutes);
        }

        /// <summary>
        /// Tests an invalid value in constructor.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-13
        /// </remarks>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidValueInConstructor()
        {
            _per = new TimePeriod("8:30/17:25");
        }

        [Test, SetCulture("en-GB")]
        public void VerifyTryParseWithEnglish()
        {
            Assert.IsFalse(TimePeriod.TryParse("", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8:00-17:00", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8-17", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8:-17:", out _per));

            Assert.IsTrue(TimePeriod.TryParse("8:00 am - 5:00 pm", out _per));
        }

        [Test]
        public void VerifyTryParseWithHungarian()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(1038);
            Assert.IsFalse(TimePeriod.TryParse("", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8:00-17:00", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8-17", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8:-17:", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8:00 am - 5:00 pm", out _per));
        }

        [Test, SetCulture("ar-SA")]
        public void VerifyTryParseWithArabicSaudi()
        {
            Assert.IsFalse(TimePeriod.TryParse("", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8:00-17:00", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8-17", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8:-17:", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8:00 am - 5:00 pm", out _per));
        }

        [Test]
        public void VerifyTryParseWithChineseProc()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(2052);
            Assert.IsFalse(TimePeriod.TryParse("", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8:00-17:00", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8-17", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8:-17:", out _per));
            Assert.IsTrue(TimePeriod.TryParse("8:00 am - 5:00 pm", out _per));
        }

        [Test]
        public void VerifyTryParseWithAllCultures()
        {
            foreach (CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                Thread.CurrentThread.CurrentCulture = cultureInfo;

                tryParseAndCheckResult("08:00 - 17:00", 8, 17);
                tryParseAndCheckResult("8-17", 8, 17);
                tryParseAndCheckResult("13-23", 13, 23);
            }
        }

        [Test]
        public void VerifyTryParseZeroToZeroWithAllCultures()
        {
            foreach (CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                Thread.CurrentThread.CurrentCulture = cultureInfo;

                tryParseAndCheckResult("00:00 - 00:00", 0, 24);
                tryParseAndCheckResult("0-0", 0, 24);
                tryParseAndCheckResult("13-13", 13, 13);
            }
        }

        [Test]
        public void VerifySpannedTimeActuallyWorks()
        {
            Assert.AreEqual(_per.EndTime.Subtract(_per.StartTime).Ticks,_per.SpanningTime().Ticks);
        }

		 [Test]
		 public void VerifyOperator()
		 {
		 	var t1 = new TimePeriod(new TimeSpan(20, 0, 0), new TimeSpan(22, 0, 0));
			var t2 = new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(23, 0, 0));
			 Assert.IsTrue(t1 > t2);
			 Assert.IsFalse(t1 < t2);
		 }


        private void tryParseAndCheckResult(string stringToParse, int expectedStartHours, int expectedEndHours)
        {
            TimePeriod tp;
            Assert.IsTrue(TimePeriod.TryParse(stringToParse, out tp));
            Assert.AreEqual(expectedStartHours, (int)tp.StartTime.TotalHours);
            Assert.AreEqual(expectedEndHours, (int)tp.EndTime.TotalHours);
        }

        
      


        [Test]
        public void VerifyCanCombineNonOverlappingAndOverlappingPeriods()
        {
            TimePeriod t3 = new TimePeriod(TimeSpan.FromHours(18), TimeSpan.FromHours(20));
            TimePeriod t1 = new TimePeriod(TimeSpan.FromHours(4), TimeSpan.FromHours(14));
            TimePeriod t2 = new TimePeriod(TimeSpan.FromHours(12), TimeSpan.FromHours(16));
            TimePeriod t4 = new TimePeriod(TimeSpan.FromHours(6), new TimeSpan(1, 1, 0, 0));
            IList<TimePeriod> periods = new List<TimePeriod>();

            periods.Add(t1);
            periods.Add(t2);
            periods.Add(t3);

            IList<TimePeriod> combinedPeriods = TimePeriod.Combine(periods);
            Assert.AreEqual(2, combinedPeriods.Count, "Verify Length");
            Assert.AreEqual(new TimePeriod(TimeSpan.FromHours(4), TimeSpan.FromHours(16)),combinedPeriods[0],"Verify StartTime and EndTime");
            Assert.AreEqual(t3, combinedPeriods[1],"Verify NonOverlapping");

            periods.Add(t4);
            combinedPeriods=TimePeriod.Combine(periods);
            Assert.AreEqual(1, combinedPeriods.Count, "Verify Length");
            Assert.AreEqual(new TimePeriod(TimeSpan.FromHours(4), new TimeSpan(1, 1, 0, 0)), combinedPeriods[0], "Verify Overlapping TimePeriod");

        }

        [Test]
        public void VerifyAlign()
        {
            TimePeriod testTimePeriod = new TimePeriod(7, 7, 16, 55);
            TimePeriod alignedTimePeriod = TimePeriod.AlignToMinutes(testTimePeriod, 15);
            Assert.AreEqual(0, alignedTimePeriod.StartTime.Minutes);
            Assert.AreEqual(17, alignedTimePeriod.EndTime.Hours);

            testTimePeriod = new TimePeriod(0, 0, 17, 0);
            alignedTimePeriod = TimePeriod.AlignToMinutes(testTimePeriod, 15);
            Assert.AreEqual(0, alignedTimePeriod.StartTime.Minutes);
            Assert.AreEqual(17, alignedTimePeriod.EndTime.Hours);
            Assert.AreEqual(0, alignedTimePeriod.EndTime.Minutes);
            //Console.Out.WriteLine(alignedTimePeriod.ToShortTimeString());
        }
        

    }
}
   