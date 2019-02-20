using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceAndScheduleDayViewModelMappingTest
	{
		private IProjectionProvider _projectionProvider;
		private PreferenceAndScheduleDayViewModelMapper _target;

		[SetUp]
		public void Setup()
		{
			_projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
			var userTimeZone = new FakeUserTimeZone(TimeZoneInfo.Local);
			_target = new PreferenceAndScheduleDayViewModelMapper(_projectionProvider, userTimeZone,
				new PreferenceDayViewModelMapper(new ExtendedPreferencePredicate()), new BankHolidayCalendarViewModelMapper());
		}
		
		[Test]
		public void ShouldMapDate()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today);

			var result = _target.Map(scheduleDay, null);

			result.Date.Should().Be(DateOnly.Today.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPreferenceViewModel()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today);
			var preferenceRestriction = new PreferenceRestriction();
			preferenceRestriction.DayOffTemplate = new DayOffTemplate(new Description("DO"));
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);
			var personRestrictionCollection = new[] {preferenceDay};

			scheduleDay.Stub(x => x.PersonRestrictionCollection()).Return(personRestrictionCollection);

			var result = _target.Map(scheduleDay, null);

			result.Preference.Should().Not.Be.Null();
			result.Preference.Preference.Should().Be("DO");
		}

		[Test]
		public void ShouldMapDayOffViewModel()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today, SchedulePartView.DayOff,
			                                                    PersonAssignmentFactory.CreateAssignmentWithDayOff(new Person(), new Scenario("s"),
				                                                    DateOnly.Today, new DayOffTemplate(new Description("DO"))));

			var result = _target.Map(scheduleDay, null);

			result.DayOff.Should().Not.Be.Null();
			result.DayOff.DayOff.Should().Be("DO");
		}

		[Test]
		public void ShouldMapAbsenceViewModel()
		{
			var stubs = new StubFactory();
			var absence = stubs.PersonAbsenceStub("Illness");
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today, SchedulePartView.FullDayAbsence, absence);

			var contractTime = TimeSpan.FromHours(8);
			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.ContractTime()).Return(contractTime);
			_projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var result = _target.Map(scheduleDay, null);

			result.Absence.Should().Not.Be.Null();
			result.Absence.Absence.Should().Be("Illness");
		}

		[Test]
		public void ShouldMapAbsenceContractTime()
		{
			var stubs = new StubFactory();
			var absence = stubs.PersonAbsenceStub("Illness");

			var contractTime = TimeSpan.FromHours(8);
			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.ContractTime()).Return(contractTime);

			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today, SchedulePartView.FullDayAbsence, absence);

			_projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var result = _target.Map(scheduleDay, null);

			result.Absence.Should().Not.Be.Null();
			result.Absence.AbsenceContractTime.Should().Be(TimeHelper.GetLongHourMinuteTimeString(contractTime, CultureInfo.CurrentUICulture));
			result.Absence.AbsenceContractTimeMinutes.Should().Be(contractTime.TotalMinutes);
		}

		[Test]
		public void ShouldMapShiftCategoryInPersonAssignmentDayViewModel()
		{
			var personAssignment = new PersonAssignment(new Person(), new Scenario("s"), new DateOnly(2000,1,1));
			personAssignment.AddActivity(new Activity("sdf"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
			personAssignment.SetShiftCategory(new ShiftCategory("shiftCategory"));
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today, SchedulePartView.MainShift, personAssignment);

			var contractTime = TimeSpan.FromHours(8);
			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.ContractTime()).Return(contractTime);
			_projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var result = _target.Map(scheduleDay, null);

			result.PersonAssignment.Should().Not.Be.Null();
			result.PersonAssignment.ShiftCategory.Should().Be("shiftCategory");
		}

		[Test]
		public void ShouldMapContractTimeInPersonAssignmentDayViewModel()
		{
			var contractTime = TimeSpan.FromHours(8);
			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.ContractTime()).Return(contractTime);
			var personAssignment = new PersonAssignment(new Person(), new Scenario("s"), new DateOnly(2000, 1, 1));
			personAssignment.SetShiftCategory(ShiftCategoryFactory.CreateShiftCategory());
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today, SchedulePartView.MainShift, personAssignment);
			_projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var result = _target.Map(scheduleDay, null);

			result.PersonAssignment.Should().Not.Be.Null();
			result.PersonAssignment.ContractTime.Should().Be(TimeHelper.GetLongHourMinuteTimeString(contractTime, CultureInfo.CurrentUICulture));
		}

		[Test]
		public void ShouldMapContractTimeMinutesInPersonAssignmentDayViewModel()
		{
			var contractTime = TimeSpan.FromHours(8);
			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.ContractTime()).Return(contractTime);
			var personAssignment = new PersonAssignment(new Person(), new Scenario("s"), new DateOnly(2000, 1, 1));
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today, SchedulePartView.MainShift, personAssignment);
			_projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var result = _target.Map(scheduleDay, null);

			result.PersonAssignment.Should().Not.Be.Null();
			result.PersonAssignment.ContractTimeMinutes.Should().Be(contractTime.TotalMinutes);
		}

		[Test]
		public void ShouldMapTimeSpanInPersonAssignmentDayViewModel()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(),new Scenario("s"),new DateOnly(2011,5,18));
			var period = new DateTimePeriod(2011,5,18,7,2011,5,18,16);
			personAssignment.AddActivity(new Activity("a") { InWorkTime = true },period);
			personAssignment.SetShiftCategory(new ShiftCategory("sc"));
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011,5,18),SchedulePartView.MainShift,personAssignment);

			var contractTime = TimeSpan.FromHours(8);
			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.ContractTime()).Return(contractTime);
			_projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);
			
			var result = _target.Map(scheduleDay, null);
			result.PersonAssignment.TimeSpan.Should().Be.EqualTo(period.TimePeriod(scheduleDay.TimeZone).ToShortTimeString());
		}

		[Test]
		public void ShouldFlagFeedbackIfInsidePeriodAndNotScheduled()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today);
			scheduleDay.Stub(x => x.IsScheduled()).Return(false);

			var result = _target.Map(scheduleDay, null);

			result.Feedback.Should().Be.True();
		}

		[Test]
		public void ShouldNotFlagFeedbackIfScheduled()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today);
			scheduleDay.Stub(x => x.IsScheduled()).Return(true);

			var result = _target.Map(scheduleDay, null);

			result.Feedback.Should().Be.False();
		}

		[Test]
		public void ShouldMapDayOffStyleClassName()
		{
			var stubs = new StubFactory();
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Today, SchedulePartView.DayOff, PersonAssignmentFactory.CreateAssignmentWithDayOff());

			var result = _target.Map(scheduleDay, null);

			result.StyleClassName.Should().Contain(StyleClasses.DayOff);
			result.StyleClassName.Should().Contain(StyleClasses.Striped);
		}

		[Test]
		public void ShouldMapContractDayOffStyleClassName()
		{
			var stubs = new StubFactory();
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Today, SchedulePartView.ContractDayOff, PersonAssignmentFactory.CreateAssignmentWithDayOff());

			var result = _target.Map(scheduleDay, null);

			result.StyleClassName.Should().Contain(StyleClasses.Striped);
		}

		[Test]
		public void ShouldMapAbsenceBorderColor()
		{
			var stubs = new StubFactory();
			var personAbsence = stubs.PersonAbsenceStub(new DateTimePeriod(), stubs.AbsenceLayerStub(stubs.AbsenceStub(Color.DarkMagenta)));
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Today, SchedulePartView.FullDayAbsence, personAbsence);

			var result = _target.Map(scheduleDay, null);

			result.BorderColor.Should().Be(Color.DarkMagenta.ToHtml());
		}

		[Test]
		public void ShouldMapPersonAssignmentsBorderColor()
		{
			var stubs = new StubFactory();
			var personAssignment = stubs.PersonAssignmentStub(new DateTimePeriod(), stubs.ShiftCategoryStub(Color.Coral));
			personAssignment.Stub(x => x.PersonalActivities()).Return(new List<PersonalShiftLayer>());
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Today, SchedulePartView.MainShift, personAssignment);

			var result = _target.Map(scheduleDay, null);

			result.BorderColor.Should().Be(Color.Coral.ToHtml());
		}

		[Test]
		public void ShouldMapMeetings()
		{
			var now = DateTime.Now;
			var stubs = new StubFactory();
			var meeting = MockRepository.GenerateMock<IPersonMeeting>();
			var belongsToMeeting = MockRepository.GenerateMock<IMeeting>();
			belongsToMeeting.Stub(x => x.GetSubject(new NoFormatting())).IgnoreArguments().Return("subject");
			var meetings = new[] {meeting};
			meeting.Stub(x => x.BelongsToMeeting).Return(belongsToMeeting);
			meeting.Stub(x => x.Period).Return(new DateTimePeriod(now.ToUniversalTime(), now.ToUniversalTime().AddHours(1)));
			meeting.Stub(x => x.Optional).Return(true);

			var scheduleDay = stubs.ScheduleDayStub(DateTime.Today);
			scheduleDay.Stub(x => x.PersonMeetingCollection()).Return(meetings);

			var result = _target.Map(scheduleDay, null);

			result.Meetings.Count().Should().Be(1);
			result.Meetings.First().Subject.Should().Be("subject");
			result.Meetings.First().TimeSpan.Should().Be(now.ToShortTimeString()+" - " + now.AddHours(1).ToShortTimeString());
			result.Meetings.First().IsOptional.Should().Be.True();
		}

		[Test]
		public void ShouldMapPersonalShifts()
		{
			var now =new DateTime(2000,1,1,10,0,0);
			var stubs = new StubFactory();
			var personAssignment = MockRepository.GenerateMock<IPersonAssignment>();
			var payload = MockRepository.GenerateMock<IActivity>();
			payload.Stub(x => x.ConfidentialDescription_DONTUSE(new Person())).IgnoreArguments().Return(new Description("activity"));
			
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Today);
			personAssignment.Stub(x => x.PersonalActivities()).Return(new List<PersonalShiftLayer>{new PersonalShiftLayer(payload, new DateTimePeriod(now.ToUniversalTime(), now.ToUniversalTime().AddHours(1)))});
			scheduleDay.Stub(x => x.PersonAssignment()).Return(personAssignment);

			var result = _target.Map(scheduleDay, null);

			result.PersonalShifts.Count().Should().Be(1);
			result.PersonalShifts.First().Subject.Should().Be("activity");
			result.PersonalShifts.First().TimeSpan.Should().Be(now.ToShortTimeString() + " - " + now.AddHours(1).ToShortTimeString());
		}

		[Test]
		public void ShouldNotMapPersonalShiftsToTime()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(),new Scenario("s"),new DateOnly(2011,5,18));
			var period = new DateTimePeriod(2011,5,18,7,2011,5,18,16);
			personAssignment.AddActivity(new Activity("a") { InWorkTime = true },period);
			personAssignment.SetShiftCategory(new ShiftCategory("sc"));
			personAssignment.AddPersonalActivity(new Activity("b") { InWorkTime = true },period.MovePeriod(TimeSpan.FromHours(-2)));

			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011,5,18),SchedulePartView.MainShift,personAssignment);

			var contractTime = TimeSpan.FromHours(8);
			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.ContractTime()).Return(contractTime);
			_projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var result = _target.Map(scheduleDay, null);
			result.PersonAssignment.TimeSpan.Should().Be.EqualTo(period.TimePeriod(scheduleDay.TimeZone).ToShortTimeString());
		}
	}
}