using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;


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
            _requestedPeriod = new DateTimePeriod(new DateTime(2010, 04, 05, 04, 00, 00, DateTimeKind.Utc), new DateTime(2010, 05, 03, 04, 00, 00, DateTimeKind.Utc));
            _target = new SchedulerRangeToLoadCalculator(_requestedPeriod);
           
            var startDate = new DateOnly(2009, 6, 1);
            _person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Week, 4));
            //when rolling this period, a new one starts 2010-04-05

            //asking for a period on the person we want the period minus 7 days as the start and the period plus 7 days as the end
            //start in summer time
	        var expectedStart = _requestedPeriod.ChangeStartTime(TimeSpan.FromDays(-7)).StartDateTime;
            // end in summer time
	        var expectedEnd = _requestedPeriod.ChangeEndTime(TimeSpan.FromDays(7)).EndDateTime;

            var expectedPeriod = new DateTimePeriod(expectedStart, expectedEnd);
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
			var firstDateInWeek = DateHelper.GetFirstDateInWeek(startDate.Date, _person.PermissionInformation.Culture());

            _person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Month, 1));
            DateTimePeriod expectedPeriod =
                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
					new DateTime(firstDateInWeek.Year, firstDateInWeek.Month, firstDateInWeek.Day, 0, 0, 0, DateTimeKind.Local),
                    new DateTime(2008, 06, 08, 0, 0, 0, DateTimeKind.Local),
                    _person.PermissionInformation.DefaultTimeZone());
            Assert.AreEqual(expectedPeriod, _target.SchedulerRangeToLoad(_person));
        }

        [Test]
        public void VerifyResultIsCached()
        {
            var startDate = new DateOnly(2008, 5, 1);
			var firstDateInWeek = DateHelper.GetFirstDateInWeek(startDate.Date, _person.PermissionInformation.Culture());

            _person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Month, 1));
            var expectedPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                    new DateTime(firstDateInWeek.Year, firstDateInWeek.Month, firstDateInWeek.Day, 0, 0, 0, DateTimeKind.Local),
                    new DateTime(2008, 06, 08, 0, 0, 0, DateTimeKind.Local),
                    _person.PermissionInformation.DefaultTimeZone());
            Assert.AreEqual(expectedPeriod, _target.SchedulerRangeToLoad(_person));
            _person.RemoveAllSchedulePeriods();
            Assert.AreEqual(expectedPeriod, _target.SchedulerRangeToLoad(_person));
        }

        [Test]
        public void VerifyGetSchedulerRangeToLoadWithSamePeriodRolling()
        {
            var startDate = new DateOnly(2008, 5, 2);
            _person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Month, 1));
            _requestedPeriod = new DateTimePeriod(new DateTime(2008, 06, 08, 00, 00, 00, DateTimeKind.Utc), new DateTime(2008, 08, 10, 00, 00, 00, DateTimeKind.Utc));
            var expectedPeriod =
                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                    TimeZoneInfo.ConvertTimeFromUtc(_requestedPeriod.StartDateTime, _timeZoneInfo).AddDays(-7),
                    new DateTime(2008, 09, 14, 0, 0, 0, DateTimeKind.Local),
                    _person.PermissionInformation.DefaultTimeZone());
            _target = new SchedulerRangeToLoadCalculator(_requestedPeriod);
            Assert.AreEqual(expectedPeriod, _target.SchedulerRangeToLoad(_person));
        }

        [Test]
        public void VerifyRequestedPeriodStartsBeforeFirstPersonPeriod()
        {
            var startDate = new DateOnly(2008, 5, 2);
            _person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Month, 1));
            _requestedPeriod = new DateTimePeriod(2008,1,1,2008,8,10);
            var expectedPeriod =
                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                    TimeZoneInfo.ConvertTimeFromUtc(_requestedPeriod.StartDateTime, _timeZoneInfo).AddDays(-7),
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
                    TimeZoneInfo.ConvertTimeFromUtc(_requestedPeriod.StartDateTime, _timeZoneInfo).AddDays(-7),
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
		public void VerifyGetSchedulerRangeToLoadCanIncludeFirstAndLastFullWeek()
		{
			_timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));//GMT-3
			_person.PermissionInformation.SetCulture(CultureInfo.CreateSpecificCulture("en-GB"));
			_person.PermissionInformation.SetDefaultTimeZone(_timeZoneInfo);

			var startDate = new DateOnly(2009, 11, 1);
			_person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Month, 1));
			var expectedPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2009, 10, 26, 0, 0, 0, DateTimeKind.Local),new DateTime(2009, 12, 14, 0, 0, 0, DateTimeKind.Local),_person.PermissionInformation.DefaultTimeZone());
			_requestedPeriod = new DateTimePeriod(2009, 11, 3, 2009, 11, 3);
			_target = new SchedulerRangeToLoadCalculator(_requestedPeriod);
			
			Assert.AreEqual(expectedPeriod, _target.SchedulerRangeToLoad(_person));
		}

		[Test]
		public void VerifyGetSchedulerRangeToLoadCanIncludeFirstAndLastFullWeek1()
		{
			_timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));//GMT+1
			_person.PermissionInformation.SetCulture(CultureInfo.CreateSpecificCulture("en-GB"));
			_person.PermissionInformation.SetDefaultTimeZone(_timeZoneInfo);

			var startDate = new DateOnly(2009, 11, 1);
			_person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Month, 1));
			var expectedPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2009, 10, 26, 0, 0, 0, DateTimeKind.Local),new DateTime(2009, 12, 14, 0, 0, 0, DateTimeKind.Local),_person.PermissionInformation.DefaultTimeZone());
			_requestedPeriod = new DateTimePeriod(2009, 11, 3, 2009, 11, 3);
			_target = new SchedulerRangeToLoadCalculator(_requestedPeriod);
			
			Assert.AreEqual(expectedPeriod, _target.SchedulerRangeToLoad(_person));
		}

	    [Test]
	    public void Bug36032ShouldReturnCorrectPeriod()
	    {
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var schedulePeriod1 = new SchedulePeriod(new DateOnly(2009, 2, 2), SchedulePeriodType.Week, 4);
			person.AddSchedulePeriod(schedulePeriod1);
			var schedulePeriod2 = new SchedulePeriod(new DateOnly(2011, 2, 7), SchedulePeriodType.Week, 4);
			person.AddSchedulePeriod(schedulePeriod2);
			var schedulePeriod3 = new SchedulePeriod(new DateOnly(2013, 2, 4), SchedulePeriodType.Week, 4);
			person.AddSchedulePeriod(schedulePeriod3);

			_requestedPeriod = new DateTimePeriod(2015, 11, 30, 2015, 12, 27);
			_target = new SchedulerRangeToLoadCalculator(_requestedPeriod);
		    var x = _target.SchedulerRangeToLoad(person);
			x.StartDateTime.Should().Be.LessThan(new DateTime(2015, 11, 9));
			x.EndDateTime.Should().Be.GreaterThan(new DateTime(2016, 1, 3));
	    }
    }
}