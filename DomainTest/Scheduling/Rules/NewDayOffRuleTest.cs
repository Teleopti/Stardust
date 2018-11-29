using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
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
	    private DayOff _dayOff1;
        private IPersonAssignment _personAssignmentConflictingWithDayOffEnd;
        private IPersonAssignment _personAssignmentConflictingWithDayOffStart;
        private IPersonAssignment _personAssignmentJustAfterDayOff;
        private IPersonAssignment _personAssignmentJustBeforeDayOff;

        private TimeZoneInfo _timeZone;
    	private IWorkTimeStartEndExtractor _workTimeStartEndExtractor;
    	private DateTimePeriod _personAssignmentConflictingWithDayOffStartPeriod;
    	private DateTimePeriod _personAssignmentJustAfterDayOffPeriod;
    	private DateTimePeriod _personAssignmentConflictingWithDayOffEndPeriod;
    	private DateTimePeriod _personAssignmentJustBeforeDayOffPeriod;

        private void setup()
        {
			_mocks = new MockRepository();
			_workTimeStartEndExtractor = _mocks.StrictMock<IWorkTimeStartEndExtractor>();
			_target = new NewDayOffRule(_workTimeStartEndExtractor);
            _day = _mocks.DynamicMock<IScheduleDay>();
            _days = new List<IScheduleDay> { _day };
						_dayBefore = _mocks.DynamicMock<IScheduleDay>();
						_dayAfter = _mocks.DynamicMock<IScheduleDay>();
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
				
				_dayOff1 = new DayOff(new DateTime(2007, 8, 3, 10, 30, 0, DateTimeKind.Utc), TimeSpan.FromHours(36), TimeSpan.FromHours(9), new Description(), Color.Beige, string.Empty, Guid.NewGuid());

            // add this and the day off cannot be moved backwards
    		_personAssignmentJustBeforeDayOffPeriod = new DateTimePeriod(_start.AddHours(-3), _end.AddHours(-3));
            _personAssignmentJustBeforeDayOff = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(_person, _scenario, _activity, _personAssignmentJustBeforeDayOffPeriod, _category);

            // this will start 3 hours before the Day Off ends
    		_personAssignmentConflictingWithDayOffEndPeriod = new DateTimePeriod(_start.AddHours(24 + 17), _end.AddHours(24 + 17));
            _personAssignmentConflictingWithDayOffEnd = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(_person, _scenario, _activity, _personAssignmentConflictingWithDayOffEndPeriod, _category);

            // add this and the day off cannot be moved forward
    		_personAssignmentJustAfterDayOffPeriod = new DateTimePeriod(_start.AddHours(24 + 18), _end.AddHours(24 + 18));
            _personAssignmentJustAfterDayOff = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(_person, _scenario, _activity, _personAssignmentJustAfterDayOffPeriod, _category);

            // this will start just before the Day Off and end in it
    		_personAssignmentConflictingWithDayOffStartPeriod = new DateTimePeriod(_start.AddHours(2), _end.AddHours(2));
            _personAssignmentConflictingWithDayOffStart = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(_person, _scenario, _activity, _personAssignmentConflictingWithDayOffStartPeriod, _category);


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
	        setup();
			Assert.IsNotNull(_target);
            Assert.AreEqual("", _target.ErrorMessage);
            Assert.IsTrue(_target.HaltModify);
            _target.HaltModify = false;
            Assert.IsFalse(_target.HaltModify);
        }

        [Test]
        public void WhenNoDayOffRuleReportsNoError()
        {
			setup();
			_mocks.ReplayAll();
            Assert.AreEqual(0, _target.Validate(_dic, _days).Count());
            _mocks.VerifyAll();
        }

        [Test]
        public void ValidatedFailsWhenAssignmentJustBeforeAndConflictingAssignmentAfter()
        {
			setup();
			var assWithDayOff = new PersonAssignment(_person, _scenario, new DateOnly(2007, 8, 3));
	        var template = new DayOffTemplate(new Description());
	        template.Anchor = new TimeSpan(10, 30, 0);
					template.SetTargetAndFlexibility(_dayOff1.TargetLength, _dayOff1.Flexibility);
					assWithDayOff.SetDayOff(template);
	        Expect.Call(_day.PersonAssignment()).Return(assWithDayOff).Repeat.Any();

            Expect.Call(_dayBefore.PersonAssignment()).Return(_personAssignmentJustBeforeDayOff).Repeat.Any();
            Expect.Call(_dayAfter.PersonAssignment()).Return(_personAssignmentConflictingWithDayOffEnd).Repeat.Any();

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
			setup();
			var assWithDayOff = new PersonAssignment(_person, _scenario, new DateOnly(2007, 8, 3));
					var template = new DayOffTemplate(new Description());
					template.Anchor = new TimeSpan(10, 30, 0);
					template.SetTargetAndFlexibility(_dayOff1.TargetLength, _dayOff1.Flexibility);
					assWithDayOff.SetDayOff(template);
					Expect.Call(_day.PersonAssignment()).Return(assWithDayOff).Repeat.Any();
					Expect.Call(_dayBefore.PersonAssignment()).Return(_personAssignmentConflictingWithDayOffStart).Repeat.Any();
					Expect.Call(_dayAfter.PersonAssignment()).Return(_personAssignmentJustAfterDayOff).Repeat.Any();


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

        [Test, SetUICulture("sv-SE")]
		public void ValidatedFailsWhenAssignmentBeforeAndAfterConflicts()
        {
			setup();
			var assWithDayOff = new PersonAssignment(_person, _scenario, new DateOnly(2007, 8, 3));
					var template = new DayOffTemplate(new Description());
					template.Anchor = new TimeSpan(10, 30, 0);
					template.SetTargetAndFlexibility(_dayOff1.TargetLength, _dayOff1.Flexibility);
					assWithDayOff.SetDayOff(template);
					Expect.Call(_day.PersonAssignment()).Return(assWithDayOff).Repeat.Any();

					Expect.Call(_dayBefore.PersonAssignment()).Return(_personAssignmentConflictingWithDayOffStart).Repeat.Any();
					Expect.Call(_dayAfter.PersonAssignment()).Return(_personAssignmentConflictingWithDayOffEnd).Repeat.Any();

        	Expect.Call(_workTimeStartEndExtractor.WorkTimeStart(null)).Return(
				_personAssignmentConflictingWithDayOffStartPeriod.StartDateTime).IgnoreArguments();
			Expect.Call(_workTimeStartEndExtractor.WorkTimeEnd(null)).Return(
				_personAssignmentConflictingWithDayOffStartPeriod.EndDateTime).IgnoreArguments();
			Expect.Call(_workTimeStartEndExtractor.WorkTimeStart(null)).Return(
				_personAssignmentConflictingWithDayOffEndPeriod.StartDateTime).IgnoreArguments();
			Expect.Call(_workTimeStartEndExtractor.WorkTimeEnd(null)).Return(
				_personAssignmentConflictingWithDayOffEndPeriod.EndDateTime).IgnoreArguments();

            _mocks.ReplayAll();

            var result = _target.Validate(_dic, _days).ToArray();
            Assert.AreEqual(1, result.Length);

	        var response = result.First();
			Assert.IsTrue(response.FriendlyName.StartsWith("Fridagskonflikt"));
			Assert.IsTrue(response.Message.StartsWith("Fridagen den "));
            _mocks.VerifyAll();
        }

        
        [Test]
        public void WhenDayOffDoesNotFitBetweenAssignmentsRuleReportsError()
        {
			setup();
			var date = new DateTime(2010, 9, 6, 6, 0, 0, DateTimeKind.Utc);
            var assBeforePeriod = new DateTimePeriod(date, date.AddHours(13));
            var assAfterPeriod = new DateTimePeriod(date.AddDays(2), date.AddDays(2).AddHours(8));

						var result = _target.DayOffDoesConflictWithActivity(new DayOff(new DateTime(2010, 9, 7, 12, 0, 0, DateTimeKind.Utc), TimeSpan.FromHours(36), TimeSpan.FromHours(2), new Description(), Color.Red, string.Empty, Guid.NewGuid()), assBeforePeriod, assAfterPeriod);

            Assert.That(result, Is.True);
            Assert.That(_target.ErrorMessage, Is.Not.Empty);
        }

        [Test]
        public void WhenAssignmentsFitsButConflictsBeforeAndDayOffHaveEnoughFlexibilityNoError()
        {
			setup();
			var date = new DateTime(2010, 9, 6, 6, 0, 0, DateTimeKind.Utc);
            // the day must move one hour forward
            var assBeforePeriod = new DateTimePeriod(date, date.AddHours(13));
            // and there is room for that
            var assAfterPeriod = new DateTimePeriod(date.AddDays(2).AddHours(2), date.AddDays(2).AddHours(10));

						var result = _target.DayOffDoesConflictWithActivity(new DayOff(new DateTime(2010, 9, 7, 12, 0, 0, DateTimeKind.Utc), TimeSpan.FromHours(36), TimeSpan.FromHours(2), new Description(), Color.Red, string.Empty, Guid.NewGuid()), assBeforePeriod, assAfterPeriod);

            Assert.That(result, Is.False);
            Assert.That(_target.ErrorMessage, Is.Empty);
        }

        [Test]
        public void WhenAssignmentsFitsButConflictsAfterAndDayOffHaveEnoughFlexibilityNoError()
        {
			setup();
			var date = new DateTime(2010, 9, 6, 6, 0, 0, DateTimeKind.Utc);
            // the day must move one hour forward
            var assBeforePeriod = new DateTimePeriod(date, date.AddHours(8));
            // and there is room for that
            var assAfterPeriod = new DateTimePeriod(date.AddDays(2).AddHours(-1), date.AddDays(2).AddHours(10));

						var result = _target.DayOffDoesConflictWithActivity(new DayOff(new DateTime(2010, 9, 7, 12, 0, 0, DateTimeKind.Utc), TimeSpan.FromHours(36), TimeSpan.FromHours(2), new Description(), Color.Red, string.Empty, Guid.NewGuid()), assBeforePeriod, assAfterPeriod);

            Assert.That(result, Is.False);
            Assert.That(_target.ErrorMessage, Is.Empty);
        }

        [Test]
        public void WhenAssignmentBeforeConflictAndNoFlexibilityError()
        {
			setup();
			var date = new DateTime(2010, 9, 6, 6, 0, 0, DateTimeKind.Utc);
            // the day must move one hour forward
            var assBeforePeriod = new DateTimePeriod(date, date.AddHours(13));
            // and there is room for that
            var assAfterPeriod = new DateTimePeriod(date.AddDays(2).AddHours(2), date.AddDays(2).AddHours(10));

						var result = _target.DayOffDoesConflictWithActivity(new DayOff(new DateTime(2010, 9, 7, 12, 0, 0, DateTimeKind.Utc), TimeSpan.FromHours(36), TimeSpan.FromHours(0), new Description(), Color.Red, string.Empty, Guid.NewGuid()), assBeforePeriod, assAfterPeriod);

            Assert.That(result, Is.True);
            Assert.That(_target.ErrorMessage, Is.Not.Empty);
        }

        [Test]
        public void WhenAssignmentAfterConflictAndNoFlexibilityError()
        {
			setup();
			var date = new DateTime(2010, 9, 6, 6, 0, 0, DateTimeKind.Utc);
            // the day must move one hour forward
            var assBeforePeriod = new DateTimePeriod(date, date.AddHours(10));
            // and there is room for that
            var assAfterPeriod = new DateTimePeriod(date.AddDays(2).AddHours(-1), date.AddDays(2).AddHours(10));

						var result = _target.DayOffDoesConflictWithActivity(new DayOff(new DateTime(2010, 9, 7, 12, 0, 0, DateTimeKind.Utc), TimeSpan.FromHours(36), TimeSpan.FromHours(0), new Description(), Color.Red, string.Empty, Guid.NewGuid()), assBeforePeriod, assAfterPeriod);

            Assert.That(result, Is.True);
            Assert.That(_target.ErrorMessage, Is.Not.Empty);
        }

        [Test]
        public void WhenAssignmentIsOnSameDateAsDayOffItIsNoConflict()
        {
			setup();
			var date = new DateTime(2007, 8, 3, 0, 0, 0, DateTimeKind.Utc);
            Assert.IsFalse(NewDayOffRule.DayOffConflictWithAssignmentAfter(_dayOff1, new DateTimePeriod(date, date.AddHours(8))));
						Assert.IsFalse(NewDayOffRule.DayOffConflictWithAssignmentBefore(_dayOff1, new DateTimePeriod(date, date.AddHours(8))));
        }

        [Test]
        public void WhenAssignmentEndBeforeAnchorItIsNoConflictAfter()
        {
			setup();
			var date = new DateTime(2007, 8, 2, 0, 0, 0, DateTimeKind.Utc);
						Assert.IsFalse(NewDayOffRule.DayOffConflictWithAssignmentAfter(_dayOff1, new DateTimePeriod(date, date.AddHours(8))));
        }
        [Test]
        public void WhenAssignmentStartsAnchorItIsNoConflictBefore()
        {
			setup();
			var date = new DateTime(2007, 8, 4, 0, 0, 0, DateTimeKind.Utc);
						Assert.IsFalse(NewDayOffRule.DayOffConflictWithAssignmentBefore(_dayOff1, new DateTimePeriod(date, date.AddHours(8))));
        }

		[Test]
		public void ShouldNotConsiderOnlyOvertimeOnSameDayAsDayOff()
		{
			setup();
			var date = new DateOnly(2007, 8, 3);
			var activity = new Activity("_") {InWorkTime = true};

			var assWithDayOff = new PersonAssignment(_person, _scenario, date);
			var template = new DayOffTemplate(new Description()) {Anchor = TimeSpan.FromHours(12)};
			template.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(8));
			assWithDayOff.SetDayOff(template);
			assWithDayOff.AddOvertimeActivity(activity, new DateTimePeriod(new DateTime(2007,8,3,11,45,0, DateTimeKind.Utc), 
				new DateTime(2007, 8, 3, 12, 15, 0, DateTimeKind.Utc)), new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));

			var assBeforeEndsAt15 = new PersonAssignment(_person, _scenario, date.AddDays(-1));
			assBeforeEndsAt15.AddActivity(activity, new TimePeriod(8,0,15,0));
			var assAfterStartsAt12 = new PersonAssignment(_person, _scenario, date.AddDays(1));
			assAfterStartsAt12.AddActivity(activity, new TimePeriod(12,0,18,0));

			Expect.Call(_day.PersonAssignment()).Return(assWithDayOff).Repeat.Any();
			Expect.Call(_dayBefore.PersonAssignment()).Return(assBeforeEndsAt15).Repeat.Any();
			Expect.Call(_dayAfter.PersonAssignment()).Return(assAfterStartsAt12).Repeat.Any();

			_mocks.ReplayAll();

			var target = new NewDayOffRule(new WorkTimeStartEndExtractor());

			target.Validate(_dic, _days).Should().Be.Empty();
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldBreakRuleWhenHavingDayOffAndOvertime()
		{
			setup();
			var date = new DateOnly(2007, 8, 3);
			var activity = new Activity("_") { InWorkTime = true };

			var assWithDayOff = new PersonAssignment(_person, _scenario, date);
			var template = new DayOffTemplate(new Description()) { Anchor = TimeSpan.FromHours(12) };
			template.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(8));
			assWithDayOff.SetDayOff(template);
			assWithDayOff.AddOvertimeActivity(activity, new DateTimePeriod(new DateTime(2007, 8, 3, 11, 45, 0, DateTimeKind.Utc),
				new DateTime(2007, 8, 3, 12, 15, 0, DateTimeKind.Utc)), new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));

			var assBeforeEndsAt15 = new PersonAssignment(_person, _scenario, date.AddDays(-1));
			assBeforeEndsAt15.AddActivity(activity, new TimePeriod(23, 0, 34, 0));
			var assAfterStartsAt12 = new PersonAssignment(_person, _scenario, date.AddDays(1));
			assAfterStartsAt12.AddActivity(activity, new TimePeriod(1, 0, 9, 0));

			Expect.Call(_day.PersonAssignment()).Return(assWithDayOff).Repeat.Any();
			Expect.Call(_dayBefore.PersonAssignment()).Return(assBeforeEndsAt15).Repeat.Any();
			Expect.Call(_dayAfter.PersonAssignment()).Return(assAfterStartsAt12).Repeat.Any();

			_mocks.ReplayAll();

			var target = new NewDayOffRule(new WorkTimeStartEndExtractor());

			target.Validate(_dic, _days).Should().Not.Be.Empty();
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldBreakRuleWhenHavingDayOffAndOvertimeIfDayBeforeAndDayAfterIsAssignedWithOvertime()
		{
			setup();
			var date = new DateOnly(2007, 8, 3);
			var activity = new Activity("_") { InWorkTime = true };

			var assWithDayOff = new PersonAssignment(_person, _scenario, date);
			var template = new DayOffTemplate(new Description()) { Anchor = TimeSpan.FromHours(12) };
			template.SetTargetAndFlexibility(TimeSpan.FromHours(36), TimeSpan.FromHours(8));
			assWithDayOff.SetDayOff(template);
			assWithDayOff.AddOvertimeActivity(activity, new DateTimePeriod(new DateTime(2007, 8, 3, 11, 45, 0, DateTimeKind.Utc),
				new DateTime(2007, 8, 3, 12, 15, 0, DateTimeKind.Utc)), new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));

			var assBeforeEndsAt23 = new PersonAssignment(_person, _scenario, date.AddDays(-1));
			assBeforeEndsAt23.AddOvertimeActivity(activity, new DateTimePeriod(2007,8,2,22,2007,8,2,23), new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));
			var assAfterStartsAt01 = new PersonAssignment(_person, _scenario, date.AddDays(1));
			assAfterStartsAt01.AddOvertimeActivity(activity, new DateTimePeriod(2007, 8, 4, 1, 2007, 8, 4, 2), new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));
			Expect.Call(_day.PersonAssignment()).Return(assWithDayOff).Repeat.Any();
			Expect.Call(_dayBefore.PersonAssignment()).Return(assBeforeEndsAt23).Repeat.Any();
			Expect.Call(_dayAfter.PersonAssignment()).Return(assAfterStartsAt01).Repeat.Any();

			_mocks.ReplayAll();

			var target = new NewDayOffRule(new WorkTimeStartEndExtractor());

			target.Validate(_dic, _days).Should().Not.Be.Empty();
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldBreakRuleWhenHavingDayOffAndOvertimeIfDayBeforeAndDayAfterIsAssignedWithOvertimeAgain()
		{
			setup();
			var date = new DateOnly(2007, 8, 3);
			var activity = new Activity("_") { InWorkTime = true };

			var assWithDayOff = new PersonAssignment(_person, _scenario, date);
			var template = new DayOffTemplate(new Description()) { Anchor = TimeSpan.FromHours(12) };
			template.SetTargetAndFlexibility(TimeSpan.FromHours(36), TimeSpan.FromHours(8));
			assWithDayOff.SetDayOff(template);
			// Not relevant
			//assWithDayOff.AddOvertimeActivity(activity, new DateTimePeriod(new DateTime(2007, 8, 3, 11, 45, 0, DateTimeKind.Utc),
			//	new DateTime(2007, 8, 3, 12, 15, 0, DateTimeKind.Utc)), new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));

			var assBeforeEndsAt23 = new PersonAssignment(_person, _scenario, date.AddDays(-1));
			assBeforeEndsAt23.AddOvertimeActivity(activity, new DateTimePeriod(2007, 8, 2, 22, 2007, 8, 2, 23), new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));
			var assAfterStartsAt01 = new PersonAssignment(_person, _scenario, date.AddDays(1));
			assAfterStartsAt01.AddOvertimeActivity(activity, new DateTimePeriod(2007, 8, 4, 1, 2007, 8, 4, 2), new MultiplicatorDefinitionSet("_", MultiplicatorType.Overtime));
			Expect.Call(_day.PersonAssignment()).Return(assWithDayOff).Repeat.Any();
			Expect.Call(_dayBefore.PersonAssignment()).Return(assBeforeEndsAt23).Repeat.Any();
			Expect.Call(_dayAfter.PersonAssignment()).Return(assAfterStartsAt01).Repeat.Any();

			_mocks.ReplayAll();

			var target = new NewDayOffRule(new WorkTimeStartEndExtractor());

			target.Validate(_dic, _days).Should().Not.Be.Empty();
			_mocks.VerifyAll();
		}
	}
}
