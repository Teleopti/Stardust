using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.WebTest.Core.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceAndScheduleDayViewModelMappingTest
	{
		private IProjectionProvider _projectionProvider;
		private IPreferenceFulfilledChecker _preferenceFulfilledChecker;


		[SetUp]
		public void Setup()
		{
			_projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
			_preferenceFulfilledChecker = MockRepository.GenerateMock<IPreferenceFulfilledChecker>();
			Mapper.Reset();
			Mapper.Initialize(c =>
				{
					c.AddProfile(new PreferenceAndScheduleDayViewModelMappingProfile(
						Depend.On(_projectionProvider), Depend.On(_preferenceFulfilledChecker)));
					c.AddProfile(new PreferenceDayViewModelMappingProfile(
						Depend.On(MockRepository.GenerateMock<IExtendedPreferencePredicate>()
						)));
					c.AddProfile(new PreferenceViewModelMappingProfile());
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
			var dayOff = new PersonDayOff(new Person(), new Scenario(" "), new DayOffTemplate(new Description("DO")), DateOnly.Today);
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today, SchedulePartView.DayOff, dayOff);

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
		public void ShouldMapShiftCategoryInPersonAssignmentDayViewModel()
		{
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "));
			personAssignment.SetMainShift(new MainShift(new ShiftCategory("shiftCategory")));
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
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "));
			personAssignment.SetMainShift(new MainShift(new ShiftCategory("shiftCategory")));
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
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "));
			personAssignment.SetMainShift(new MainShift(new ShiftCategory("shiftCategory")));
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
			personAssignment.SetMainShift(new MainShift(new ShiftCategory("shiftCategory")));
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today, SchedulePartView.MainShift, personAssignment);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.PersonAssignment.Should().Not.Be.Null();
			result.PersonAssignment.TimeSpan.Should().Be(period.TimePeriod(scheduleDay.TimeZone).ToShortTimeString());
		}


		[Test]
		public void ShouldMapFulfilled()
		{
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "));
			personAssignment.SetMainShift(new MainShift(new ShiftCategory("shiftCategory")));
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today, SchedulePartView.MainShift, personAssignment);
			scheduleDay.Stub(x => x.IsScheduled()).Return(true);

			_preferenceFulfilledChecker.Stub(x => x.IsPreferenceFulfilled(scheduleDay)).Return(true);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			var fulfilled = result.Fulfilled;
			(fulfilled != null && fulfilled.Value).Should().Be(true);
		}

		[Test]
		public void ShouldMapFulfilledForNonScheduledDay()
		{
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "));
			personAssignment.SetMainShift(new MainShift(new ShiftCategory("shiftCategory")));
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today, SchedulePartView.MainShift, personAssignment);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.Fulfilled.Should().Be(null);
		}

		[Test]
		public void ShouldFlagFeedbackIfInsidePeriodAndNotScheduled()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today);
			scheduleDay.Stub(x => x.IsScheduled()).Return(false);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.Feedback.Should().Be.True();
		}

		//[Test]
		//public void ShouldFlagFeedbackIfInsidePeriodAndNoScheduleDay()
		//{
		//    var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(null);

		//    result.Feedback.Should().Be.True();
		//}

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
			var scheduleDay = stubs.ScheduleDayStub(DateOnly.Today, SchedulePartView.DayOff, stubs.PersonDayOffStub());

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.StyleClassName.Should().Contain(StyleClasses.DayOff);
			result.StyleClassName.Should().Contain(StyleClasses.Striped);
		}

		[Test]
		public void ShouldMapContractDayOffStyleClassName()
		{
			var stubs = new StubFactory();
			var scheduleDay = stubs.ScheduleDayStub(DateOnly.Today, SchedulePartView.ContractDayOff, stubs.PersonDayOffStub());

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
			var personAssignment = stubs.PersonAssignmentStub(new DateTimePeriod(), stubs.MainShiftStub(stubs.ShiftCategoryStub(Color.Coral)));
			var scheduleDay = stubs.ScheduleDayStub(DateOnly.Today, SchedulePartView.MainShift, personAssignment);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.BorderColor.Should().Be(Color.Coral.ToHtml());
		}

	}
}