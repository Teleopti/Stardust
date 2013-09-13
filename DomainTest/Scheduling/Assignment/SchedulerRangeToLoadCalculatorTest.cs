using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class SchedulerRangeToLoadCalculatorTest
    {
        private ISchedulerRangeToLoadCalculator _target;
        private IPerson _person;
        private DateTimePeriod _requestedPeriod;
        private TimeZoneInfo _timeZoneInfo;
        
        [SetUp]
        public void Setup()
        {
            _timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));//GMT-3
            _person = PersonFactory.CreatePerson();
            _person.PermissionInformation.SetCulture(CultureInfo.CreateSpecificCulture("en-US"));
            _person.PermissionInformation.SetDefaultTimeZone(_timeZoneInfo); 
            _requestedPeriod = new DateTimePeriod(new DateTime(2008, 05, 08, 03, 00, 00, DateTimeKind.Utc), new DateTime(2008, 05, 10, 03, 00, 00, DateTimeKind.Utc));
            _target = new SchedulerRangeToLoadCalculator(_requestedPeriod);
        }

        [Test]
        public void CanGetCorrectPeriodWithFourWeekSchedulePeriod()
        {
            _timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));//GMT-4
            _person.PermissionInformation.SetCulture(CultureInfo.CreateSpecificCulture("sv-SE"));
            _person.PermissionInformation.SetDefaultTimeZone(_timeZoneInfo);
            _requestedPeriod = new DateTimePeriod(new DateTime(2010, 04, 05, 04, 00, 00, DateTimeKind.Utc), new DateTime(2010, 05, 03 , 04, 00, 00, DateTimeKind.Utc));
            _target = new SchedulerRangeToLoadCalculator(_requestedPeriod);

            DateOnly startDate = new DateOnly(2009, 6, 1);
            _person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Week, 4));
            //when rolling this period, a new one starts 2010-04-05
        
            //asking for a period on the person we want the period minus 28 days as the start and the period plus 7 days as the end
            //start in winter time
            DateTime expectedStart = new DateTime(2010,3,8,5,0,0,DateTimeKind.Utc);
            // end in summer time
            DateTime expectedEnd = new DateTime(2010, 5, 10, 4, 0, 0, DateTimeKind.Utc);

            DateTimePeriod expectedPeriod = new DateTimePeriod(expectedStart,expectedEnd);
            Assert.AreEqual(expectedPeriod,_target.SchedulerRangeToLoad(_person));
        }
        
        [Test]
        public void CanGetCorrectPeriodWithFourWeekSchedulePeriodAndNoJusticeLoaded()
        {
            _timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));//GMT-4
            _person.PermissionInformation.SetCulture(CultureInfo.CreateSpecificCulture("sv-SE"));
            _person.PermissionInformation.SetDefaultTimeZone(_timeZoneInfo);
            _requestedPeriod = new DateTimePeriod(new DateTime(2010, 04, 05, 04, 00, 00, DateTimeKind.Utc), new DateTime(2010, 05, 03, 04, 00, 00, DateTimeKind.Utc));
            _target = new SchedulerRangeToLoadCalculator(_requestedPeriod);
            _target.JusticeValue = 0;
            DateOnly startDate = new DateOnly(2009, 6, 1);
            _person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Week, 4));
            //when rolling this period, a new one starts 2010-04-05

            //asking for a period on the person we want the period minus 7 days as the start and the period plus 7 days as the end
            //start in summer time
            DateTime expectedStart = new DateTime(2010, 3, 29, 4, 0, 0, DateTimeKind.Utc);
            // end in summer time
            DateTime expectedEnd = new DateTime(2010, 5, 10, 4, 0, 0, DateTimeKind.Utc);

            DateTimePeriod expectedPeriod = new DateTimePeriod(expectedStart, expectedEnd);
            Assert.AreEqual(expectedPeriod, _target.SchedulerRangeToLoad(_person));
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_requestedPeriod, _target.RequestedPeriod);
        }

        [Test]
        public void VerifyGetSchedulerRangeToLoad()
        {
            DateOnly startDate = new DateOnly(2008, 5, 1);
            _person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Month, 1));
            DateTimePeriod expectedPeriod =
                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                    new DateTime(2008, 04, 10, 0, 0, 0, DateTimeKind.Local),
                    new DateTime(2008, 06, 08, 0, 0, 0, DateTimeKind.Local),
                    _person.PermissionInformation.DefaultTimeZone());
            Assert.AreEqual(expectedPeriod, _target.SchedulerRangeToLoad(_person));
        }

        [Test]
        public void VerifyResultIsCached()
        {
            DateOnly startDate = new DateOnly(2008, 5, 1);
            _person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Month, 1));
            DateTimePeriod expectedPeriod =
                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                    new DateTime(2008, 04, 10, 0, 0, 0, DateTimeKind.Local),
                    new DateTime(2008, 06, 08, 0, 0, 0, DateTimeKind.Local),
                    _person.PermissionInformation.DefaultTimeZone());
            //expectedPeriod = expectedPeriod.ChangeStartTime(new TimeSpan(-28, 0, 0, 0));
            Assert.AreEqual(expectedPeriod, _target.SchedulerRangeToLoad(_person));
            _person.RemoveAllSchedulePeriods();
            Assert.AreEqual(expectedPeriod, _target.SchedulerRangeToLoad(_person));
        }

        [Test]
        public void VerifyGetSchedulerRangeToLoadWithSamePeriodRolling()
        {
            DateOnly startDate = new DateOnly(2008, 5, 2);
            _person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Month, 1));
            _requestedPeriod = new DateTimePeriod(new DateTime(2008, 06, 08, 00, 00, 00, DateTimeKind.Utc), new DateTime(2008, 08, 10, 00, 00, 00, DateTimeKind.Utc));
            DateTimePeriod expectedPeriod =
                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                    TimeZoneInfo.ConvertTimeFromUtc(_requestedPeriod.StartDateTime, _timeZoneInfo).AddDays(-28),
                    new DateTime(2008, 09, 14, 0, 0, 0, DateTimeKind.Local),
                    _person.PermissionInformation.DefaultTimeZone());
            _target = new SchedulerRangeToLoadCalculator(_requestedPeriod);
            Assert.AreEqual(expectedPeriod, _target.SchedulerRangeToLoad(_person));
        }

        [Test]
        public void VerifyRequestedPeriodStartsBeforeFirstPersonPeriod()
        {
            DateOnly startDate = new DateOnly(2008, 5, 2);
            _person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Month, 1));
            _requestedPeriod = new DateTimePeriod(2008,1,1,2008,8,10);
            DateTimePeriod expectedPeriod =
                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                    TimeZoneInfo.ConvertTimeFromUtc(_requestedPeriod.StartDateTime, _timeZoneInfo).AddDays(-28),
                    new DateTime(2008, 09, 14, 0, 0, 0, DateTimeKind.Local),
                    _person.PermissionInformation.DefaultTimeZone());
            _target = new SchedulerRangeToLoadCalculator(_requestedPeriod);
            Assert.AreEqual(expectedPeriod, _target.SchedulerRangeToLoad(_person));
        }

        [Test]
        public void VerifyTerminatedPersonHandledCorrectly()
        {
            DateOnly startDate = new DateOnly(2008, 5, 2);
            _person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Month, 1));
            _person.TerminatePerson(new DateOnly(2008,5,5), new MockRepository().StrictMock<IPersonAccountUpdater>());
            _requestedPeriod = new DateTimePeriod(2008, 5, 3, 2008, 5, 10);
          
            _target = new SchedulerRangeToLoadCalculator(_requestedPeriod);

            DateTimePeriod expectedPeriod =
                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                    TimeZoneInfo.ConvertTimeFromUtc(_requestedPeriod.StartDateTime, _timeZoneInfo).AddDays(-28),
                    new DateTime(2008, 05, 18, 00, 00, 00, DateTimeKind.Local),
                    _person.PermissionInformation.DefaultTimeZone());
            Assert.AreEqual(expectedPeriod, _target.SchedulerRangeToLoad(_person));
        }

        [Test]
        public void VerifyRequestedPeriodContainsNoValidPeriodsReturnsRequestedPeriod()
        {   //requested period is from 10/5 so in my opinion null return is correct
            DateOnly startDate = new DateOnly(2008, 5, 2);
            _person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Month, 1));
            _person.TerminatePerson(new DateOnly(2008, 5, 5), new MockRepository().StrictMock<IPersonAccountUpdater>());

            Assert.AreEqual(_requestedPeriod, _target.SchedulerRangeToLoad(_person));
        }

        [Test]
        public void VerifyGetSchedulerRangeToLoadUsesJusticeCalculatorWindow()
        {
            DateOnly startDate = new DateOnly(2008, 5, 2);
            _person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Month, 1));
            DateTimePeriod expectedPeriod =
                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                    TimeZoneInfo.ConvertTimeFromUtc(_requestedPeriod.StartDateTime, _timeZoneInfo).AddDays(-28),
                    new DateTime(2008, 06, 15, 0, 0, 0, DateTimeKind.Local),
                    _person.PermissionInformation.DefaultTimeZone());
            _target = new SchedulerRangeToLoadCalculator(_requestedPeriod);
            Assert.AreEqual(expectedPeriod.StartDateTime, _target.SchedulerRangeToLoad(_person).StartDateTime);
            Assert.AreEqual(expectedPeriod.EndDateTime, _target.SchedulerRangeToLoad(_person).EndDateTime);
        }

        [Test]
        public void VerifyGetSchedulerRangeToLoadUsesChangedJusticeCalculatorWindow()
        {
            DateOnly startDate = new DateOnly(2008, 5, 2);
            _person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Month, 1));
            DateTimePeriod expectedPeriod =
                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                    TimeZoneInfo.ConvertTimeFromUtc(_requestedPeriod.StartDateTime, _timeZoneInfo).AddDays(-20),
                    new DateTime(2008, 06, 15, 0, 0, 0, DateTimeKind.Local),
                    _person.PermissionInformation.DefaultTimeZone());
            _target = new SchedulerRangeToLoadCalculator(_requestedPeriod);
            _target.JusticeValue = -20;
            Assert.AreEqual(expectedPeriod.StartDateTime, _target.SchedulerRangeToLoad(_person).StartDateTime);
            Assert.AreEqual(expectedPeriod.EndDateTime, _target.SchedulerRangeToLoad(_person).EndDateTime);
        }

        [Test]
        public void VerifyGetSchedulerRangeToLoadCanIncludeFirstAndLastFullWeek()
        {
            _timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));//GMT-3
            _person.PermissionInformation.SetCulture(CultureInfo.CreateSpecificCulture("en-GB"));
            _person.PermissionInformation.SetDefaultTimeZone(_timeZoneInfo); 

            DateOnly startDate = new DateOnly(2009, 11, 1);
            _person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Month, 1));
            DateTimePeriod expectedPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                new DateTime(2009, 10, 26, 0, 0, 0, DateTimeKind.Local),
                new DateTime(2009, 12, 14, 0, 0, 0, DateTimeKind.Local),
                _person.PermissionInformation.DefaultTimeZone());
            _requestedPeriod = new DateTimePeriod(2009,11,3,2009,11,3);
            _target = new SchedulerRangeToLoadCalculator(_requestedPeriod);
            _target.JusticeValue = 0;
            Assert.AreEqual(expectedPeriod, _target.SchedulerRangeToLoad(_person));
        }

        [Test]
        public void VerifyGetSchedulerRangeToLoadCanIncludeFirstAndLastFullWeek1()
        {
            _timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));//GMT+1
            _person.PermissionInformation.SetCulture(CultureInfo.CreateSpecificCulture("en-GB"));
            _person.PermissionInformation.SetDefaultTimeZone(_timeZoneInfo);

            DateOnly startDate = new DateOnly(2009, 11, 1);
            _person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Month, 1));
            DateTimePeriod expectedPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                new DateTime(2009, 10, 26, 0, 0, 0, DateTimeKind.Local),
                new DateTime(2009, 12, 14, 0, 0, 0, DateTimeKind.Local),
                _person.PermissionInformation.DefaultTimeZone());
            _requestedPeriod = new DateTimePeriod(2009, 11, 3, 2009, 11, 3);
            _target = new SchedulerRangeToLoadCalculator(_requestedPeriod);
            _target.JusticeValue = 0;
            Assert.AreEqual(expectedPeriod, _target.SchedulerRangeToLoad(_person));
        }
    }
}