﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[TestFixture]
	public class NonMainShiftActivityRuleTest
	{
		private IScheduleCommandToggle _toggleManager;

		[SetUp]
		public void Setup()
		{
			_toggleManager = MockRepository.GenerateMock<IScheduleCommandToggle>();
		}

		 [Test]
		 public void ShouldResultInNoBusinessRuleRespone()
		 {
			 var scheduleOk = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));

			 var pa = new PersonAssignment(scheduleOk.Person, scheduleOk.Scenario, new DateOnly(2000, 1, 1));
			 var start = new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc);
			 var end = new DateTime(2000, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			 pa.AddActivity(new Activity("d"), new DateTimePeriod(start, end));
			 scheduleOk.Add(pa);

			 setToggle(false);
			 new NonMainShiftActivityRule(_toggleManager)
					.Validate(null, new[] {scheduleOk})
					.Should().Be.Empty();
		 }

		[Test]
		public void ShouldReturnCorrectResponseWhenHasPersonalActivity()
		{
			var start = new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 15, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(start, end);

			var scheduleDataWithPersonalActivity = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));
			var dateOnly = new DateOnly(2000, 1, 1);
			var period = new DateOnlyPeriod(dateOnly, dateOnly);
			var person = scheduleDataWithPersonalActivity.Person;
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			string message = string.Format(CultureInfo.CurrentCulture,
												 Resources.HasNonMainShiftActivityErrorMessage, scheduleDataWithPersonalActivity.Person.Name,
												 dateOnly.Date.ToShortDateString());
			var expected = new BusinessRuleResponse(typeof(NonMainShiftActivityRule), message, true, false, period.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()),
					                         scheduleDataWithPersonalActivity.Person, period);

			var pa = new PersonAssignment(scheduleDataWithPersonalActivity.Person, scheduleDataWithPersonalActivity.Scenario, dateOnly);
			pa.AddPersonalActivity(new Activity("p"), assignmentPeriod);
			scheduleDataWithPersonalActivity.Add(pa);

			setToggle(false);
			var targetRel = new NonMainShiftActivityRule(_toggleManager)
				.Validate(null, new[] {scheduleDataWithPersonalActivity});

			targetRel.Should().Have.SameValuesAs(expected);
		}

		[Test]
		public void ShouldReturnCorrectResponseWhenHasOvertimeActivity()
		{
			var start = new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 15, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(start, end);

			var scheduleDataWithOvertimeActivity = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));
			var dateOnly = new DateOnly(2000, 1, 1);
			var period = new DateOnlyPeriod(dateOnly, dateOnly);
			var person = scheduleDataWithOvertimeActivity.Person;
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			string message = string.Format(CultureInfo.CurrentCulture,
												 Resources.HasNonMainShiftActivityErrorMessage, scheduleDataWithOvertimeActivity.Person.Name,
												 dateOnly.Date.ToShortDateString());
			var expected = new BusinessRuleResponse(typeof(NonMainShiftActivityRule), message, true, false, period.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()),
											 scheduleDataWithOvertimeActivity.Person, period);

			var pa = new PersonAssignment(scheduleDataWithOvertimeActivity.Person, scheduleDataWithOvertimeActivity.Scenario, dateOnly);
			pa.AddOvertimeActivity(new Activity("p"), assignmentPeriod, new MultiplicatorDefinitionSet("d", MultiplicatorType.Overtime));
			scheduleDataWithOvertimeActivity.Add(pa);

			setToggle(false);
			var targetRel = new NonMainShiftActivityRule(_toggleManager)
				.Validate(null, new[] { scheduleDataWithOvertimeActivity });

			targetRel.Should().Have.SameValuesAs(expected);
		}

		[Test]
		public void ShouldReturnCorrectResponseWhenHasMeeting()
		{
			var personMeeting = MockRepository.GenerateMock<IPersonMeeting>();
			var personMeetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>{personMeeting});
			var scheduleDataWithMeeting = MockRepository.GenerateMock<IScheduleDay>();
			var pa = MockRepository.GenerateMock<IPersonAssignment>();
			var dateOnly = new DateOnly(2000, 1, 1);
			var period = new DateOnlyPeriod(dateOnly, dateOnly);
			var person = PersonFactory.CreatePerson();

			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			pa.Stub(p => p.Date).Return(dateOnly);
			scheduleDataWithMeeting.Stub(s => s.Person).Return(person);
			scheduleDataWithMeeting.Stub(s => s.PersonAssignment()).Return(pa);
			scheduleDataWithMeeting.Stub(s => s.PersonMeetingCollection()).Return(personMeetings);

			string message = string.Format(CultureInfo.CurrentCulture,
												 Resources.HasNonMainShiftActivityErrorMessage, scheduleDataWithMeeting.Person.Name,
												 dateOnly.Date.ToShortDateString());
			var expected = new BusinessRuleResponse(typeof(NonMainShiftActivityRule), message, true, false, period.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()),
											 scheduleDataWithMeeting.Person, period);

			setToggle(false);
			var targetRel = new NonMainShiftActivityRule(_toggleManager)
				.Validate(null, new[] { scheduleDataWithMeeting });

			targetRel.Should().Have.SameValuesAs(expected);
		}

		[Test]
		public void ShouldBeOkWithNoAssignment()
		{
			var scheduleOk = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));

			new DataPartOfAgentDay()
				 .Validate(null, new[] { scheduleOk })
				 .Should().Be.Empty();
		}
		
		[Test]
		public void ShouldResultInTwoResponses()
		{
			var start = new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 15, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(start, end);

			var scheduleDataWithPersonalActivity = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));
			var dateOnly = new DateOnly(2000, 1, 1);
			var period = new DateOnlyPeriod(dateOnly, dateOnly);
			var person = scheduleDataWithPersonalActivity.Person;
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			string message = string.Format(CultureInfo.CurrentCulture,
												 Resources.HasNonMainShiftActivityErrorMessage, scheduleDataWithPersonalActivity.Person.Name,
												 dateOnly.Date.ToShortDateString());
			var expected = new BusinessRuleResponse(typeof(NonMainShiftActivityRule), message, true, false, period.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()),
					                         scheduleDataWithPersonalActivity.Person, period);

			var pa = new PersonAssignment(scheduleDataWithPersonalActivity.Person, scheduleDataWithPersonalActivity.Scenario, dateOnly);
			pa.AddPersonalActivity(new Activity("p"), assignmentPeriod);
			scheduleDataWithPersonalActivity.Add(pa);

			setToggle(false);
			var targetRel = new NonMainShiftActivityRule(_toggleManager)
				.Validate(null, new[] {scheduleDataWithPersonalActivity, scheduleDataWithPersonalActivity});

			targetRel.Should().Have.SameValuesAs(expected, expected);
		}

		[Test]
		public void ShouldResultNothingWhenMeetingInWorkSchedule()
		{
			var personMeeting = MockRepository.GenerateMock<IPersonMeeting>();
			personMeeting.Stub(x => x.Period).Return(new DateTimePeriod(2001, 1, 1, 11, 2001, 1, 1, 12));
			var personMeetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { personMeeting });
			var scheduleDataWithMeeting = MockRepository.GenerateMock<IScheduleDay>();
			var pa = MockRepository.GenerateMock<IPersonAssignment>();
			var dateOnly = new DateOnly(2000, 1, 1);
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			pa.Stub(p => p.Date).Return(dateOnly);
			scheduleDataWithMeeting.Stub(s => s.Person).Return(person);
			scheduleDataWithMeeting.Stub(s => s.PersonAssignment()).Return(pa);
			scheduleDataWithMeeting.Stub(s => s.PersonMeetingCollection()).Return(personMeetings);

			var scheduleBeTrade = MockRepository.GenerateMock<IScheduleDay>();
			var person2 = PersonFactory.CreatePerson();
			person2.SetId(Guid.NewGuid());
			scheduleBeTrade.Stub(s => s.Person).Return(person2);
			var shiftLayer = new MainShiftLayer(new Activity("phone"), new DateTimePeriod(2001, 1, 1, 9, 2001, 1, 1, 17));
			shiftLayer.Payload.InWorkTime = true;
			pa.Stub(x => x.ShiftLayers).Return(new List<IShiftLayer> { shiftLayer });
			scheduleBeTrade.Stub(s => s.PersonAssignment()).Return(pa);

			setToggle(true);
			var targetRel = new NonMainShiftActivityRule(_toggleManager)
				.Validate(null, new[] { scheduleDataWithMeeting, scheduleBeTrade });

			targetRel.Should().Be.Empty();
		}

		[Test]
		public void ShouldResultWhenMeetingInNotWorkSchedule()
		{
			var personMeeting = MockRepository.GenerateMock<IPersonMeeting>();
			personMeeting.Stub(x => x.Period).Return(new DateTimePeriod(2001, 1, 1, 11, 2001, 1, 1, 12));
			var personMeetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { personMeeting });
			var scheduleDataWithMeeting = MockRepository.GenerateMock<IScheduleDay>();
			var pa = MockRepository.GenerateMock<IPersonAssignment>();
			var dateOnly = new DateOnly(2000, 1, 1);
			var period = new DateOnlyPeriod(dateOnly, dateOnly);
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			pa.Stub(p => p.Date).Return(dateOnly);
			scheduleDataWithMeeting.Stub(s => s.Person).Return(person);
			scheduleDataWithMeeting.Stub(s => s.PersonAssignment()).Return(pa);
			scheduleDataWithMeeting.Stub(s => s.PersonMeetingCollection()).Return(personMeetings);

			var scheduleBeTrade = MockRepository.GenerateMock<IScheduleDay>();
			var person2 = PersonFactory.CreatePerson();
			person2.SetId(Guid.NewGuid());
			scheduleBeTrade.Stub(s => s.Person).Return(person2);
			var shiftLayer =   new MainShiftLayer(new Activity("lunch"), new DateTimePeriod(2001, 1,1,11, 2001,1,1,12));
			pa.Stub(x => x.ShiftLayers).Return(new List<IShiftLayer> { shiftLayer });
			scheduleBeTrade.Stub(s => s.PersonAssignment()).Return(pa);
			scheduleBeTrade.Stub(s => s.PersonMeetingCollection()).Return(new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>()));

			string message = string.Format(CultureInfo.CurrentCulture,
												 Resources.HasNonMainShiftActivityErrorMessage, scheduleDataWithMeeting.Person.Name,
												 dateOnly.Date.ToShortDateString());
			var expected = new BusinessRuleResponse(typeof(NonMainShiftActivityRule), message, true, false, period.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()),
											 scheduleDataWithMeeting.Person, period);

			setToggle(true);
			var targetRel = new NonMainShiftActivityRule(_toggleManager)
				.Validate(null, new[] { scheduleDataWithMeeting, scheduleBeTrade });

			targetRel.Should().Have.SameValuesAs(expected);
		}

		[Test]
		public void ShouldResultNothingWhenPersonalActivityInWorkSchedule()
		{
			var start = new DateTime(2001, 1, 1, 11, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2001, 1, 1, 15, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(start, end);

			var scheduleDataWithPersonalActivity = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));
			var dateOnly = new DateOnly(2001, 1, 1);
			var person = scheduleDataWithPersonalActivity.Person;
			person.SetId(Guid.NewGuid());
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var pa = new PersonAssignment(scheduleDataWithPersonalActivity.Person, scheduleDataWithPersonalActivity.Scenario, dateOnly);
			pa.AddPersonalActivity(new Activity("p"), assignmentPeriod);
			scheduleDataWithPersonalActivity.Add(pa);

			var scheduleBeTrade = MockRepository.GenerateMock<IScheduleDay>();
			var person2 = PersonFactory.CreatePerson();
			person2.SetId(Guid.NewGuid());
			scheduleBeTrade.Stub(s => s.Person).Return(person2);
			var shiftLayer = new MainShiftLayer(new Activity("phone"), new DateTimePeriod(2001, 1, 1, 9, 2001, 1, 1, 17));
			shiftLayer.Payload.InWorkTime = true;
			var pa2 = MockRepository.GenerateMock<IPersonAssignment>();
			pa2.Stub(p => p.Date).Return(dateOnly);
			pa2.Stub(x => x.ShiftLayers).Return(new List<IShiftLayer> { shiftLayer });
			scheduleBeTrade.Stub(s => s.PersonAssignment()).Return(pa2);

			setToggle(true);
			var targetRel = new NonMainShiftActivityRule(_toggleManager)
				.Validate(null, new[] { scheduleDataWithPersonalActivity, scheduleBeTrade });

			targetRel.Should().Be.Empty();
		}

		[Test]
		public void ShouldResultWhenPersonalActivityInNotWorkSchedule()
		{
			var start = new DateTime(2001, 1, 1, 11, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2001, 1, 1, 15, 0, 0, DateTimeKind.Utc);
			var dateOnly = new DateOnly(2001, 1, 1);
			var period = new DateOnlyPeriod(dateOnly, dateOnly);
			var assignmentPeriod = new DateTimePeriod(start, end);

			var scheduleDataWithPersonalActivity = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));
			
			var person = scheduleDataWithPersonalActivity.Person;
			person.SetId(Guid.NewGuid());
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var pa = new PersonAssignment(scheduleDataWithPersonalActivity.Person, scheduleDataWithPersonalActivity.Scenario, dateOnly);
			pa.AddPersonalActivity(new Activity("p"), assignmentPeriod);
			scheduleDataWithPersonalActivity.Add(pa);

			var scheduleBeTrade = MockRepository.GenerateMock<IScheduleDay>();
			var person2 = PersonFactory.CreatePerson();
			person2.SetId(Guid.NewGuid());
			scheduleBeTrade.Stub(s => s.Person).Return(person2);
			var shiftLayer = new MainShiftLayer(new Activity("lunch"), new DateTimePeriod(2001, 1, 1, 11, 2001, 1, 1, 12));
			shiftLayer.Payload.InWorkTime = false;
			var pa2 = MockRepository.GenerateMock<IPersonAssignment>();
			pa2.Stub(p => p.Date).Return(dateOnly);
			pa2.Stub(x => x.ShiftLayers).Return(new List<IShiftLayer> { shiftLayer });
			scheduleBeTrade.Stub(s => s.PersonAssignment()).Return(pa2);

			string message = string.Format(CultureInfo.CurrentCulture,
									 Resources.HasNonMainShiftActivityErrorMessage, scheduleDataWithPersonalActivity.Person.Name,
									 dateOnly.Date.ToShortDateString());
			var expected = new BusinessRuleResponse(typeof(NonMainShiftActivityRule), message, true, false, period.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()),
											 scheduleDataWithPersonalActivity.Person, period);

			setToggle(true);
			var targetRel = new NonMainShiftActivityRule(_toggleManager)
				.Validate(null, new[] { scheduleDataWithPersonalActivity, scheduleBeTrade });

			targetRel.Should().Have.SameValuesAs(expected);
		}

		private void setToggle(bool flag)
		{
			_toggleManager.Stub(x => x.IsEnabled(Toggles.MyTimeWeb_AutoShiftTradeWithMeetingAndPersonalActivity_33281)).Return(flag);
		}
	}
}