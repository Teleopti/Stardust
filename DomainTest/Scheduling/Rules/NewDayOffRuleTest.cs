using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class NewDayOffRuleTest
    {
        private NewDayOffRule _target;
        private MockRepository _mocks;
        private IScheduleDay _day;
        private IScheduleDay _dayBefore;
        private IScheduleDay _dayAfter;
        private IList<IScheduleDay> _days;
        private IList<IScheduleDay> _dayBeforeDayAfter;
        private IPerson _person;
        private IScheduleRange _scheduleRange;
        private IDictionary<IPerson, IScheduleRange> _dic;
        private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;

        private IActivity _activity;
        private ShiftCategory _category;
        private IScenario _scenario;
        private DateTime _start, _end;

        private IPersonAssignment _personAssignmentConflictingWithDayOffEnd;
        private IPersonAssignment _personAssignmentConflictingWithDayOffStart;
        private IPersonAssignment _personAssignmentJustAfterDayOff;
        private IPersonAssignment _personAssignmentJustBeforeDayOff;
        private PersonDayOff _personDayOff1;
        private TimeZoneInfo _timeZone;
    	private IWorkTimeStartEndExtractor _workTimeStartEndExtractor;
    	private DateTimePeriod _personAssignmentConflictingWithDayOffStartPeriod;
    	private DateTimePeriod _personAssignmentJustAfterDayOffPeriod;
    	private DateTimePeriod _personAssignmentConflictingWithDayOffEndPeriod;
    	private DateTimePeriod _personAssignmentJustBeforeDayOffPeriod;

    	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
			_mocks = new MockRepository();
			_workTimeStartEndExtractor = _mocks.StrictMock<IWorkTimeStartEndExtractor>();
			_target = new NewDayOffRule(_workTimeStartEndExtractor);
            _day = _mocks.StrictMock<IScheduleDay>();
            _days = new List<IScheduleDay> { _day };
            _dayBefore = _mocks.StrictMock<IScheduleDay>();
            _dayAfter = _mocks.StrictMock<IScheduleDay>();
            _dayBeforeDayAfter = new List<IScheduleDay> { _dayBefore,_day, _dayAfter };

            _timeZone = TimeZoneInfo.Utc;
            _person = PersonFactory.CreatePerson();
            _person.PermissionInformation.SetDefaultTimeZone(_timeZone);
           
            _scheduleRange = _mocks.StrictMock<IScheduleRange>();
            _dic = new Dictionary<IPerson, IScheduleRange> {{_person, _scheduleRange}};
            _dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2007, 8, 3), _timeZone);

            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _category = ShiftCategoryFactory.CreateShiftCategory("myCategory");
            _activity = ActivityFactory.CreateActivity("Phone");
            _start = new DateTime(2007, 8, 2, 8, 30, 0, DateTimeKind.Utc);
            _end = new DateTime(2007, 8, 2, 17, 30, 0, DateTimeKind.Utc);
            var dayOff = new DayOffTemplate(new Description("test"));
            dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(36), TimeSpan.FromHours(9));
            dayOff.Anchor = new TimeSpan(8, 30, 0);

            var dayOff1 = new DayOffTemplate(new Description("test"));
            dayOff1.SetTargetAndFlexibility(TimeSpan.FromHours(36), TimeSpan.FromHours(9));
            dayOff1.Anchor = new TimeSpan(10, 30, 0);
            _personDayOff1 = new PersonDayOff(_person, _scenario, dayOff1, new DateOnly(2007, 8, 3));

            // add this and the day off cannot be moved backwards
    		_personAssignmentJustBeforeDayOffPeriod = new DateTimePeriod(_start.AddHours(-3), _end.AddHours(-3));
            _personAssignmentJustBeforeDayOff = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
			  _activity, _person, _personAssignmentJustBeforeDayOffPeriod, _category, _scenario);

            // this will start 3 hours before the Day Off ends
    		_personAssignmentConflictingWithDayOffEndPeriod = new DateTimePeriod(_start.AddHours(24 + 17), _end.AddHours(24 + 17));
            _personAssignmentConflictingWithDayOffEnd = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
			  _activity, _person, _personAssignmentConflictingWithDayOffEndPeriod, _category, _scenario);

            // add this and the day off cannot be moved forward
    		_personAssignmentJustAfterDayOffPeriod = new DateTimePeriod(_start.AddHours(24 + 18), _end.AddHours(24 + 18));
            _personAssignmentJustAfterDayOff = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
			  _activity, _person, _personAssignmentJustAfterDayOffPeriod, _category, _scenario);

            // this will start just before the Day Off and end in it
    		_personAssignmentConflictingWithDayOffStartPeriod = new DateTimePeriod(_start.AddHours(2), _end.AddHours(2));
            _personAssignmentConflictingWithDayOffStart = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
              _activity, _person, _personAssignmentConflictingWithDayOffStartPeriod, _category, _scenario);


            Expect.Call(_day.Person).Return(_person);
            Expect.Call(_scheduleRange.BusinessRuleResponseInternalCollection).Return(new List<IBusinessRuleResponse>());
            Expect.Call(_day.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();

    		Expect.Call(_scheduleRange.ScheduledDayCollection(new DateOnlyPeriod(2007, 7, 31, 2007, 8, 6)))
    		      .Return(_dayBeforeDayAfter);

            Expect.Call(_dayBefore.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_dateOnlyAsDateTimePeriod.DateOnly.AddDays(-1), _timeZone));
            Expect.Call(_dayAfter.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_dateOnlyAsDateTimePeriod.DateOnly.AddDays(1), _timeZone));
        }

        [Test]
        public void CanAccessSimpleProperties()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual("", _target.ErrorMessage);
            Assert.IsTrue(_target.HaltModify);
            _target.HaltModify = false;
            Assert.IsFalse(_target.HaltModify);
        }

        [Test]
        public void WhenNoDayOffRuleReportsNoError()
        {
            Expect.Call(_dayBefore.PersonDayOffCollection()).Return(
                new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
            Expect.Call(_day.PersonDayOffCollection()).Return(
                new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
            Expect.Call(_dayAfter.PersonDayOffCollection()).Return(
                new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
            _mocks.ReplayAll();
            Assert.AreEqual(0, _target.Validate(_dic, _days).Count());
            _mocks.VerifyAll();
        }

        [Test]
        public void ValidatedFailsWhenAssignmentJustBeforeAndConflictingAssignmentAfter()
        {
            Expect.Call(_day.PersonDayOffCollection()).Return(
                new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { _personDayOff1 }));
            Expect.Call(_dayBefore.PersonAssignmentCollection()).Return(
                new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>
                                                              {_personAssignmentJustBeforeDayOff})).Repeat.Twice();
            Expect.Call(_dayAfter.PersonAssignmentCollection()).Return(
                new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>
                                                              {_personAssignmentConflictingWithDayOffEnd})).Repeat.Twice();
            Expect.Call(_dayBefore.PersonDayOffCollection()).Return(
               new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
            Expect.Call(_dayAfter.PersonDayOffCollection()).Return(
                new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
            Expect.Call(_day.PersonAssignmentCollection()).Return(
                new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>())).Repeat.Twice();

			Expect.Call(_workTimeStartEndExtractor.WorkTimeStart(null)).Return(
				_personAssignmentJustBeforeDayOffPeriod.StartDateTime).IgnoreArguments();
			Expect.Call(_workTimeStartEndExtractor.WorkTimeEnd(null)).Return(
				_personAssignmentJustBeforeDayOffPeriod.EndDateTime).IgnoreArguments();

			Expect.Call(_workTimeStartEndExtractor.WorkTimeStart(null)).Return(
				_personAssignmentConflictingWithDayOffEndPeriod.StartDateTime).IgnoreArguments();
			Expect.Call(_workTimeStartEndExtractor.WorkTimeEnd(null)).Return(
				_personAssignmentConflictingWithDayOffEndPeriod.EndDateTime).IgnoreArguments();

            _mocks.ReplayAll();

            var result = _target.Validate(_dic, _days);
            Assert.AreEqual(1,result.Count());
            _mocks.VerifyAll();
        }

        [Test]
        public void ValidatedFailsWhenAssignmentJustAfterAndConflictingAssignmentBefore()
        {
            Expect.Call(_day.PersonDayOffCollection()).Return(
                new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { _personDayOff1 }));
            Expect.Call(_dayBefore.PersonAssignmentCollection()).Return(
                new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { _personAssignmentConflictingWithDayOffStart })).Repeat.Twice();
            Expect.Call(_dayAfter.PersonAssignmentCollection()).Return(
                new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { _personAssignmentJustAfterDayOff })).Repeat.Twice();

            Expect.Call(_dayBefore.PersonDayOffCollection()).Return(
                new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> ()));
            Expect.Call(_dayAfter.PersonDayOffCollection()).Return(
                new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));

            Expect.Call(_day.PersonAssignmentCollection()).Return(
                new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>())).Repeat.Twice();

			Expect.Call(_workTimeStartEndExtractor.WorkTimeStart(null)).Return(
				_personAssignmentConflictingWithDayOffStartPeriod.StartDateTime).IgnoreArguments();
			Expect.Call(_workTimeStartEndExtractor.WorkTimeEnd(null)).Return(
				_personAssignmentConflictingWithDayOffStartPeriod.EndDateTime).IgnoreArguments();

			Expect.Call(_workTimeStartEndExtractor.WorkTimeStart(null)).Return(
				_personAssignmentJustAfterDayOffPeriod.StartDateTime).IgnoreArguments();
			Expect.Call(_workTimeStartEndExtractor.WorkTimeEnd(null)).Return(
				_personAssignmentJustAfterDayOffPeriod.EndDateTime).IgnoreArguments();

            _mocks.ReplayAll();

            var result = _target.Validate(_dic, _days);
            Assert.AreEqual(1, result.Count());
            _mocks.VerifyAll();
        }

        [Test]
        public void ValidatedFailsWhenAssignmentBeforeAndAfterConflicts()
        {
            Expect.Call(_day.PersonDayOffCollection()).Return(
                new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { _personDayOff1 }));
            Expect.Call(_dayBefore.PersonAssignmentCollection()).Return(
                new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { _personAssignmentConflictingWithDayOffStart })).Repeat.Twice();
            Expect.Call(_dayAfter.PersonAssignmentCollection()).Return(
                new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { _personAssignmentConflictingWithDayOffEnd })).Repeat.Twice();
        	Expect.Call(_workTimeStartEndExtractor.WorkTimeStart(null)).Return(
				_personAssignmentConflictingWithDayOffStartPeriod.StartDateTime).IgnoreArguments();
			Expect.Call(_workTimeStartEndExtractor.WorkTimeEnd(null)).Return(
				_personAssignmentConflictingWithDayOffStartPeriod.EndDateTime).IgnoreArguments();
			Expect.Call(_workTimeStartEndExtractor.WorkTimeStart(null)).Return(
				_personAssignmentConflictingWithDayOffEndPeriod.StartDateTime).IgnoreArguments();
			Expect.Call(_workTimeStartEndExtractor.WorkTimeEnd(null)).Return(
				_personAssignmentConflictingWithDayOffEndPeriod.EndDateTime).IgnoreArguments();

            Expect.Call(_dayBefore.PersonDayOffCollection()).Return(
               new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));
            Expect.Call(_dayAfter.PersonDayOffCollection()).Return(
                new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>()));

            Expect.Call(_day.PersonAssignmentCollection()).Return(
                new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>())).Repeat.Twice();
            _mocks.ReplayAll();

            var result = _target.Validate(_dic, _days);
            Assert.AreEqual(1, result.Count());
            _mocks.VerifyAll();
        }

        
        [Test]
        public void WhenDayOffDoesNotFitBetweenAssignmentsRuleReportsError()
        {
            var date = new DateTime(2010, 9, 6, 6, 0, 0, DateTimeKind.Utc);
            var assBeforePeriod = new DateTimePeriod(date, date.AddHours(13));
            var assAfterPeriod = new DateTimePeriod(date.AddDays(2), date.AddDays(2).AddHours(8));

            var dayOff1 = new DayOffTemplate(new Description("test"));
            dayOff1.SetTargetAndFlexibility(TimeSpan.FromHours(36), TimeSpan.FromHours(2));
            dayOff1.Anchor = new TimeSpan(12, 0, 0);
            _personDayOff1 = new PersonDayOff(_person, _scenario, dayOff1, new DateOnly(2010, 9, 7));

            var result = _target.DayOffDoesConflictWithActivity(_personDayOff1, assBeforePeriod, assAfterPeriod);

            Assert.That(result, Is.True);
            Assert.That(_target.ErrorMessage, Is.Not.Empty);
        }

        [Test]
        public void WhenAssignmentsFitsButConflictsBeforeAndDayOffHaveEnoughFlexibilityNoError()
        {
            var date = new DateTime(2010, 9, 6, 6, 0, 0, DateTimeKind.Utc);
            // the day must move one hour forward
            var assBeforePeriod = new DateTimePeriod(date, date.AddHours(13));
            // and there is room for that
            var assAfterPeriod = new DateTimePeriod(date.AddDays(2).AddHours(2), date.AddDays(2).AddHours(10));

            var dayOff1 = new DayOffTemplate(new Description("test"));
            dayOff1.SetTargetAndFlexibility(TimeSpan.FromHours(36), TimeSpan.FromHours(2));
            dayOff1.Anchor = new TimeSpan(12, 0, 0);
            _personDayOff1 = new PersonDayOff(_person, _scenario, dayOff1, new DateOnly(2010, 9, 7));

            var result = _target.DayOffDoesConflictWithActivity(_personDayOff1, assBeforePeriod, assAfterPeriod);

            Assert.That(result, Is.False);
            Assert.That(_target.ErrorMessage, Is.Empty);
        }

        [Test]
        public void WhenAssignmentsFitsButConflictsAfterAndDayOffHaveEnoughFlexibilityNoError()
        {
            var date = new DateTime(2010, 9, 6, 6, 0, 0, DateTimeKind.Utc);
            // the day must move one hour forward
            var assBeforePeriod = new DateTimePeriod(date, date.AddHours(8));
            // and there is room for that
            var assAfterPeriod = new DateTimePeriod(date.AddDays(2).AddHours(-1), date.AddDays(2).AddHours(10));

            var dayOff1 = new DayOffTemplate(new Description("test"));
            dayOff1.SetTargetAndFlexibility(TimeSpan.FromHours(36), TimeSpan.FromHours(2));
            dayOff1.Anchor = new TimeSpan(12, 0, 0);
            _personDayOff1 = new PersonDayOff(_person, _scenario, dayOff1, new DateOnly(2010, 9, 7));

            var result = _target.DayOffDoesConflictWithActivity(_personDayOff1, assBeforePeriod, assAfterPeriod);

            Assert.That(result, Is.False);
            Assert.That(_target.ErrorMessage, Is.Empty);
        }

        [Test]
        public void WhenAssignmentBeforeConflictAndNoFlexibilityError()
        {
            var date = new DateTime(2010, 9, 6, 6, 0, 0, DateTimeKind.Utc);
            // the day must move one hour forward
            var assBeforePeriod = new DateTimePeriod(date, date.AddHours(13));
            // and there is room for that
            var assAfterPeriod = new DateTimePeriod(date.AddDays(2).AddHours(2), date.AddDays(2).AddHours(10));

            var dayOff1 = new DayOffTemplate(new Description("test"));
            dayOff1.SetTargetAndFlexibility(TimeSpan.FromHours(36), TimeSpan.FromHours(0));
            dayOff1.Anchor = new TimeSpan(12, 0, 0);
            _personDayOff1 = new PersonDayOff(_person, _scenario, dayOff1, new DateOnly(2010, 9, 7));

            var result = _target.DayOffDoesConflictWithActivity(_personDayOff1, assBeforePeriod, assAfterPeriod);

            Assert.That(result, Is.True);
            Assert.That(_target.ErrorMessage, Is.Not.Empty);
        }

        [Test]
        public void WhenAssignmentAfterConflictAndNoFlexibilityError()
        {
            var date = new DateTime(2010, 9, 6, 6, 0, 0, DateTimeKind.Utc);
            // the day must move one hour forward
            var assBeforePeriod = new DateTimePeriod(date, date.AddHours(10));
            // and there is room for that
            var assAfterPeriod = new DateTimePeriod(date.AddDays(2).AddHours(-1), date.AddDays(2).AddHours(10));

            var dayOff1 = new DayOffTemplate(new Description("test"));
            dayOff1.SetTargetAndFlexibility(TimeSpan.FromHours(36), TimeSpan.FromHours(0));
            dayOff1.Anchor = new TimeSpan(12, 0, 0);
            _personDayOff1 = new PersonDayOff(_person, _scenario, dayOff1, new DateOnly(2010, 9, 7));

            var result = _target.DayOffDoesConflictWithActivity(_personDayOff1, assBeforePeriod, assAfterPeriod);

            Assert.That(result, Is.True);
            Assert.That(_target.ErrorMessage, Is.Not.Empty);
        }

        [Test]
        public void WhenAssignmentIsOnSameDateAsDayOffItIsNoConflict()
        {
            var date = new DateTime(2007, 8, 3, 0, 0, 0, DateTimeKind.Utc);
            Assert.IsFalse(NewDayOffRule.DayOffConflictWithAssignmentAfter(_personDayOff1, new DateTimePeriod(date, date.AddHours(8))));
            Assert.IsFalse(NewDayOffRule.DayOffConflictWithAssignmentBefore(_personDayOff1, new DateTimePeriod(date, date.AddHours(8))));
        }

        [Test]
        public void WhenAssignmentEndBeforeAnchorItIsNoConflictAfter()
        {
            var date = new DateTime(2007, 8, 2, 0, 0, 0, DateTimeKind.Utc);
            Assert.IsFalse(NewDayOffRule.DayOffConflictWithAssignmentAfter(_personDayOff1, new DateTimePeriod(date, date.AddHours(8))));
        }
        [Test]
        public void WhenAssignmentStartsAnchorItIsNoConflictBefore()
        {
            var date = new DateTime(2007, 8, 4, 0, 0, 0, DateTimeKind.Utc);
            Assert.IsFalse(NewDayOffRule.DayOffConflictWithAssignmentBefore(_personDayOff1, new DateTimePeriod(date, date.AddHours(8))));
        }
    }
}
