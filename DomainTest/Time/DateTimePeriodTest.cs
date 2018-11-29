using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Time
{
    [TestFixture]
	public class DateTimePeriodTest
    {
        private readonly DateTime _start = new DateTime(2007, 06, 01, 12, 31, 0, DateTimeKind.Utc);
        private readonly DateTime _end = new DateTime(2008, 02, 28, 0, 0, 0, DateTimeKind.Utc);
        private DateTimePeriod _period;
        private TimeZoneInfo _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");

		[SetUp]
        public void TestSetup()
        {
            _period = new DateTimePeriod(_start, _end);
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
                _period.TimePeriod((TimeZoneInfo.Utc)));
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
                _period.TimePeriod((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"))));
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
        public void VerifyStartDateTimeEqualOrBeforeEndDateTime()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _period = new DateTimePeriod(_end, _start));
        }

        /// <summary>
        /// Verifies that the start DateTime is not later than end DateTime
        /// </summary>
        [Test]
        public void VerifyStartDateTimeEqualOrBeforeEndDateTimeWhenUsingSixParameters()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _period = new DateTimePeriod(2001, 1, 1, 2000, 1, 1));
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
        public void VerifyPeriodTimeSpanWorks()
        {
            _period = new DateTimePeriod(2006, 1, 1, 2006, 1, 10);
            Assert.AreEqual(TimeSpan.FromDays(9), _period.ElapsedTime());
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
		
        [Test]
        public void VerifyDateTimeLocalWorks()
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfoFactory.StockholmTimeZoneInfo();
            DateTime expectedLocalStart =
                TimeZoneInfo.ConvertTimeFromUtc(_start, timeZoneInfo);
            DateTime expectedLocalEnd =
                TimeZoneInfo.ConvertTimeFromUtc(_end, timeZoneInfo);

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

            _period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTime, endDateTime, TimeZoneInfoFactory.StockholmTimeZoneInfo());

            IList<IntervalDefinition> list = _period.IntervalsFromHourCollection(15, TimeZoneInfoFactory.StockholmTimeZoneInfo());

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

            _period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTime, endDateTime, TimeZoneInfoFactory.StockholmTimeZoneInfo());

            IList<IntervalDefinition> list = _period.IntervalsFromHourCollection(15, TimeZoneInfoFactory.StockholmTimeZoneInfo());

            Assert.AreEqual(23, list.Count);
            Assert.AreEqual(23, list[0].TimeSpan.Hours);
            Assert.AreEqual(30, list[22].TimeSpan.Minutes);
        }

        [Test]
        public void VerifyEmptyDateTimePeriodOnIntervals()
        {
            _period = new DateTimePeriod();

            IList<IntervalDefinition> list = _period.IntervalsFromHourCollection(15, TimeZoneInfoFactory.StockholmTimeZoneInfo());

            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void VerifyToSmallResolutionIsNotAllowedOnIntervalCollection()
        {
            DateTime startDateTime = new DateTime(2007, 10, 27, 23, 0, 0);
            DateTime endDateTime = new DateTime(2007, 10, 28, 3, 45, 0);

            _period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTime, endDateTime, TimeZoneInfoFactory.StockholmTimeZoneInfo());
            Assert.Throws<ArgumentException>(() => _period.IntervalsFromHourCollection(0, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
        }

        [Test]
        public void VerifyHighResolutionIsAllowedOnIntervalCollection()
        {
            DateTime startDateTime = new DateTime(2007, 10, 27, 23, 0, 0);
            DateTime endDateTime = new DateTime(2007, 10, 27, 23, 0, 0);

            _period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTime, endDateTime, TimeZoneInfoFactory.StockholmTimeZoneInfo());
            _period.IntervalsFromHourCollection(60, TimeZoneInfoFactory.StockholmTimeZoneInfo());
        }

        [Test]
        public void VerifyHigherResolutionIsAllowedOnIntervalCollection()
        {
            DateTime startDateTime = new DateTime(2007, 10, 26, 23, 0, 0);
            DateTime endDateTime = new DateTime(2007, 10, 27, 23, 0, 0);

            _period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTime, endDateTime, TimeZoneInfoFactory.StockholmTimeZoneInfo());
            _period.IntervalsFromHourCollection(120, TimeZoneInfoFactory.StockholmTimeZoneInfo());
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

            IList<DateTimePeriod> wholeDays = _period.WholeDayCollection(TimeZoneInfo.Utc );

            Assert.AreEqual(4, wholeDays.Count);
            Assert.AreEqual(new TimeSpan(1, 30, 0), wholeDays[3].ElapsedTime());
        }

        [Test]
        public void VerifyDateTimePeriodCanReturn24HourChunksDuringDst()
        {
            DateTime startDate1 = new DateTime(2008, 10, 25, 12, 00, 0, DateTimeKind.Utc);
            DateTime endDate1 = new DateTime(2008, 10, 26, 13, 30, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(startDate1, endDate1);

			var stockholmTimeZoneInfo = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			IList<DateTimePeriod> wholeDays = _period.WholeDayCollection(stockholmTimeZoneInfo);

            Assert.AreEqual(2, wholeDays.Count);
            Assert.AreEqual(new TimeSpan(0, 30, 0), wholeDays[1].ElapsedTime());
	        Assert.AreEqual(wholeDays[0].StartDateTimeLocal(stockholmTimeZoneInfo).TimeOfDay,
		        wholeDays[1].StartDateTimeLocal(stockholmTimeZoneInfo).TimeOfDay);

            startDate1 = new DateTime(2008, 3, 29, 12, 00, 0, DateTimeKind.Utc);
            endDate1 = new DateTime(2008, 3, 30, 13, 30, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(startDate1, endDate1);

            wholeDays = _period.WholeDayCollection(stockholmTimeZoneInfo);

            Assert.AreEqual(2, wholeDays.Count);
            Assert.AreEqual(new TimeSpan(2, 30, 0), wholeDays[1].ElapsedTime());
	        Assert.AreEqual(wholeDays[0].StartDateTimeLocal(stockholmTimeZoneInfo).TimeOfDay,
		        wholeDays[1].StartDateTimeLocal(stockholmTimeZoneInfo).TimeOfDay);
        }

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
        public void VerifyAdjacent()
        {
            DateTime start = new DateTime(2007, 6, 1, 23, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(2007, 6, 2, 1, 0, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(start, end);

            //adjacent with start date equals end date (no offset)
            DateTime start1 = new DateTime(2007, 6, 1, 22, 0, 0, DateTimeKind.Utc);
            DateTime end1 = new DateTime(2007, 6, 1, 23, 0, 0, DateTimeKind.Utc);
            DateTimePeriod per1 = new DateTimePeriod(start1, end1);
            Assert.IsTrue(_period.AdjacentTo(per1));

            //outside
            start1 = new DateTime(2007, 5, 1, 0, 0, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 5, 1, 1, 0, 0, DateTimeKind.Utc);
            per1 = new DateTimePeriod(start1, end1);
            Assert.IsFalse(_period.AdjacentTo(per1));
            Assert.IsFalse(per1.AdjacentTo(_period));

            //intersect
            start1 = new DateTime(2007, 6, 1, 22, 30, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 1, 23, 30, 0, DateTimeKind.Utc);
            per1 = new DateTimePeriod(start1, end1);
            Assert.IsFalse(_period.AdjacentTo(per1));

            //equals
            start1 = new DateTime(2007, 6, 1, 23, 0, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 2, 1, 0, 0, DateTimeKind.Utc);
            per1 = new DateTimePeriod(start1, end1);
            Assert.IsFalse(_period.AdjacentTo(per1));

            //inside
            start1 = new DateTime(2007, 6, 1, 23, 30, 0, DateTimeKind.Utc);
            end1 = new DateTime(2007, 6, 2, 0, 30, 0, DateTimeKind.Utc);
            per1 = new DateTimePeriod(start1, end1);
            Assert.IsFalse(_period.AdjacentTo(per1));
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
        public void VerifyMergedList1()
        {
            _period = new DateTimePeriod(2004, 1, 1, 2009, 1, 1);
            DateTimePeriod dtp2 = new DateTimePeriod(2015, 1, 1, 2026, 1, 1);
            DateTimePeriod dtp3 = new DateTimePeriod(2032, 1, 1, 2034, 1, 1);
            DateTimePeriod dtp4 = new DateTimePeriod(2039, 1, 1, 2066, 1, 1);

            DateTimePeriod dtp5 = new DateTimePeriod(2001, 1, 1, 2006, 1, 1);
            DateTimePeriod dtp6 = new DateTimePeriod(2011, 1, 1, 2043, 1, 1);
            DateTimePeriod dtp7 = new DateTimePeriod(2069, 1, 1, 2070, 1, 1);
            IList<DateTimePeriod> list = new List<DateTimePeriod>();

            list.Add(_period);
            list.Add(dtp2);
            list.Add(dtp3);
            list.Add(dtp4);
            list.Add(dtp5);
            list.Add(dtp6);
            list.Add(dtp7);

	        DateTimePeriod.MergePeriods(list).Count()
	                      .Should().Be.EqualTo(3);
        }

			[Test]
			public void ShouldCreateEmptyListWhenMergingEmpty()
			{
				DateTimePeriod.MergePeriods(Enumerable.Empty<DateTimePeriod>())
				              .Should().Be.Empty();
			}

        [Test]
        public void VerifyMergedListNoIntersect()
        {
            _period = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
            DateTimePeriod dtp2 = new DateTimePeriod(2000, 1, 3, 2000, 1, 4);
            DateTimePeriod dtp3 = new DateTimePeriod(2000, 1, 5, 2000, 1, 6);
            DateTimePeriod dtp4 = new DateTimePeriod(2000, 1, 7, 2000, 1, 8);
            IList<DateTimePeriod> list = new List<DateTimePeriod>();

            list.Add(_period);
            list.Add(dtp2);
            list.Add(dtp3);
            list.Add(dtp4);

            var ret = new List<DateTimePeriod>(DateTimePeriod.MergePeriods(list));
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
            var list = new List<DateTimePeriod>();

            list.Add(_period);
            list.Add(dtp2);
            list.Add(dtp3);
            list.Add(dtp4);

            var ret = DateTimePeriod.MergePeriods(list).ToList();
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
            IList<DateTimePeriod> list = new List<DateTimePeriod>();

            list.Add(_period);
            list.Add(dtp2);
            list.Add(dtp3);
            list.Add(dtp4);

            var ret = DateTimePeriod.MergePeriods(list);
            Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2000, 1, 10), ret.Single());
        }

        [Test]
        public void VerifyMergedListEqualLists()
        {
            _period = new DateTimePeriod(2000, 1, 1, 2000, 1, 5);
            DateTimePeriod dtp2 = new DateTimePeriod(2000, 1, 1, 2000, 1, 5);

            IList<DateTimePeriod> list = new List<DateTimePeriod>();

            list.Add(_period);
            list.Add(dtp2);

            var ret = DateTimePeriod.MergePeriods(list);
            Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2000, 1, 5), ret.Single());
        }

        [Test]
        public void VerifyToDateOnlyPeriod()
        {
            DateOnly startDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(_start, _timeZoneInfo));
            DateOnly endDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(_end, _timeZoneInfo));
            DateOnlyPeriod dateOnlyPeriod = _period.ToDateOnlyPeriod(_timeZoneInfo);
            Assert.AreEqual(dateOnlyPeriod.StartDate, startDate);
            Assert.AreEqual(dateOnlyPeriod.EndDate, endDate);
        }

        [Test]
        public void VerifyToDateOnlyPeriodWorksBothWays()
        {
            DateOnly startDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(_start, _timeZoneInfo));
            DateOnly endDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(_end, _timeZoneInfo));
            DateOnlyPeriod dateOnlyPeriod = _period.ToDateOnlyPeriod(_timeZoneInfo);
            Assert.AreEqual(dateOnlyPeriod.StartDate, startDate);
            Assert.AreEqual(dateOnlyPeriod.EndDate, endDate);
            DateTimePeriod dateTimePeriod = dateOnlyPeriod.ToDateTimePeriod(_timeZoneInfo);
            DateOnlyPeriod dateOnlyPeriodAfterBeingDateTimePeriod = dateTimePeriod.ToDateOnlyPeriod(_timeZoneInfo);
            Assert.AreEqual(dateOnlyPeriodAfterBeingDateTimePeriod.StartDate, startDate);
            Assert.AreEqual(dateOnlyPeriodAfterBeingDateTimePeriod.EndDate, endDate);
        }

        [Test]
        public void VerifyToDateOnlyPeriodWorksBothWaysWithOneDate()
        {
            DateTime dateTime = new DateTime(2007,12,31,23,0,0,DateTimeKind.Utc);
            _timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            DateOnly startDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(dateTime, _timeZoneInfo));
            DateOnly endDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(dateTime, _timeZoneInfo));
            _period = new DateTimePeriod(dateTime,dateTime);
            DateOnlyPeriod dateOnlyPeriod = _period.ToDateOnlyPeriod(_timeZoneInfo);
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

            var result = dateTimePeriod.Subtract(excludeDateTimePeriod);
            Assert.AreEqual(1, result.Count());
            var resultDateTimePeriod = new DateTimePeriod(startTime, excludeStartTime);
            Assert.AreEqual(resultDateTimePeriod, result.FirstOrDefault());

            excludeStartTime = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            excludeEndTime = new DateTime(2011, 1, 1, 18, 0, 0, DateTimeKind.Utc);
            excludeDateTimePeriod = new DateTimePeriod(excludeStartTime, excludeEndTime);

            result = dateTimePeriod.Subtract(excludeDateTimePeriod);
            Assert.AreEqual(0, result.Count());

            excludeStartTime = new DateTime(2011, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            excludeEndTime = new DateTime(2011, 1, 1, 16, 0, 0, DateTimeKind.Utc);
            excludeDateTimePeriod = new DateTimePeriod(excludeStartTime, excludeEndTime);
            result = dateTimePeriod.Subtract(excludeDateTimePeriod);
            var resultDateTimePeriod1 = new DateTimePeriod(startTime, excludeStartTime);
            var resultDateTimePeriod2 = new DateTimePeriod(excludeEndTime, endTime);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(resultDateTimePeriod1, result.FirstOrDefault());
            Assert.AreEqual(resultDateTimePeriod2, result.LastOrDefault());
        }

        [Test]
        public void VerifyTheDayCollectionToReturnCorrectCollectionIncaseOfDayLightSaving()
        {
            var timeZoneInfo = TimeZoneInfoFactory.BrazilTimeZoneInfo();
            var  dateTimePeriod = new DateTimePeriod(new DateTime(2013,02,13,0,0,0,DateTimeKind.Utc  ),new DateTime(2013,02,20,0,0,0,DateTimeKind.Utc) );
            var returnList = dateTimePeriod.WholeDayCollection(timeZoneInfo);
            Assert.AreEqual(returnList.Count(), 7);
            Assert.AreEqual(returnList[0], new DateTimePeriod(new DateTime(2013, 02, 13, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 02, 14, 0, 0, 0, DateTimeKind.Utc)));
            Assert.AreEqual(returnList[1], new DateTimePeriod(new DateTime(2013, 02, 14, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 02, 15, 0, 0, 0, DateTimeKind.Utc)));
            Assert.AreEqual(returnList[2], new DateTimePeriod(new DateTime(2013, 02, 15, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 02, 16, 0, 0, 0, DateTimeKind.Utc)));
            Assert.AreEqual(returnList[3], new DateTimePeriod(new DateTime(2013, 02, 16, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 02, 17, 0, 0, 0, DateTimeKind.Utc)));
            Assert.AreEqual(returnList[4], new DateTimePeriod(new DateTime(2013, 02, 17, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 02, 18, 1, 0, 0, DateTimeKind.Utc)));
            Assert.AreEqual(returnList[5], new DateTimePeriod(new DateTime(2013, 02, 18, 1, 0, 0, DateTimeKind.Utc), new DateTime(2013, 02, 19, 1, 0, 0, DateTimeKind.Utc)));
            Assert.AreEqual(returnList[6], new DateTimePeriod(new DateTime(2013, 02, 19, 1, 0, 0, DateTimeKind.Utc), new DateTime(2013, 02, 20, 0, 0, 0, DateTimeKind.Utc)));

        }

        [Test]
        public void VerifyTheDayCollectionToReturnCorrectCollection()
        {
            var timeZoneInfo = TimeZoneInfoFactory.BrazilTimeZoneInfo();
            var dateTimePeriod = new DateTimePeriod(new DateTime(2013, 02, 13, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 02, 15, 0, 0, 0, DateTimeKind.Utc));
            var returnList = dateTimePeriod.WholeDayCollection(timeZoneInfo);
            Assert.AreEqual(returnList.Count(), 2);
            Assert.AreEqual(returnList[0], new DateTimePeriod(new DateTime(2013, 02, 13, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 02, 14, 0, 0, 0, DateTimeKind.Utc)));
            Assert.AreEqual(returnList[1], new DateTimePeriod(new DateTime(2013, 02, 14, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 02, 15, 0, 0, 0, DateTimeKind.Utc)));
        }

			[Test]
			public void ShouldBehaveAsBeforeBecauseLegacyCodeIsDependantOnThisBehaviour()
			{
				_period = new DateTimePeriod(
								new DateTime(2014, 10, 26, 0, 45, 0, DateTimeKind.Utc),
								new DateTime(2014, 10, 26, 1, 0, 0, DateTimeKind.Utc));

				Assert.AreEqual(
						new TimePeriod(
								TimeSpan.FromHours(2).Add(
										TimeSpan.FromMinutes(45)),
								TimeSpan.FromHours(3)),
						_period.TimePeriod((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"))));
			}
	}
}