using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Time
{
    /// <summary>
    /// Tests DateTimePeriod struct
    /// </summary>
    [TestFixture]
    public class DateTimePeriodTest
    {
        private readonly DateTime _start = new DateTime(2007, 06, 01, 12, 31, 0, DateTimeKind.Utc);
        private readonly DateTime _end = new DateTime(2008, 02, 28, 0, 0, 0, DateTimeKind.Utc);
        private DateTimePeriod _period;
        private CccTimeZoneInfo _cccTimeZoneInfo;

        [SetUp]
        public void TestSetup()
        {
            _period = new DateTimePeriod(_start, _end);
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");
            _cccTimeZoneInfo = new CccTimeZoneInfo(timeZone);
        }

        /// <summary>
        /// Verifies the can get time period.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-18
        /// </remarks>
        [Test]
        public void VerifyCanGetTimePeriod()
        {
            _period = new DateTimePeriod(
                new DateTime(2008,2,1,2,0,0, DateTimeKind.Utc),
                new DateTime(2008,2,2,6,0,0, DateTimeKind.Utc));

            Assert.AreEqual(
                new TimePeriod(
                    TimeSpan.FromHours(2),
                    TimeSpan.FromDays(1).Add(
                        TimeSpan.FromHours(6))),
                _period.TimePeriod(new CccTimeZoneInfo(TimeZoneInfo.Utc)));
        }

        [Test]
        public void VerifyTwoNullsHaveNullMaxPeriod()
        {
            Assert.IsNull(DateTimePeriod.MaximumPeriod(null, null));
        }

        /// <summary>
        /// Verifies the can get time period with time zone.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-18
        /// </remarks>
        [Test]
        public void VerifyCanGetTimePeriodWithTimeZone()
        {
            _period = new DateTimePeriod(
                new DateTime(2008, 2, 1, 2, 0, 0, DateTimeKind.Utc),
                new DateTime(2008, 2, 2, 6, 0, 0, DateTimeKind.Utc));

            Assert.AreEqual(
                new TimePeriod(
                    TimeSpan.FromHours(3),
                    TimeSpan.FromDays(1).Add(
                        TimeSpan.FromHours(7))),
                _period.TimePeriod(new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"))));
        }

        /// <summary>
        /// Verifies the can get time period with time zone.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-18
        /// </remarks>
        [Test]
        public void VerifyCanGetLocalTimePeriod()
        {
            _period = new DateTimePeriod(
                new DateTime(2008, 2, 1, 2, 0, 0, DateTimeKind.Utc),
                new DateTime(2008, 2, 2, 6, 0, 0, DateTimeKind.Utc));

            Assert.AreEqual(
                new TimePeriod(
                    TimeSpan.FromHours(3),
                    TimeSpan.FromDays(1).Add(
                        TimeSpan.FromHours(7))),
                _period.TimePeriodLocal());
        }

        /// <summary>
        /// Verifies that start datetime and end datetime are set using create.
        /// </summary>
        [Test]
        public void VerifyStartDateTimeAndEndDateTimeAreSetUsingCreate()
        {
            Assert.AreEqual(_start, _period.StartDateTime);
            Assert.AreEqual(_end, _period.EndDateTime);
        }

        /// <summary>
        /// Verifies the equals method works.
        /// </summary>
        [Test]
        public void VerifyEqualsWork()
        {
            DateTimePeriod per2 = new DateTimePeriod(_start, _end);
            DateTimePeriod per3 = new DateTimePeriod(_start, _start);

            Assert.IsTrue(_period.Equals(per2));
            Object test = per2;
            Assert.IsTrue(_period.Equals(test));
            Assert.IsFalse(_period.Equals(per3));
            Assert.IsFalse(_period.Equals(null));
        }

        /// <summary>
        /// Verifies that overloaded operators work.
        /// </summary>
        [Test]
        public void VerifyOverloadedOperatorsWork()
        {
            DateTimePeriod per2 = new DateTimePeriod(_start, _end);
            DateTimePeriod per3 = new DateTimePeriod(_start, _start);

            Assert.IsTrue(_period == per2);
            Assert.IsTrue(_period != per3);
        }

        /// <summary>
        /// Verifies that comparable interface works.
        /// </summary>
        [Test]
        public void VerifyComparableInterfaceMethodsWork()
        {
            DateTimePeriod per2 = _period.MovePeriod(new TimeSpan(0, -30, 0));
            DateTimePeriod per3 = new DateTimePeriod(_start, _start);
            DateTimePeriod per4 = _period.MovePeriod(new TimeSpan(0, 30, 0));

            Assert.AreEqual(1,_period.CompareTo(per2));
            Assert.AreEqual(1, _period.CompareTo(per3));
            Assert.AreEqual(0, _period.CompareTo(_period));
            Assert.AreEqual(-1,_period.CompareTo(per4));
        }

        /// <summary>
        /// Verifies gethashcode works.
        /// </summary>
        [Test]
        public void VerifyGetHashCodeWorks()
        {
            DateTimePeriod per2 = _period;
            Assert.AreEqual(_period.GetHashCode(), per2.GetHashCode());
        }

        /// <summary>
        /// Verifies that the start DateTime is not later than end DateTime
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void VerifyStartDateTimeEqualOrBeforeEndDateTime()
        {
            _period = new DateTimePeriod(_end, _start);
        }

        /// <summary>
        /// Verifies that the start DateTime is not later than end DateTime
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void VerifyStartDateTimeEqualOrBeforeEndDateTimeWhenUsingSixParameters()
        {
            _period = new DateTimePeriod(2001, 1, 1, 2000, 1, 1);
        }

        /// <summary>
        /// Verifies the MovePeriod function
        /// </summary>
        [Test]
        public void VerifyMovePeriod()
        {
            TimeSpan span = new TimeSpan(-1, 15, 0);
            DateTimePeriod per2 = new DateTimePeriod(_start.Add(span), _end.Add(span));
            Assert.AreEqual(_period.MovePeriod(span), per2);
            per2 = new DateTimePeriod(_start.Add(span), _end.Subtract(span));
            Assert.AreNotEqual(_period.MovePeriod(span), per2);

            span = new TimeSpan(2, 0, 0);
            per2 =
                new DateTimePeriod(new DateTime(2007, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 1, 13, 0, 0, DateTimeKind.Utc));
            per2 = per2.MovePeriod(span);
            Assert.AreEqual(per2,
                            new DateTimePeriod(new DateTime(2007, 1, 1, 14, 0, 0, DateTimeKind.Utc),
                                               new DateTime(2007, 1, 1, 15, 0, 0, DateTimeKind.Utc)));

            span = new TimeSpan(-2, 0, 0);
            per2 =
                new DateTimePeriod(new DateTime(2007, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 1, 13, 0, 0, DateTimeKind.Utc));
            per2 = per2.MovePeriod(span);
            Assert.AreEqual(per2,
                            new DateTimePeriod(new DateTime(2007, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                                               new DateTime(2007, 1, 1, 11, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        public void VerifyContainsDate()
        {
            _period = new DateTimePeriod(2006, 1, 1, 2007, 1, 1);
            Assert.IsFalse(_period.Contains(new DateTime(2005, 12, 31, 23, 59, 59, DateTimeKind.Utc)));
            Assert.IsTrue(_period.Contains(new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
            Assert.IsTrue(_period.Contains(new DateTime(2006, 11, 1, 0, 0, 0, DateTimeKind.Utc)));
            Assert.IsFalse(_period.Contains(new DateTime(2007, 1, 1, 0, 0, 1, DateTimeKind.Utc)));
        }

        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void VerifyNotOverlapWorksWithSharedPeriod()
        {
            DateTimePeriod period1 = new DateTimePeriod(2006, 1, 1, 2006, 12, 31);
            DateTimePeriod period2 = new DateTimePeriod(2007, 1, 1, 2008, 1, 1);
            _period = period1.SharedPeriod(period2);
        }

        [Test]
        public void VerifyPeriodTimeSpanWorks()
        {
            _period = new DateTimePeriod(2006, 1, 1, 2006, 1, 10);
            Assert.AreEqual(TimeSpan.FromDays(9), _period.ElapsedTime());
        }

        [Test]
        public void VerifySharedPeriodWorks()
        {
            _period = new DateTimePeriod(2006, 1, 1, 2006, 12, 31);
            DateTimePeriod period2 = new DateTimePeriod(2005, 1, 1, 2008, 1, 1);
            DateTimePeriod period3 = new DateTimePeriod(2006, 6, 1, 2006, 7, 1);
            DateTimePeriod period4 = new DateTimePeriod(2005, 1, 1, 2006, 7, 1);
            DateTimePeriod period5 = new DateTimePeriod(2006, 7, 1, 2008, 1, 1);
            Assert.AreEqual(_period, period2.SharedPeriod(_period));
            Assert.AreEqual(period3, period3.SharedPeriod(_period));
            Assert.AreEqual(new DateTimePeriod(2006, 1, 1, 2006, 7, 1), period4.SharedPeriod(_period));
            Assert.AreEqual(new DateTimePeriod(2006, 7, 1, 2006, 12, 31), period5.SharedPeriod(_period));
            Assert.AreEqual(new DateTimePeriod(2006, 7, 1, 2006, 12, 31), _period.SharedPeriod(period5));
        }

        [Test]
        public void VerifyContainsPeriod()
        {
            DateTime beforeStartEarlierDate = new DateTime(2006, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime beforeStartLaterDate = new DateTime(2006, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime inPeriodEarlierDate = new DateTime(2007, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime inPeriodLaterDate = new DateTime(2008, 2, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime afterEndEarlierDate = new DateTime(2008, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime afterEndLaterDate = new DateTime(2008, 7, 1, 0, 0, 0, DateTimeKind.Utc);

            DateTimePeriod containsPeriod = new DateTimePeriod(inPeriodEarlierDate, inPeriodLaterDate);
            DateTimePeriod startEarlierFinishInPeriod = new DateTimePeriod(beforeStartEarlierDate, inPeriodLaterDate);
            DateTimePeriod startInFinishLaterPeriod = new DateTimePeriod(inPeriodEarlierDate, afterEndLaterDate);
            DateTimePeriod startEarlierFinishLaterPeriod = new DateTimePeriod(beforeStartLaterDate, afterEndLaterDate);
            DateTimePeriod startEarlierFinishAtStartPeriod = new DateTimePeriod(beforeStartLaterDate, _start);
            DateTimePeriod startAtEndFinishLaterPeriod = new DateTimePeriod(_end, afterEndEarlierDate);
            DateTimePeriod startEarlierFinishAtEndPeriod = new DateTimePeriod(beforeStartLaterDate, _end);
            DateTimePeriod startAtStartFinishLaterPeriod = new DateTimePeriod(_start, afterEndLaterDate);
            DateTimePeriod startLaterFinishLaterPeriod = new DateTimePeriod(afterEndEarlierDate, afterEndLaterDate);
            DateTimePeriod startEarlierFinishEarlierPeriod =
                new DateTimePeriod(beforeStartEarlierDate, beforeStartLaterDate);
            DateTimePeriod equalsPeriod = new DateTimePeriod(_start, _end);

            Assert.IsFalse(_period.Contains(startEarlierFinishInPeriod));
            Assert.IsFalse(_period.Contains(startInFinishLaterPeriod));
            Assert.IsTrue(_period.Contains(containsPeriod));
            Assert.IsTrue(_period.Contains(equalsPeriod));
            Assert.IsFalse(_period.Contains(startEarlierFinishLaterPeriod));
            Assert.IsFalse(_period.Contains(startEarlierFinishAtStartPeriod));
            Assert.IsFalse(_period.Contains(startAtEndFinishLaterPeriod));
            Assert.IsFalse(_period.Contains(startEarlierFinishAtEndPeriod));
            Assert.IsFalse(_period.Contains(startAtStartFinishLaterPeriod));
            Assert.IsFalse(_period.Contains(startEarlierFinishEarlierPeriod));
            Assert.IsFalse(_period.Contains(startLaterFinishLaterPeriod));
        }

        [Test]
        public void VerifyIntersects()
        {
            _period = new DateTimePeriod(new DateTime(2007, 1, 1, 12, 0, 0, DateTimeKind.Utc), new DateTime(2007, 1, 1, 13, 0, 0, DateTimeKind.Utc));
            DateTimePeriod t2 = new DateTimePeriod(new DateTime(2007, 1, 1, 13, 0, 0, DateTimeKind.Utc), new DateTime(2007, 1, 1, 14, 0, 0, DateTimeKind.Utc));
            DateTimePeriod t3 = new DateTimePeriod(new DateTime(2007, 1, 1, 14, 0, 0, DateTimeKind.Utc), new DateTime(2007, 1, 1, 15, 0, 0, DateTimeKind.Utc));
            Assert.IsFalse(_period.Intersect(t2));
            Assert.IsFalse(t3.Intersect(t2));
            Assert.IsFalse(t2.Intersect(_period));
            Assert.IsFalse(t2.Intersect(t3));

            _period = new DateTimePeriod(new DateTime(2007, 1, 1, 12, 0, 0, DateTimeKind.Utc), new DateTime(2007, 1, 1, 13, 0, 1, DateTimeKind.Utc));
            t2 = new DateTimePeriod(new DateTime(2007, 1, 1, 13, 0, 0, DateTimeKind.Utc), new DateTime(2007, 1, 1, 14, 0, 1, DateTimeKind.Utc));
            Assert.IsTrue(_period.Intersect(t2));
            Assert.IsTrue(t3.Intersect(t2));
            Assert.IsTrue(t2.Intersect(_period));
            Assert.IsTrue(t2.Intersect(t3));
        }

        [Test]
        public void VerifyIntersectsCanBeReplacedByContainsPart()
        {
            _period = new DateTimePeriod(new DateTime(2007, 1, 1, 12, 0, 0, DateTimeKind.Utc), new DateTime(2007, 1, 1, 13, 0, 0, DateTimeKind.Utc));
            DateTimePeriod t2 = new DateTimePeriod(new DateTime(2007, 1, 1, 13, 0, 0, DateTimeKind.Utc), new DateTime(2007, 1, 1, 14, 0, 0, DateTimeKind.Utc));
            DateTimePeriod t3 = new DateTimePeriod(new DateTime(2007, 1, 1, 14, 0, 0, DateTimeKind.Utc), new DateTime(2007, 1, 1, 15, 0, 0, DateTimeKind.Utc));
            Assert.IsTrue(_period.Intersect(t2) == _period.ContainsPart(t2, false));
            Assert.IsTrue(t3.Intersect(t2) == t3.ContainsPart(t2, false));
            Assert.IsTrue(t2.Intersect(_period) == t2.ContainsPart(_period, false));
            Assert.IsTrue(t2.Intersect(t3) == t2.ContainsPart(t3, false));

            _period = new DateTimePeriod(new DateTime(2007, 1, 1, 12, 0, 0, DateTimeKind.Utc), new DateTime(2007, 1, 1, 13, 0, 1, DateTimeKind.Utc));
            t2 = new DateTimePeriod(new DateTime(2007, 1, 1, 13, 0, 0, DateTimeKind.Utc), new DateTime(2007, 1, 1, 14, 0, 1, DateTimeKind.Utc));
            Assert.IsTrue(_period.Intersect(t2) == _period.ContainsPart(t2, false));
            Assert.IsTrue(t3.Intersect(t2) == t3.ContainsPart(t2, false));
            Assert.IsTrue(t2.Intersect(_period) == t2.ContainsPart(_period, false));
            Assert.IsTrue(t2.Intersect(t3) == t2.ContainsPart(t3, false));
        }

        [Test]
        public void VerifyContainsPart()
        {
            DateTime beforeStartEarlierDate = new DateTime(2006, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime beforeStartLaterDate = new DateTime(2006, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime inPeriodEarlierDate = new DateTime(2007, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime inPeriodLaterDate = new DateTime(2008, 2, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime afterEndEarlierDate = new DateTime(2008, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime afterEndLaterDate = new DateTime(2008, 7, 1, 0, 0, 0, DateTimeKind.Utc);

            DateTimePeriod containsPeriod = new DateTimePeriod(inPeriodEarlierDate, inPeriodLaterDate);
            DateTimePeriod startEarlierFinishInPeriod = new DateTimePeriod(beforeStartEarlierDate, inPeriodLaterDate);
            DateTimePeriod startInFinishLaterPeriod = new DateTimePeriod(inPeriodEarlierDate, afterEndLaterDate);
            DateTimePeriod startEarlierFinishLaterPeriod = new DateTimePeriod(beforeStartLaterDate, afterEndLaterDate);
            DateTimePeriod startEarlierFinishAtStartPeriod = new DateTimePeriod(beforeStartLaterDate, _start);
            DateTimePeriod startAtEndFinishLaterPeriod = new DateTimePeriod(_end, afterEndEarlierDate);
            DateTimePeriod startEarlierFinishAtEndPeriod = new DateTimePeriod(beforeStartLaterDate, _end);
            DateTimePeriod startAtStartFinishLaterPeriod = new DateTimePeriod(_start, afterEndLaterDate);
            DateTimePeriod startLaterFinishLaterPeriod = new DateTimePeriod(afterEndEarlierDate, afterEndLaterDate);
            DateTimePeriod startEarlierFinishEarlierPeriod =
                new DateTimePeriod(beforeStartEarlierDate, beforeStartLaterDate);
            DateTimePeriod equalsPeriod = new DateTimePeriod(_start, _end);

            Assert.IsTrue(_period.ContainsPart(startEarlierFinishInPeriod));
            Assert.IsTrue(_period.ContainsPart(startInFinishLaterPeriod));
            Assert.IsTrue(_period.ContainsPart(containsPeriod));
            Assert.IsTrue(_period.ContainsPart(equalsPeriod));
            Assert.IsTrue(_period.ContainsPart(startEarlierFinishLaterPeriod));
            Assert.IsTrue(_period.ContainsPart(startEarlierFinishAtStartPeriod));
            Assert.IsTrue(_period.ContainsPart(startAtEndFinishLaterPeriod));
            Assert.IsTrue(_period.ContainsPart(startEarlierFinishAtEndPeriod));
            Assert.IsTrue(_period.ContainsPart(startAtStartFinishLaterPeriod));
            Assert.IsFalse(_period.ContainsPart(startEarlierFinishEarlierPeriod));
            Assert.IsFalse(_period.ContainsPart(startLaterFinishLaterPeriod));
        }

        [Test]
        public void VerifyContainsPartWithSettingForInclusion()
        {
            DateTime beforeStartEarlierDate = new DateTime(2006, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime beforeStartLaterDate = new DateTime(2006, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime inPeriodEarlierDate = new DateTime(2007, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime inPeriodLaterDate = new DateTime(2008, 2, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime afterEndEarlierDate = new DateTime(2008, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime afterEndLaterDate = new DateTime(2008, 7, 1, 0, 0, 0, DateTimeKind.Utc);

            DateTimePeriod containsPeriod = new DateTimePeriod(inPeriodEarlierDate, inPeriodLaterDate);
            DateTimePeriod startEarlierFinishInPeriod = new DateTimePeriod(beforeStartEarlierDate, inPeriodLaterDate);
            DateTimePeriod startInFinishLaterPeriod = new DateTimePeriod(inPeriodEarlierDate, afterEndLaterDate);
            DateTimePeriod startEarlierFinishLaterPeriod = new DateTimePeriod(beforeStartLaterDate, afterEndLaterDate);
            DateTimePeriod startEarlierFinishAtStartPeriod = new DateTimePeriod(beforeStartLaterDate, _start);
            DateTimePeriod startAtEndFinishLaterPeriod = new DateTimePeriod(_end, afterEndEarlierDate);
            DateTimePeriod startEarlierFinishAtEndPeriod = new DateTimePeriod(beforeStartLaterDate, _end);
            DateTimePeriod startAtStartFinishLaterPeriod = new DateTimePeriod(_start, afterEndLaterDate);
            DateTimePeriod startLaterFinishLaterPeriod = new DateTimePeriod(afterEndEarlierDate, afterEndLaterDate);
            DateTimePeriod startEarlierFinishEarlierPeriod =
                new DateTimePeriod(beforeStartEarlierDate, beforeStartLaterDate);
            DateTimePeriod equalsPeriod = new DateTimePeriod(_start, _end);

            Assert.IsTrue(_period.ContainsPart(startEarlierFinishInPeriod, false));
            Assert.IsTrue(_period.ContainsPart(startInFinishLaterPeriod, false));
            Assert.IsTrue(_period.ContainsPart(containsPeriod, false));
            Assert.IsTrue(_period.ContainsPart(equalsPeriod, false));
            Assert.IsTrue(_period.ContainsPart(startEarlierFinishLaterPeriod, false));
            Assert.IsFalse(_period.ContainsPart(startEarlierFinishAtStartPeriod, false));
            Assert.IsFalse(_period.ContainsPart(startAtEndFinishLaterPeriod, false));
            Assert.IsTrue(_period.ContainsPart(startEarlierFinishAtEndPeriod, false));
            Assert.IsTrue(_period.ContainsPart(startAtStartFinishLaterPeriod, false));
            Assert.IsFalse(_period.ContainsPart(startEarlierFinishEarlierPeriod, false));
            Assert.IsFalse(_period.ContainsPart(startLaterFinishLaterPeriod, false));
        }

        /// <summary>
        /// Verifies that DateTimes are checked correctly against a DateTimePeriod
        /// </summary>
        [Test]
        public void VerifyContainsPartOverloadWithDateTime()
        {
            DateTime startDate = new DateTime(2006, 7, 1, 10, 0, 0, DateTimeKind.Utc);
            DateTime endDate = new DateTime(2006, 7, 1, 15, 0, 0, DateTimeKind.Utc);
            DateTime checkTimeInside = new DateTime(2006, 7, 1, 12, 0, 0, DateTimeKind.Utc);
            DateTime checkTimeOutside = new DateTime(2006, 7, 1, 18, 0, 0, DateTimeKind.Utc);

            _period = new DateTimePeriod(startDate, endDate);

            Assert.IsTrue(_period.ContainsPart(checkTimeInside));
            Assert.IsFalse(_period.ContainsPart(checkTimeOutside));
            Assert.IsTrue(_period.ContainsPart(checkTimeInside));
        }

        /// <summary>
        /// Verifies the date collection property.
        /// </summary>
        [Test]
        public void VerifyDateCollection()
        {
            DateTime startDate = new DateTime(2007, 6, 1, 12, 31, 0, DateTimeKind.Utc);
            DateTime endDate = new DateTime(2007, 6, 5, 0, 0, 1, DateTimeKind.Utc);
            _period = new DateTimePeriod(startDate, endDate);

            ICollection<DateTime> resultDates = _period.DateCollection();
            Assert.AreEqual(5, resultDates.Count);
        }

        /// <summary>
        /// Verifies the six parameter constructor.
        /// </summary>
        [Test]
        public void VerifySixParameterConstructor()
        {
            _period = new DateTimePeriod(2000, 1, 1, 2005, 1, 1);
            Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), _period.StartDateTime);
            Assert.AreEqual(new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc), _period.EndDateTime);
        }

        /// <summary>
        /// Verifies that a new end time is created from a timespan
        /// </summary>
        [Test]
        public void CanChangePeriodEndTime()
        {
            DateTime startDate = new DateTime(2007, 8, 10, 12, 00, 0, DateTimeKind.Utc);
            DateTime endDate = new DateTime(2007, 8, 10, 12, 30, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(startDate, endDate);

            DateTime expStartDate = new DateTime(2007, 8, 10, 12, 00, 0, DateTimeKind.Utc);
            DateTime expEndDate = new DateTime(2007, 8, 12, 13, 30, 0, DateTimeKind.Utc);
            DateTimePeriod expPeriod = new DateTimePeriod(expStartDate, expEndDate);

            Assert.AreEqual(expPeriod.EndDateTime, _period.ChangeEndTime(new TimeSpan(2, 1, 0, 0, 0)).EndDateTime);
            Assert.AreEqual(expPeriod.StartDateTime, _period.ChangeEndTime(new TimeSpan(2, 1, 0, 0, 0)).StartDateTime);
        }

        /// <summary>
        /// Verifies that a new end time is created from a negative timespan
        /// </summary>
        [Test]
        public void CanChangePeriodEndTimeNegative()
        {
            DateTime startDate = new DateTime(2007, 8, 10, 12, 00, 0, DateTimeKind.Utc);
            DateTime endDate = new DateTime(2007, 8, 10, 14, 30, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(startDate, endDate);

            DateTime expStartDate = new DateTime(2007, 8, 10, 12, 00, 0, DateTimeKind.Utc);
            DateTime expEndDate = new DateTime(2007, 8, 10, 13, 30, 0, DateTimeKind.Utc);
            DateTimePeriod expPeriod = new DateTimePeriod(expStartDate, expEndDate);

            Assert.AreEqual(expPeriod.EndDateTime, _period.ChangeEndTime(TimeSpan.FromHours(-1)).EndDateTime);
            Assert.AreEqual(expPeriod.StartDateTime, _period.ChangeEndTime(TimeSpan.FromHours(-1)).StartDateTime);
        }

        /// <summary>
        /// Verifies that a new start time is created from a timespan
        /// </summary>
        [Test]
        public void CanChangePeriodStartTime()
        {
            DateTime startDate = new DateTime(2007, 8, 09, 10, 30, 0, DateTimeKind.Utc);
            DateTime endDate = new DateTime(2007, 8, 10, 12, 30, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(startDate, endDate);

            DateTime expStartDate = new DateTime(2007, 8, 10, 11, 30, 0, DateTimeKind.Utc);
            DateTime expEndDate = new DateTime(2007, 8, 10, 12, 30, 0, DateTimeKind.Utc);
            DateTimePeriod expPeriod = new DateTimePeriod(expStartDate, expEndDate);

            Assert.AreEqual(expPeriod.StartDateTime, _period.ChangeStartTime(new TimeSpan(1, 1, 0, 0, 0)).StartDateTime);
            Assert.AreEqual(expPeriod.EndDateTime, _period.ChangeStartTime(new TimeSpan(1, 1, 0, 0, 0)).EndDateTime);
        }

        /// <summary>
        /// Verifies that a new end time is created from a negative timespan
        /// </summary>
        [Test]
        public void CanChangePeriodStartTimeNegative()
        {
            DateTime startDate = new DateTime(2007, 8, 10, 12, 00, 0, DateTimeKind.Utc);
            DateTime endDate = new DateTime(2007, 8, 10, 13, 30, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(startDate, endDate);

            DateTime expStartDate = new DateTime(2007, 8, 10, 11, 00, 0, DateTimeKind.Utc);
            DateTime expEndDate = new DateTime(2007, 8, 10, 13, 30, 0, DateTimeKind.Utc);
            DateTimePeriod expPeriod = new DateTimePeriod(expStartDate, expEndDate);

            Assert.AreEqual(expPeriod.StartDateTime, _period.ChangeStartTime(new TimeSpan(0, -1, 0, 0, 0)).StartDateTime);
            Assert.AreEqual(expPeriod.EndDateTime, _period.ChangeStartTime(new TimeSpan(0, -1, 0, 0, 0)).EndDateTime);
        }

        /// <summary>
        /// Verifies the local date time properties works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-23
        /// </remarks>
        [Test]
        public void VerifyLocalDateTimesWorks()
        {
            DateTime expectedLocalStart =
                TimeZoneInfo.ConvertTimeFromUtc(_start, (TimeZoneInfo)StateHolder.Instance.StateReader.SessionScopeData.TimeZone.TimeZoneInfoObject);
            DateTime expectedLocalEnd =
                TimeZoneInfo.ConvertTimeFromUtc(_end, (TimeZoneInfo)StateHolder.Instance.StateReader.SessionScopeData.TimeZone.TimeZoneInfoObject);

            Assert.AreEqual(expectedLocalStart, _period.LocalStartDateTime);
            Assert.AreEqual(expectedLocalEnd, _period.LocalEndDateTime);
        }

        [Test]
        public void VerifyDateTimeLocalWorks()
        {
            ICccTimeZoneInfo timeZoneInfo = StateHolder.Instance.StateReader.SessionScopeData.TimeZone;
            DateTime expectedLocalStart =
                TimeZoneInfo.ConvertTimeFromUtc(_start, (TimeZoneInfo)timeZoneInfo.TimeZoneInfoObject);
            DateTime expectedLocalEnd =
                TimeZoneInfo.ConvertTimeFromUtc(_end, (TimeZoneInfo)timeZoneInfo.TimeZoneInfoObject);

            Assert.AreEqual(expectedLocalStart, _period.StartDateTimeLocal(timeZoneInfo));
            Assert.AreEqual(expectedLocalEnd, _period.EndDateTimeLocal(timeZoneInfo));
        }

        /// <summary>
        /// Verifies the date time period whole days work.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-23
        /// </remarks>
        [Test]
        public void VerifyDateTimePeriodWholeDaysWork()
        {
            DateTime startDate = DateTime.UtcNow.Date;
            DateTime expectedEndDate = startDate.AddDays(4).AddTicks(-1L);

            _period = DateTimeFactory.CreateDateTimePeriod(startDate, 3);

            Assert.AreEqual(startDate, _period.StartDateTime);
            Assert.AreEqual(expectedEndDate, _period.EndDateTime);
        }

        /// <summary>
        /// Verifies the date time period whole days work when days out of range.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-23
        /// </remarks>
        [Test]
        public void VerifyDateTimePeriodWholeDaysWorkWhenDaysOutOfRange()
        {
            DateTime startDate = DateTime.UtcNow.Date;

            _period = DateTimeFactory.CreateDateTimePeriod(startDate, int.MaxValue);

            Assert.AreEqual(startDate, _period.StartDateTime);
            Assert.AreEqual(DateTime.SpecifyKind(DateTime.MaxValue,DateTimeKind.Utc), _period.EndDateTime);
        }

        [Test]
        public void VerifyChangeToSummertimeWorksOnIntervals()
        {
            DateTime startDateTime = new DateTime(2007, 3, 24, 23, 0, 0);
            DateTime endDateTime = new DateTime(2007, 3, 25, 3, 45, 0);

            _period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTime, endDateTime);

            IList<IntervalDefinition> list = _period.IntervalsFromHourCollection(15, TimeZoneHelper.CurrentSessionTimeZone);

            Assert.AreEqual(15, list.Count);
            Assert.AreEqual(23, list[0].TimeSpan.Hours);
            Assert.AreEqual(15, list[1].TimeSpan.Minutes);
            Assert.AreEqual(30, list[14].TimeSpan.Minutes);
        }

        [Test]
        public void VerifyChangeFromSummertimeWorksOnIntervals()
        {
            DateTime startDateTime = new DateTime(2007, 10, 27, 23, 0, 0);
            DateTime endDateTime = new DateTime(2007, 10, 28, 3, 45, 0);

            _period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTime, endDateTime);

            IList<IntervalDefinition> list = _period.IntervalsFromHourCollection(15, TimeZoneHelper.CurrentSessionTimeZone);

            Assert.AreEqual(23, list.Count);
            Assert.AreEqual(23, list[0].TimeSpan.Hours);
            Assert.AreEqual(30, list[22].TimeSpan.Minutes);
        }

        [Test]
        public void VerifyEmptyDateTimePeriodOnIntervals()
        {
            _period = new DateTimePeriod();

            IList<IntervalDefinition> list = _period.IntervalsFromHourCollection(15, TimeZoneHelper.CurrentSessionTimeZone);

            Assert.AreEqual(0, list.Count);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyToSmallResolutionIsNotAllowedOnIntervalCollection()
        {
            DateTime startDateTime = new DateTime(2007, 10, 27, 23, 0, 0);
            DateTime endDateTime = new DateTime(2007, 10, 28, 3, 45, 0);

            _period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTime, endDateTime);
            _period.IntervalsFromHourCollection(0, TimeZoneHelper.CurrentSessionTimeZone);
        }

        [Test]
        public void VerifyHighResolutionIsAllowedOnIntervalCollection()
        {
            DateTime startDateTime = new DateTime(2007, 10, 27, 23, 0, 0);
            DateTime endDateTime = new DateTime(2007, 10, 27, 23, 0, 0);

            _period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTime, endDateTime);
            _period.IntervalsFromHourCollection(60, TimeZoneHelper.CurrentSessionTimeZone);
        }

        [Test]
        public void VerifyHigherResolutionIsAllowedOnIntervalCollection()
        {
            DateTime startDateTime = new DateTime(2007, 10, 26, 23, 0, 0);
            DateTime endDateTime = new DateTime(2007, 10, 27, 23, 0, 0);

            _period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTime, endDateTime);
            _period.IntervalsFromHourCollection(120, TimeZoneHelper.CurrentSessionTimeZone);
        }

        [Test, Ignore("Removed this check due to bug 9873")]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyEvenDividedOnIntervalCollection()
        {
            DateTime startDateTime = new DateTime(2007, 10, 27, 23, 0, 0);
            DateTime endDateTime = new DateTime(2007, 10, 27, 23, 45, 0);

            _period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTime, endDateTime);
            _period.IntervalsFromHourCollection(13, TimeZoneHelper.CurrentSessionTimeZone);
        }

        /// <summary>
        /// Verifies the date time period is less than y works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-06
        /// </remarks>
        [Test]
        public void VerifyDateTimePeriodIsLessThan()
        {
            DateTime startDate1 = new DateTime(2007, 8, 10, 12, 00, 0, DateTimeKind.Utc);
            DateTime endDate1 = new DateTime(2007, 8, 10, 13, 30, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(startDate1, endDate1);

            DateTime startDate2 = new DateTime(2007, 8, 10, 11, 00, 0, DateTimeKind.Utc);
            DateTime endDate2 = new DateTime(2007, 8, 10, 13, 30, 0, DateTimeKind.Utc);
            DateTimePeriod period2 = new DateTimePeriod(startDate2, endDate2);

            Assert.IsTrue(_period < period2);
            Assert.IsFalse(period2 < _period);
        }

        /// <summary>
        /// Verifies the date time period can return 24 hour chunks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-21
        /// </remarks>
        [Test]
        public void VerifyDateTimePeriodCanReturn24HourChunks()
        {
            DateTime startDate1 = new DateTime(2007, 8, 10, 12, 00, 0, DateTimeKind.Utc);
            DateTime endDate1 = new DateTime(2007, 8, 13, 13, 30, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(startDate1, endDate1);

            IList<DateTimePeriod> wholeDays = _period.WholeDayCollection();

            Assert.AreEqual(4, wholeDays.Count);
            Assert.AreEqual(new TimeSpan(1, 30, 0), wholeDays[3].ElapsedTime());
        }

        [Test]
        public void VerifyDateTimePeriodCanReturn24HourChunksDuringDst()
        {
            DateTime startDate1 = new DateTime(2008, 10, 25, 12, 00, 0, DateTimeKind.Utc);
            DateTime endDate1 = new DateTime(2008, 10, 26, 13, 30, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(startDate1, endDate1);

            IList<DateTimePeriod> wholeDays = _period.WholeDayCollection();

            Assert.AreEqual(2, wholeDays.Count);
            Assert.AreEqual(new TimeSpan(0, 30, 0), wholeDays[1].ElapsedTime());
            Assert.AreEqual(wholeDays[0].LocalStartDateTime.TimeOfDay, wholeDays[1].LocalStartDateTime.TimeOfDay);

            startDate1 = new DateTime(2008, 3, 29, 12, 00, 0, DateTimeKind.Utc);
            endDate1 = new DateTime(2008, 3, 30, 13, 30, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(startDate1, endDate1);

            wholeDays = _period.WholeDayCollection();

            Assert.AreEqual(2, wholeDays.Count);
            Assert.AreEqual(new TimeSpan(2, 30, 0), wholeDays[1].ElapsedTime());
            Assert.AreEqual(wholeDays[0].LocalStartDateTime.TimeOfDay, wholeDays[1].LocalStartDateTime.TimeOfDay);
        }

        [Test]
        public void VerifyDateTimePeriodCanReturnLocalDaysAffected()
        {
            DateTime startDate1 = new DateTime(2007, 8, 10, 22, 00, 0, DateTimeKind.Utc);
            DateTime endDate1 = new DateTime(2007, 8, 13, 21, 30, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(startDate1, endDate1);

            IList<DateTime> daysAffected = _period.LocalDaysAffected();

            Assert.AreEqual(3, daysAffected.Count);
            Assert.AreEqual(new DateTime(2007, 8, 11), daysAffected[0]);
            Assert.AreEqual(new DateTime(2007, 8, 13), daysAffected[2]);
        }


        [Test]
        public void VerifyDateTimePeriodCanReturnUtcDaysAffected()
        {
            DateTime startDate1 = new DateTime(2007, 8, 10, 22, 00, 0, DateTimeKind.Utc);
            DateTime endDate1 = new DateTime(2007, 8, 13, 21, 30, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(startDate1, endDate1);

            IList<DateTime> daysAffected = _period.UtcDaysAffected();

            Assert.AreEqual(4, daysAffected.Count);
            Assert.AreEqual(new DateTime(2007, 8, 10), daysAffected[0]);
            Assert.AreEqual(new DateTime(2007, 8, 13), daysAffected[3]);
        }

        #region WholeWeeks
        
        [Test, Explicit("Micke - vad g�r vi med denna?")]
        public void VerifyToWholeWeeks()
        {
            _period = new DateTimePeriod(2008,5,4,2008,5,5);
            Assert.AreEqual(new DateTimePeriod(2008, 4, 28, 2008, 5, 12), _period.WholeWeek(CultureInfo.GetCultureInfo("sv-SE"), new CccTimeZoneInfo(TimeZoneInfo.Utc)));
            Assert.AreEqual(new DateTimePeriod(2008, 4, 27, 2008, 5, 11), _period.WholeWeek(CultureInfo.GetCultureInfo("en-US"), new CccTimeZoneInfo(TimeZoneInfo.Utc)));
        }


        #endregion


        [Test]
        public void VerifyDateTimePeriodIsGreaterThan()
        {
            DateTime startDate1 = new DateTime(2007, 8, 10, 12, 00, 0, DateTimeKind.Utc);
            DateTime endDate1 = new DateTime(2007, 8, 10, 13, 30, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(startDate1, endDate1);

            DateTime startDate2 = new DateTime(2007, 8, 10, 11, 00, 0, DateTimeKind.Utc);
            DateTime endDate2 = new DateTime(2007, 8, 10, 13, 30, 0, DateTimeKind.Utc);
            DateTimePeriod period2 = new DateTimePeriod(startDate2, endDate2);

            Assert.IsFalse(_period > period2);
            Assert.IsTrue(period2 > _period);
        }

        [Test]
        public void VerifyMaximumPeriod()
        {
            _period = new DateTimePeriod(1900,1,1,1901,1,1);
            DateTimePeriod per2 = new DateTimePeriod(1999,1,1,2000,1,1);
            Assert.AreEqual(new DateTimePeriod(1900,1,1,2000,1,1), _period.MaximumPeriod(per2));
        }

        [Test]
        public void VerifyUnion()
        {
            DateTime start = new DateTime(2007, 6, 1, 23, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(2007, 6, 2, 1, 0, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(start,end);

            //outside
            DateTime start1 = new DateTime(2007, 5, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end1 = new DateTime(2007, 5, 1, 1, 0, 0, DateTimeKind.Utc);
            DateTimePeriod per1 = new DateTimePeriod(start1, end1);
            Assert.IsFalse(_period.Union(per1).HasValue);
            Assert.IsFalse(per1.Union(_period).HasValue);

            //adjacent
            start1 = new DateTime(2007, 6, 1, 22, 0, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 1, 23, 0, 0, DateTimeKind.Utc);
            per1 = new DateTimePeriod(start1, end1);
            DateTime expStart = start1;
            DateTime expEnd = end;
            DateTimePeriod expPeriod = new DateTimePeriod(expStart, expEnd);
            DateTimePeriod? result = _period.Union(per1);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(expPeriod, result);

            //intersect
            start1 = new DateTime(2007, 6, 1, 22, 30, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 1, 23, 30, 0, DateTimeKind.Utc);
            per1 = new DateTimePeriod(start1, end1);
            expStart = start1;
            expEnd = end;
            expPeriod = new DateTimePeriod(expStart, expEnd);
            result = _period.Union(per1);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(expPeriod, result);

            //equals
            start1 = new DateTime(2007, 6, 1, 23, 0, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 2, 1, 0, 0, DateTimeKind.Utc);
            per1 = new DateTimePeriod(start1, end1);
            expStart = start1;
            expEnd = end1;
            expPeriod = new DateTimePeriod(expStart, expEnd);
            result = _period.Union(per1);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(expPeriod, result);

            //inside
            start1 = new DateTime(2007, 6, 1, 23, 30, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 2, 0, 30, 0, DateTimeKind.Utc);
            per1 = new DateTimePeriod(start1, end1);
            expStart = start;
            expEnd = end;
            expPeriod = new DateTimePeriod(expStart, expEnd);
            result = _period.Union(per1);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(expPeriod, result);
        }

        [Test]
        public void VerifyAdjacent()
        {
            DateTime start = new DateTime(2007, 6, 1, 23, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(2007, 6, 2, 1, 0, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(start, end);

            //adjacent with start date equals end date (no offset)
            DateTime start1 = new DateTime(2007, 6, 1, 22, 0, 0, DateTimeKind.Utc);
            DateTime end1 = new DateTime(2007, 6, 1, 23, 0, 0, DateTimeKind.Utc);
            DateTimePeriod per1 = new DateTimePeriod(start1, end1);
            Assert.IsTrue(_period.Adjacent(per1));

            //outside
            start1 = new DateTime(2007, 5, 1, 0, 0, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 5, 1, 1, 0, 0, DateTimeKind.Utc);
            per1 = new DateTimePeriod(start1, end1);
            Assert.IsFalse(_period.Adjacent(per1));
            Assert.IsFalse(per1.Adjacent(_period));

            //intersect
            start1 = new DateTime(2007, 6, 1, 22, 30, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 1, 23, 30, 0, DateTimeKind.Utc);
            per1 = new DateTimePeriod(start1, end1);
            Assert.IsFalse(_period.Adjacent(per1));

            //equals
            start1 = new DateTime(2007, 6, 1, 23, 0, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 2, 1, 0, 0, DateTimeKind.Utc);
            per1 = new DateTimePeriod(start1, end1);
            Assert.IsFalse(_period.Adjacent(per1));

            //inside
            start1 = new DateTime(2007, 6, 1, 23, 30, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 2, 0, 30, 0, DateTimeKind.Utc);
            per1 = new DateTimePeriod(start1, end1);
            Assert.IsFalse(_period.Adjacent(per1));
        }

        [Test]
        public void VerifyAdjacentWithOffset()
        {
            DateTime start = new DateTime(2007, 6, 1, 23, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(2007, 6, 2, 1, 0, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(start, end);

            //adjacent with offset
            DateTime start1 = new DateTime(2007, 6, 1, 20, 0, 0, DateTimeKind.Utc);
            DateTime end1 = new DateTime(2007, 6, 1, 22, 59, 0, DateTimeKind.Utc);
            DateTimePeriod per1 = new DateTimePeriod(start1, end1);
            Assert.IsTrue(_period.Adjacent(per1, new TimeSpan(0, 1, 0)));
            Assert.IsFalse(_period.Adjacent(per1, new TimeSpan(0, 0, 30)));

            start1 = new DateTime(2007, 6, 1, 20, 0, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 1, 23, 05, 0, DateTimeKind.Utc);
            per1 = new DateTimePeriod(start1, end1);
            Assert.IsTrue(_period.Adjacent(per1, new TimeSpan(0, 6, 0)));
            Assert.IsTrue(_period.Adjacent(per1, new TimeSpan(0, 5, 0)));
            Assert.IsFalse(_period.Adjacent(per1, new TimeSpan(0, 4, 0)));

        }

        [Test]
        public void VerifyIntersection()
        {
            DateTime start = new DateTime(2007, 6, 1, 23, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(2007, 6, 2, 1, 0, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(start, end);

            //outside
            DateTime start1 = new DateTime(2007, 5, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end1 = new DateTime(2007, 5, 1, 1, 0, 0, DateTimeKind.Utc);
            DateTimePeriod per1 = new DateTimePeriod(start1, end1);
            Assert.IsNull(_period.Intersection(per1));
            Assert.IsNull(per1.Intersection(_period));

            //adjacent
            start1 = new DateTime(2007, 6, 1, 22, 0, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 1, 23, 0, 0, DateTimeKind.Utc);
            per1 = new DateTimePeriod(start1, end1);
            Assert.IsNull(_period.Intersection(per1));
            Assert.IsNull(per1.Intersection(_period));

            //intersect
            start1 = new DateTime(2007, 6, 1, 22, 30, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 1, 23, 30, 0, DateTimeKind.Utc);
            per1 = new DateTimePeriod(start1, end1);
            Assert.AreEqual(new TimeSpan(0, 0, 30, 0), _period.Intersection(per1).Value.ElapsedTime());
            Assert.AreEqual(new TimeSpan(0, 0, 30, 0), per1.Intersection(_period).Value.ElapsedTime());

            //equals
            start1 = new DateTime(2007, 6, 1, 23, 0, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 2, 1, 0, 0, DateTimeKind.Utc);
            per1 = new DateTimePeriod(start1, end1);
            Assert.AreEqual(new TimeSpan(0, 2, 0, 0), _period.Intersection(per1).Value.ElapsedTime());
            Assert.AreEqual(new TimeSpan(0, 2, 0, 0), per1.Intersection(_period).Value.ElapsedTime());

            //inside
            start1 = new DateTime(2007, 6, 1, 23, 30, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 2, 0, 30, 0, DateTimeKind.Utc);
            per1 = new DateTimePeriod(start1, end1);
            Assert.AreEqual(new TimeSpan(0, 1, 0, 0), _period.Intersection(per1).Value.ElapsedTime());
            Assert.AreEqual(new TimeSpan(0, 1, 0, 0), per1.Intersection(_period).Value.ElapsedTime());
        }

        [Test]
        public void VerifyDifferenceBetweenIntersectionAndSharedPeriod()
        {
            DateTime start = new DateTime(2007, 6, 1, 23, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(2007, 6, 2, 1, 0, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(start, end);

            // WORKS SAME

            //intersect
            DateTime start1 = new DateTime(2007, 6, 1, 22, 30, 0, DateTimeKind.Utc);
            DateTime end1 = new DateTime(2007, 6, 1, 23, 30, 0, DateTimeKind.Utc);
            DateTimePeriod period1 = new DateTimePeriod(start1, end1);
            Assert.AreEqual(_period.SharedPeriod(period1).ElapsedTime(), _period.Intersection(period1).Value.ElapsedTime());
            Assert.AreEqual(period1.SharedPeriod(_period).ElapsedTime(), period1.Intersection(_period).Value.ElapsedTime());

            //equals
            start1 = new DateTime(2007, 6, 1, 23, 0, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 2, 1, 0, 0, DateTimeKind.Utc);
            period1 = new DateTimePeriod(start1, end1);
            Assert.AreEqual(_period.SharedPeriod(period1).ElapsedTime(), _period.Intersection(period1).Value.ElapsedTime());
            Assert.AreEqual(period1.SharedPeriod(_period).ElapsedTime(), period1.Intersection(_period).Value.ElapsedTime());

            //inside
            start1 = new DateTime(2007, 6, 1, 23, 30, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 2, 0, 30, 0, DateTimeKind.Utc);
            period1 = new DateTimePeriod(start1, end1);
            Assert.AreEqual(_period.SharedPeriod(period1).ElapsedTime(), _period.Intersection(period1).Value.ElapsedTime());
            Assert.AreEqual(period1.SharedPeriod(_period).ElapsedTime(), period1.Intersection(_period).Value.ElapsedTime());


            // DIFFERENCE

            //adjacent
            start1 = new DateTime(2007, 6, 1, 22, 0, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 1, 23, 0, 0, DateTimeKind.Utc);
            period1 = new DateTimePeriod(start1, end1);

            Assert.IsNull(_period.Intersection(period1));
            Assert.IsNull(period1.Intersection(_period));

            Assert.IsTrue(_period.SharedPeriod(period1).StartDateTime == _period.SharedPeriod(period1).EndDateTime);
            Assert.IsTrue(period1.SharedPeriod(_period).StartDateTime == period1.SharedPeriod(_period).EndDateTime);

        }

        [Test]
        public void VerifyAffectedHours()
        {
            _period =
                new DateTimePeriod(new DateTime(2007, 1, 1, 8, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 1, 10, 30, 0, DateTimeKind.Utc));
            IList<DateTimePeriod> retList = _period.AffectedHourCollection();

            Assert.AreEqual(3, retList.Count);
        }

        [Test]
        public void VerifyAffectedHoursOverMidnight()
        {
            _period =
                new DateTimePeriod(new DateTime(2007, 1, 1, 20, 15, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 1, 23, 30, 0, DateTimeKind.Utc));
            IList<DateTimePeriod> retList = _period.AffectedHourCollection();

            Assert.AreEqual(4, retList.Count);
        }

        [Test]
        public void VerifyToString()
        {
            Assert.AreEqual(_start + " - " + _end, _period.ToString());
        }

        [Test]
        public void VerifyToLocalString()
        {
            Assert.AreEqual(_period.LocalStartDateTime + " - " + _period.LocalEndDateTime, _period.ToLocalString());
        }

        [Test]
        public void VerifyLocalDateString()
        {
            Assert.AreEqual(_period.LocalStartDateTime.ToShortDateString() + " - " + _period.LocalEndDateTime.AddDays(-1).ToShortDateString(), _period.LocalDateString);
        }

        [Test]
        public void VerifyLocalDaysAffectedShorterThanOneDay()
        {
            _period = new DateTimePeriod(new DateTime(2000, 1, 1, 23, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 2, 23, 0, 0, DateTimeKind.Utc).AddMinutes(-1));
            DateTimePeriod notWorking = new DateTimePeriod(new DateTime(2000, 1, 1, 23, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 2, 23, 0, 0, DateTimeKind.Utc).AddTicks(-1));

            Assert.AreEqual(1, _period.LocalDaysAffected().Count);
            Assert.AreEqual(1, notWorking.LocalDaysAffected().Count);
        }


        [Test]
        public void VerifyCanGetListOfPeriodsDependingOnTimeSpan()
        {
            DateTime baseDateTime = new DateTime(2001,1,1,8,0,0,DateTimeKind.Utc);
            TimeSpan interval = TimeSpan.FromHours(1);
            int hours = 24;
            _period = new DateTimePeriod(baseDateTime,baseDateTime.Add(TimeSpan.FromHours(hours)));
            IList<DateTimePeriod> periods = _period.Intervals(interval);
            Assert.AreEqual(hours,periods.Count,"Should be 24 intervals");
            Assert.AreEqual(new DateTimePeriod(baseDateTime, baseDateTime.Add(interval)), periods[0]);
            Assert.AreEqual(new DateTimePeriod(baseDateTime.Add(interval), baseDateTime.Add(interval).Add(interval)), periods[1]);
            
            periods = new DateTimePeriod(baseDateTime, baseDateTime.Add(TimeSpan.FromHours(1))).Intervals(TimeSpan.FromHours(2));
            Assert.AreEqual(new DateTimePeriod(baseDateTime, baseDateTime.Add(TimeSpan.FromHours(2))), periods[0],"Minium interval is the length of the interval, not the period");

        }

        [Test]
        public void VerifyMergedListWhenOneListIsEmpty()
        {
            _period = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
            DateTimePeriod dtp2 = new DateTimePeriod(2000, 1, 3, 2000, 1, 4);
            //DateTimePeriod dtp3 = new DateTimePeriod(2000, 1, 5, 2000, 1, 6);
            //DateTimePeriod dtp4 = new DateTimePeriod(2000, 1, 7, 2000, 1, 8);
            IList<DateTimePeriod> list1 = new List<DateTimePeriod>();
            IList<DateTimePeriod> list2 = new List<DateTimePeriod>();

            list1.Add(_period);
            list1.Add(dtp2);

            IList<DateTimePeriod> ret = DateTimePeriod.MergeLists(list1, list2);
            Assert.AreEqual(_period, ret[0]);
            Assert.AreEqual(dtp2, ret[1]);

            //Reverse parameters
            ret = DateTimePeriod.MergeLists(list2, list1);
            Assert.AreEqual(_period, ret[0]);
            Assert.AreEqual(dtp2, ret[1]);
        }

        [Test]
        public void VerifyMergedList1()
        {
            _period = new DateTimePeriod(2004, 1, 1, 2009, 1, 1);
            DateTimePeriod dtp2 = new DateTimePeriod(2015, 1, 1, 2026, 1, 1);
            DateTimePeriod dtp3 = new DateTimePeriod(2032, 1, 1, 2034, 1, 1);
            DateTimePeriod dtp4 = new DateTimePeriod(2039, 1, 1, 2066, 1, 1);

            DateTimePeriod dtp5 = new DateTimePeriod(2001, 1, 1, 2006, 1, 1);
            DateTimePeriod dtp6 = new DateTimePeriod(2011, 1, 1, 2043, 1, 1);
            DateTimePeriod dtp7 = new DateTimePeriod(2069, 1, 1, 2070, 1, 1);
            IList<DateTimePeriod> list1 = new List<DateTimePeriod>();
            IList<DateTimePeriod> list2 = new List<DateTimePeriod>();

            list1.Add(_period);
            list1.Add(dtp2);
            list2.Add(dtp3);
            list2.Add(dtp4);

            list1.Add(dtp5);
            list2.Add(dtp6);
            list2.Add(dtp7);

            IList<DateTimePeriod> ret = DateTimePeriod.MergeLists(list1, list2);
            Assert.AreEqual(3, ret.Count);
        }

        [Test]
        public void VerifyMergedListNoIntersect()
        {
            _period = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
            DateTimePeriod dtp2 = new DateTimePeriod(2000, 1, 3, 2000, 1, 4);
            DateTimePeriod dtp3 = new DateTimePeriod(2000, 1, 5, 2000, 1, 6);
            DateTimePeriod dtp4 = new DateTimePeriod(2000, 1, 7, 2000, 1, 8);
            IList<DateTimePeriod> list1 = new List<DateTimePeriod>();
            IList<DateTimePeriod> list2 = new List<DateTimePeriod>();

            list1.Add(_period);
            list1.Add(dtp2);
            list2.Add(dtp3);
            list2.Add(dtp4);

            IList<DateTimePeriod> ret = DateTimePeriod.MergeLists(list1, list2);
            Assert.AreEqual(4, ret.Count);
            Assert.AreEqual(_period, ret[0]);
            Assert.AreEqual(dtp2, ret[1]);
            Assert.AreEqual(dtp3, ret[2]);
            Assert.AreEqual(dtp4, ret[3]);
        }

        [Test]
        public void VerifyMergedListIntersectAndContains()
        {
            _period = new DateTimePeriod(2000, 1, 1, 2000, 1, 5);
            DateTimePeriod dtp2 = new DateTimePeriod(2000, 1, 3, 2000, 1, 4);
            DateTimePeriod dtp3 = new DateTimePeriod(2000, 1, 7, 2000, 1, 9);
            DateTimePeriod dtp4 = new DateTimePeriod(2000, 1, 8, 2000, 1, 10);
            IList<DateTimePeriod> list1 = new List<DateTimePeriod>();
            IList<DateTimePeriod> list2 = new List<DateTimePeriod>();

            list1.Add(_period);
            list1.Add(dtp2);
            list2.Add(dtp3);
            list2.Add(dtp4);

            IList<DateTimePeriod> ret = DateTimePeriod.MergeLists(list1, list2);
            Assert.AreEqual(2, ret.Count);
            Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2000, 1, 5), ret[0]);
            Assert.AreEqual(new DateTimePeriod(2000, 1, 7, 2000, 1, 10), ret[1]);
        }

        [Test]
        public void VerifyMergedListAdjacent()
        {
            _period = new DateTimePeriod(2000, 1, 1, 2000, 1, 5);
            DateTimePeriod dtp2 = new DateTimePeriod(2000, 1, 5, 2000, 1, 6);
            DateTimePeriod dtp3 = new DateTimePeriod(2000, 1, 6, 2000, 1, 7);
            DateTimePeriod dtp4 = new DateTimePeriod(2000, 1, 7, 2000, 1, 10);
            IList<DateTimePeriod> list1 = new List<DateTimePeriod>();
            IList<DateTimePeriod> list2 = new List<DateTimePeriod>();

            list1.Add(_period);
            list1.Add(dtp2);
            list2.Add(dtp3);
            list2.Add(dtp4);

            IList<DateTimePeriod> ret = DateTimePeriod.MergeLists(list1, list2);
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2000, 1, 10), ret[0]);
        }

        [Test]
        public void VerifyMergedListEqualLists()
        {
            _period = new DateTimePeriod(2000, 1, 1, 2000, 1, 5);
            DateTimePeriod dtp2 = new DateTimePeriod(2000, 1, 1, 2000, 1, 5);

            IList<DateTimePeriod> list1 = new List<DateTimePeriod>();
            IList<DateTimePeriod> list2 = new List<DateTimePeriod>();

            list1.Add(_period);
            list2.Add(dtp2);

            IList<DateTimePeriod> ret = DateTimePeriod.MergeLists(list1, list2);
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2000, 1, 5), ret[0]);
        }

        [Test]
        public void VerifyRoundedCountOfDays()
        {
            DateTime date1 = new DateTime(2000,1,1,23,0,0,DateTimeKind.Utc);
            DateTime date2 = new DateTime(2000, 1, 5, 22, 0, 0, DateTimeKind.Utc);
            
            _period = new DateTimePeriod(date1, date2);

            TimeSpan elapsedTime = date2 - date1;
            double result = _period.RoundedDaysCount();
            Assert.AreEqual(Math.Round(elapsedTime.TotalDays),result);

            date2 = new DateTime(2000, 1, 6, 3, 0, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(date1, date2);

            elapsedTime = date2 - date1;
            result = _period.RoundedDaysCount();
            Assert.AreEqual(Math.Round(elapsedTime.TotalDays), result);
        }

        [Test]
        public void VerifyToDateOnlyPeriod()
        {
            DateOnly startDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(_start, _cccTimeZoneInfo));
            DateOnly endDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(_end, _cccTimeZoneInfo));
            DateOnlyPeriod dateOnlyPeriod = _period.ToDateOnlyPeriod(_cccTimeZoneInfo);
            Assert.AreEqual(dateOnlyPeriod.StartDate, startDate);
            Assert.AreEqual(dateOnlyPeriod.EndDate, endDate);
        }

        [Test]
        public void VerifyToDateOnlyPeriodWorksBothWays()
        {
            DateOnly startDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(_start, _cccTimeZoneInfo));
            DateOnly endDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(_end, _cccTimeZoneInfo));
            DateOnlyPeriod dateOnlyPeriod = _period.ToDateOnlyPeriod(_cccTimeZoneInfo);
            Assert.AreEqual(dateOnlyPeriod.StartDate, startDate);
            Assert.AreEqual(dateOnlyPeriod.EndDate, endDate);
            DateTimePeriod dateTimePeriod = dateOnlyPeriod.ToDateTimePeriod(_cccTimeZoneInfo);
            DateOnlyPeriod dateOnlyPeriodAfterBeingDateTimePeriod = dateTimePeriod.ToDateOnlyPeriod(_cccTimeZoneInfo);
            Assert.AreEqual(dateOnlyPeriodAfterBeingDateTimePeriod.StartDate, startDate);
            Assert.AreEqual(dateOnlyPeriodAfterBeingDateTimePeriod.EndDate, endDate);
        }

        [Test]
        public void VerifyToDateOnlyPeriodWorksBothWaysWithOneDate()
        {
            DateTime dateTime = new DateTime(2007,12,31,23,0,0,DateTimeKind.Utc);
            _cccTimeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            DateOnly startDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(dateTime, _cccTimeZoneInfo));
            DateOnly endDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(dateTime, _cccTimeZoneInfo));
            _period = new DateTimePeriod(dateTime,dateTime);
            DateOnlyPeriod dateOnlyPeriod = _period.ToDateOnlyPeriod(_cccTimeZoneInfo);
            Assert.AreEqual(dateOnlyPeriod.StartDate, startDate);
            Assert.AreEqual(dateOnlyPeriod.EndDate, endDate);
        }

        [Test]
        public void VerifyCultureDoesNotAffectHashCode()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            CultureInfo swedishCulture = CultureInfo.GetCultureInfo(1053);
            CultureInfo englishCulture = CultureInfo.GetCultureInfo(1033);
            Thread.CurrentThread.CurrentCulture = swedishCulture;
            int hashCodeWithSwedishCulture = _period.GetHashCode();
            Thread.CurrentThread.CurrentCulture = englishCulture;
            int hashCodeWithEnglishCulture = _period.GetHashCode();
            Thread.CurrentThread.CurrentCulture = currentCulture;

            Assert.AreEqual(hashCodeWithEnglishCulture,hashCodeWithSwedishCulture);
        }

        [Test]
        public void VerifyExcludingADateTimePeriod()
        {
            var startTime = new DateTime(2011,1,1,8,0,0,DateTimeKind.Utc);
            var endTime = new DateTime(2011,1,1,18,0,0,DateTimeKind.Utc);
            var dateTimePeriod = new DateTimePeriod(startTime, endTime);

            var excludeStartTime = new DateTime(2011, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var excludeEndTime = new DateTime(2011, 1, 1, 18, 0, 0, DateTimeKind.Utc);
            var excludeDateTimePeriod = new DateTimePeriod(excludeStartTime, excludeEndTime);

            var result = dateTimePeriod.ExcludeDateTimePeriod(excludeDateTimePeriod);
            Assert.AreEqual(1, result.Count());
            var resultDateTimePeriod = new DateTimePeriod(startTime, excludeStartTime);
            Assert.AreEqual(resultDateTimePeriod, result.FirstOrDefault());

            excludeStartTime = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            excludeEndTime = new DateTime(2011, 1, 1, 18, 0, 0, DateTimeKind.Utc);
            excludeDateTimePeriod = new DateTimePeriod(excludeStartTime, excludeEndTime);

            result = dateTimePeriod.ExcludeDateTimePeriod(excludeDateTimePeriod);
            Assert.AreEqual(0, result.Count());

            excludeStartTime = new DateTime(2011, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            excludeEndTime = new DateTime(2011, 1, 1, 16, 0, 0, DateTimeKind.Utc);
            excludeDateTimePeriod = new DateTimePeriod(excludeStartTime, excludeEndTime);
            result = dateTimePeriod.ExcludeDateTimePeriod(excludeDateTimePeriod);
            var resultDateTimePeriod1 = new DateTimePeriod(startTime, excludeStartTime);
            var resultDateTimePeriod2 = new DateTimePeriod(excludeEndTime, endTime);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(resultDateTimePeriod1, result.FirstOrDefault());
            Assert.AreEqual(resultDateTimePeriod2, result.LastOrDefault());
        }
    }
}