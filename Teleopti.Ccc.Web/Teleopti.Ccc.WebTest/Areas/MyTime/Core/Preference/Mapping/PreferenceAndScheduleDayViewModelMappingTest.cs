using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceAndScheduleDayViewModelMappingTest
	{
		private IProjectionProvider _projectionProvider;
		private IUserTimeZone _userTimeZone;
		private TimeZoneInfo _timeZone;
		private IPreferenceOptionsProvider _preferenceOptionsProvider;
		private IToggleManager _toggleManager;

		[SetUp]
		public void Setup()
		{
			_projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			_preferenceOptionsProvider = MockRepository.GenerateMock<IPreferenceOptionsProvider>();
			_toggleManager = MockRepository.GenerateMock<IToggleManager>();
			_timeZone = TimeZoneInfo.Local;
			_userTimeZone.Stub(x => x.TimeZone()).Return(_timeZone);
			Mapper.Reset();
			Mapper.Initialize(c =>
				{
					c.AddProfile(new PreferenceAndScheduleDayViewModelMappingProfile(_projectionProvider, _userTimeZone));
					c.AddProfile(new PreferenceDayViewModelMappingProfile(MockRepository.GenerateMock<IExtendedPreferencePredicate>()));
					c.AddProfile(new PreferenceViewModelMappingProfile(new FakePermissionProvider(), () => _preferenceOptionsProvider, _toggleManager, new Now()));
					c.AddProfile(new CommonViewModelMappingProfile());
				});
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapDate()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.Date.Should().Be(DateOnly.Today.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPreferenceViewModel()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today);
			var preferenceRestriction = new PreferenceRestriction();
			preferenceRestriction.DayOffTemplate = new DayOffTemplate(new Description("DO"));
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);
			var personRestrictionCollection =
				new ReadOnlyObservableCollection<IScheduleData>(new ObservableCollection<IScheduleData>(new[] {preferenceDay}));

			scheduleDay.Stub(x => x.PersonRestrictionCollection()).Return(personRestrictionCollection);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.Preference.Should().Not.Be.Null();
			result.Preference.Preference.Should().Be("DO");
		}

		[Test]
		public void ShouldMapDayOffViewModel()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today, SchedulePartView.DayOff,
			                                                    PersonAssignmentFactory.CreateAssignmentWithDayOff(
				                                                    new Scenario(" "), new Person(), DateOnly.Today,
				                                                    new DayOffTemplate(new Description("DO"))));

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.DayOff.Should().Not.Be.Null();
			result.DayOff.DayOff.Should().Be("DO");
		}

		[Test]
		public void ShouldMapAbsenceViewModel()
		{
			var stubs = new StubFactory();
			var absence = stubs.PersonAbsenceStub("Illness");
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today, SchedulePartView.FullDayAbsence, absence);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

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

			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today, SchedulePartView.FullDayAbsence, absence);

			_projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.Absence.Should().Not.Be.Null();
			result.Absence.AbsenceContractTime.Should().Be(TimeHelper.GetLongHourMinuteTimeString(contractTime, CultureInfo.CurrentUICulture));
			result.Absence.AbsenceContractTimeMinutes.Should().Be(contractTime.TotalMinutes);
		}

		[Test]
		public void ShouldMapShiftCategoryInPersonAssignmentDayViewModel()
		{
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "), new DateOnly(2000,1,1));
			personAssignment.AddActivity(new Activity("sdf"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
			personAssignment.SetShiftCategory(new ShiftCategory("shiftCategory"));
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today, SchedulePartView.MainShift, personAssignment);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.PersonAssignment.Should().Not.Be.Null();
			result.PersonAssignment.ShiftCategory.Should().Be("shiftCategory");
		}

		[Test]
		public void ShouldMapContractTimeInPersonAssignmentDayViewModel()
		{
			var contractTime = TimeSpan.FromHours(8);
			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.ContractTime()).Return(contractTime);
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "), new DateOnly(2000,1,1));
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today, SchedulePartView.MainShift, personAssignment);
			_projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.PersonAssignment.Should().Not.Be.Null();
			result.PersonAssignment.ContractTime.Should().Be(TimeHelper.GetLongHourMinuteTimeString(contractTime, CultureInfo.CurrentUICulture));
		}

		[Test]
		public void ShouldMapContractTimeMinutesInPersonAssignmentDayViewModel()
		{
			var contractTime = TimeSpan.FromHours(8);
			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.ContractTime()).Return(contractTime);
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "), new DateOnly(2000, 1, 1));
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today, SchedulePartView.MainShift, personAssignment);
			_projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.PersonAssignment.Should().Not.Be.Null();
			result.PersonAssignment.ContractTimeMinutes.Should().Be(contractTime.TotalMinutes);
		}

		[Test]
		public void ShouldMapTimeSpanInPersonAssignmentDayViewModel()
		{
			var stubs = new StubFactory();
			var period = new DateTimePeriod(new DateTime(2012, 2, 21, 7, 0, 0, DateTimeKind.Utc), new DateTime(2012, 2, 21, 16, 0, 0, DateTimeKind.Utc));
			var personAssignment = stubs.PersonAssignmentStub(period);
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today, SchedulePartView.MainShift, personAssignment);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.PersonAssignment.Should().Not.Be.Null();
			result.PersonAssignment.TimeSpan.Should().Be(period.TimePeriod(scheduleDay.TimeZone).ToShortTimeString());
		}

		[Test]
		public void ShouldFlagFeedbackIfInsidePeriodAndNotScheduled()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today);
			scheduleDay.Stub(x => x.IsScheduled()).Return(false);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.Feedback.Should().Be.True();
		}

		[Test]
		public void ShouldNotFlagFeedbackIfScheduled()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today);
			scheduleDay.Stub(x => x.IsScheduled()).Return(true);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.Feedback.Should().Be.False();
		}

		[Test]
		public void ShouldMapDayOffStyleClassName()
		{
			var stubs = new StubFactory();
			var scheduleDay = stubs.ScheduleDayStub(DateOnly.Today, SchedulePartView.DayOff, PersonAssignmentFactory.CreateAssignmentWithDayOff());

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.StyleClassName.Should().Contain(StyleClasses.DayOff);
			result.StyleClassName.Should().Contain(StyleClasses.Striped);
		}

		[Test]
		public void ShouldMapContractDayOffStyleClassName()
		{
			var stubs = new StubFactory();
			var scheduleDay = stubs.ScheduleDayStub(DateOnly.Today, SchedulePartView.ContractDayOff, PersonAssignmentFactory.CreateAssignmentWithDayOff());

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.StyleClassName.Should().Contain(StyleClasses.Striped);
		}

		[Test]
		public void ShouldMapAbsenceBorderColor()
		{
			var stubs = new StubFactory();
			var personAbsence = stubs.PersonAbsenceStub(new DateTimePeriod(), stubs.AbsenceLayerStub(stubs.AbsenceStub(Color.DarkMagenta)));
			var scheduleDay = stubs.ScheduleDayStub(DateOnly.Today, SchedulePartView.FullDayAbsence, personAbsence);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.BorderColor.Should().Be(Color.DarkMagenta.ToHtml());
		}

		[Test]
		public void ShouldMapPersonAssignmentsBorderColor()
		{
			var stubs = new StubFactory();
			var personAssignment = stubs.PersonAssignmentStub(new DateTimePeriod(), stubs.ShiftCategoryStub(Color.Coral));
			var scheduleDay = stubs.ScheduleDayStub(DateOnly.Today, SchedulePartView.MainShift, personAssignment);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

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
			var meetings = new ReadOnlyCollection<IPersonMeeting>(new[] {meeting});
			meeting.Stub(x => x.BelongsToMeeting).Return(belongsToMeeting);
			meeting.Stub(x => x.Period).Return(new DateTimePeriod(now.ToUniversalTime(), now.ToUniversalTime().AddHours(1)));
			meeting.Stub(x => x.Optional).Return(true);

			var scheduleDay = stubs.ScheduleDayStub(DateOnly.Today);
			scheduleDay.Stub(x => x.PersonMeetingCollection()).Return(meetings);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.Meetings.Count().Should().Be(1);
			result.Meetings.First().Subject.Should().Be("subject");
			result.Meetings.First().TimeSpan.Should().Be(now.ToShortTimeString()+" - " + now.AddHours(1).ToShortTimeString());
			result.Meetings.First().IsOptional.Should().Be.True();
		}

		[Test]
		public void ShouldMapPersonalShifts()
		{
			var now = DateTime.Now;
			var stubs = new StubFactory();
			var personAssignment = MockRepository.GenerateMock<IPersonAssignment>();
			var activityLayer = MockRepository.GenerateMock<IPersonalShiftLayer>();
			var payload = MockRepository.GenerateMock<IActivity>();
			payload.Stub(x => x.ConfidentialDescription(new Person(),DateOnly.Today)).IgnoreArguments().Return(new Description("activity"));
			activityLayer.Stub(x => x.Payload).Return(payload);
			activityLayer.Stub(x => x.Period).Return(new DateTimePeriod(now.ToUniversalTime(), now.ToUniversalTime().AddHours(1)));

			var scheduleDay = stubs.ScheduleDayStub(DateOnly.Today);
			personAssignment.Stub(x => x.PersonalActivities()).Return(new List<IPersonalShiftLayer>{activityLayer});
			scheduleDay.Stub(x => x.PersonAssignment()).Return(personAssignment);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.PersonalShifts.Count().Should().Be(1);
			result.PersonalShifts.First().Subject.Should().Be("activity");
			result.PersonalShifts.First().TimeSpan.Should().Be(now.ToShortTimeString() + " - " + now.AddHours(1).ToShortTimeString());
		}

	}
}