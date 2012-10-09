﻿using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.WebTest.Core.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceViewModelMappingTest
	{
		private PreferenceDomainData data;
		private IScheduleColorProvider scheduleColorProvider;
		private IPreferenceFulfilledChecker preferenceFulfilledChecker;

		[SetUp]
		public void Setup()
		{
			scheduleColorProvider = MockRepository.GenerateMock<IScheduleColorProvider>();
			preferenceFulfilledChecker = MockRepository.GenerateMock<IPreferenceFulfilledChecker>();

			data = new PreferenceDomainData
			       	{
			       		SelectedDate = DateOnly.Today,
			       		Period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1)),
			       		Days = new[] {new PreferenceDayDomainData {Date = DateOnly.Today}},
			       		WorkflowControlSet =
			       			new WorkflowControlSet(null)
			       				{
			       					PreferencePeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1)),
			       					PreferenceInputPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today)
			       				}
			       	};

			Mapper.Reset();
			Mapper.Initialize(c =>
			                  	{
			                  		c.AddProfile(new PreferenceViewModelMappingProfile(
			                  		             	Depend.On(scheduleColorProvider),
													Depend.On(preferenceFulfilledChecker)
													));
									c.AddProfile(new CommonViewModelMappingProfile());
			                  	});
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapPeriodSelectionDate()
		{
			data.SelectedDate = DateOnly.Today;

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.PeriodSelection.Date.Should().Be.EqualTo(data.SelectedDate.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPeriodSelectionDisplay()
		{
			data.Period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1));

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.PeriodSelection.Display.Should().Be.EqualTo(data.Period.DateString);
		}

		[Test]
		public void ShouldFillPeriodSelectionNavigation()
		{
			data.Period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1));

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.PeriodSelection.Navigation.CanPickPeriod.Should().Be.True();
			result.PeriodSelection.Navigation.HasNextPeriod.Should().Be.True();
			result.PeriodSelection.Navigation.HasPrevPeriod.Should().Be.True();
			result.PeriodSelection.Navigation.FirstDateNextPeriod.Should().Be.EqualTo(data.Period.EndDate.AddDays(1).ToFixedClientDateOnlyFormat());
			result.PeriodSelection.Navigation.LastDatePreviousPeriod.Should().Be.EqualTo(data.Period.StartDate.AddDays(-1).ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldFillPeriodSelectaleDateRange()
		{
			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.PeriodSelection.SelectableDateRange.MinDate.Should().Be.EqualTo(new DateOnly(CultureInfo.CurrentCulture.Calendar.MinSupportedDateTime).ToFixedClientDateOnlyFormat());
			result.PeriodSelection.SelectableDateRange.MaxDate.Should().Be.EqualTo(new DateOnly(CultureInfo.CurrentCulture.Calendar.MaxSupportedDateTime).ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPeriodSelectedDateRange()
		{
			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.PeriodSelection.SelectedDateRange.MinDate.Should().Be.EqualTo(data.Period.StartDate.ToFixedClientDateOnlyFormat());
			result.PeriodSelection.SelectedDateRange.MaxDate.Should().Be.EqualTo(data.Period.EndDate.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldFillWeekDayHeaders()
		{
			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.WeekDayHeaders.Should().Have.Count.EqualTo(7);
			result.WeekDayHeaders.Select(x => x.Title).Should().Have.SameSequenceAs(DateHelper.GetWeekdayNames(CultureInfo.CurrentCulture));
		}

		[Test]
		public void ShouldFillWeekViewModelsWithDisplayedPeriod()
		{
			data.SelectedDate = DateOnly.Today;
			data.Period = new DateOnlyPeriod(data.SelectedDate.AddDays(-7), data.SelectedDate.AddDays(7).AddDays(-1));
			var firstDisplayedDate = new DateOnly(DateHelper.GetFirstDateInWeek(data.Period.StartDate, CultureInfo.CurrentCulture).AddDays(-7));
			var lastDisplayedDate = new DateOnly(DateHelper.GetLastDateInWeek(data.Period.EndDate, CultureInfo.CurrentCulture).AddDays(7));
			// get number of weeks for period. Why cant working with weeks ever be easy...
			var firstDateNotShown = lastDisplayedDate.AddDays(1);
			var shownTime = firstDateNotShown.Date.Subtract(firstDisplayedDate);
			var shownWeeks = shownTime.Days / 7;

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.Weeks.Should().Have.Count.EqualTo(shownWeeks);
		}

		[Test]
		public void ShouldFillDayViewModels()
		{
			data.Period = new DateOnlyPeriod(data.SelectedDate.AddDays(-7), data.SelectedDate.AddDays(7).AddDays(-1));

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.Weeks.ForEach(
				week =>
					{
						week.Days.Should().Have.Count.EqualTo(7);
						week.Days.ElementAt(0).Date.DayOfWeek.Should().Be(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
					}
				);
		}

		[Test]
		public void ShouldFillDayViewModelDate()
		{
			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate).Date.Should().Be(data.SelectedDate);
		}

		[Test]
		public void ShouldFillDayViewHeaderDayNumber()
		{
			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate).Header.DayNumber.Should().Be(data.SelectedDate.Day.ToString());
		}

		[Test]
		public void ShouldFillDayViewHeaderWithMonthNameWhenFirstDayOfMonth()
		{
			var firstDateInMonth = new DateOnly(data.SelectedDate.Year, data.SelectedDate.Month, 1);
			data.SelectedDate = firstDateInMonth;
			data.Period = new DateOnlyPeriod(firstDateInMonth, firstDateInMonth);
			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate).Header.DayDescription
				.Should().Be(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(data.SelectedDate.Month));
		}

		[Test]
		public void ShouldNotFillDayViewHeaderWithMonthWhenNotFirstDayOfMonth()
		{
			var dateThatIsNotTheFirstInThePeriodAndNotFirstOfMonth = new DateOnly(2011, 9, 8);
			data.SelectedDate = dateThatIsNotTheFirstInThePeriodAndNotFirstOfMonth;
			data.Period = new DateOnlyPeriod(2011, 9, 5, 2011, 9, 11);

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.Header.DayDescription.Should().Be.Empty();
		}

		[Test]
		public void ShouldFillDayViewHeaderWithMonthNameForFirstDayOfDisplayedPeriod()
		{
			var firstDisplayedDate = new DateOnly(DateHelper.GetFirstDateInWeek(data.Period.StartDate, CultureInfo.CurrentCulture).AddDays(-7));

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(firstDisplayedDate)
				.Header.DayDescription.Should().Be.EqualTo(
				CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(firstDisplayedDate.Month));
		}

		[Test]
		public void ShouldSetEditable()
		{
			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.Editable.Should().Be.True();
		}

		[Test]
		public void ShouldNotSetEditableWhenScheduled()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(data.SelectedDate);
			data.Days = new[] {new PreferenceDayDomainData {Date = data.SelectedDate, ScheduleDay = scheduleDay}};

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.Editable.Should().Be.False();
		}

		[Test]
		public void ShouldNotBeEditablWhenOutsideSchedulePeriod()
		{
			var outsideDate = data.Period.EndDate.AddDays(1);
			data.SelectedDate = outsideDate;

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.Editable.Should().Be.False();
		}

		[Test]
		public void ShouldNotBeEditablWhenNoWorkflowControlSet()
		{
			data.WorkflowControlSet = null;

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.Editable.Should().Be.False();
		}

		[Test]
		public void ShouldNotBeEditableWhenOutsidePreferenceInputPeriod()
		{
			var closedDate = DateOnly.Today.AddDays(1);
			data.WorkflowControlSet.PreferenceInputPeriod = new DateOnlyPeriod(closedDate, closedDate);
			data.WorkflowControlSet.PreferencePeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.Editable.Should().Be.False();
		}

		[Test]
		public void ShouldNotBeEditableWhenOutsidePreferencePeriod()
		{
			var closedDate = DateOnly.Today.AddDays(1);
			data.WorkflowControlSet.PreferenceInputPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			data.WorkflowControlSet.PreferencePeriod = new DateOnlyPeriod(closedDate, closedDate);

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.Editable.Should().Be.False();
		}

		[Test]
		public void ShouldFlagFeedbackIfInsidePeriodAndNotScheduled()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(data.SelectedDate);
			scheduleDay.Stub(x => x.IsScheduled()).Return(false);
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.Feedback.Should().Be.True();
		}

		[Test]
		public void ShouldFlagFeedbackIfInsidePeriodAndNoScheduleDay()
		{
			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.Feedback.Should().Be.True();
		}

		[Test]
		public void ShouldNotFlagFeedbackIfOutsidePeriodAndNotScheduled()
		{
			data.SelectedDate = DateOnly.Today.AddDays(-1);
			data.Period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var scheduleDay = new StubFactory().ScheduleDayStub(data.SelectedDate);
			scheduleDay.Stub(x => x.IsScheduled()).Return(false);
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.Feedback.Should().Be.False();
		}

		[Test]
		public void ShouldNotFlagFeedbackIfScheduled()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(data.SelectedDate);
			scheduleDay.Stub(x => x.IsScheduled()).Return(true);
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.Feedback.Should().Be.False();
		}

		[Test]
		public void ShouldFlagInPeriodWhenInsideSchedulePeriod()
		{
			data.SelectedDate = data.Period.StartDate;

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.InPeriod.Should().Be.True();
		}

		[Test]
		public void ShouldNotFlagInPeriodWhenOutsideSchedulePeriod()
		{
			var outsideDate = data.Period.EndDate.AddDays(1);
			data.SelectedDate = outsideDate;

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.InPeriod.Should().Be.False();
		}

		[Test]
		public void ShouldMapPersonAssignmentShiftCategory()
		{
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "));
			personAssignment.SetMainShift(new MainShift(new ShiftCategory("shiftCategory")));
			var scheduleDay = new StubFactory().ScheduleDayStub(data.SelectedDate, SchedulePartView.MainShift, personAssignment);
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.PersonAssignment.ShiftCategory.Should().Be(personAssignment.MainShift.ShiftCategory.Description.Name);
		}

		[Test]
		public void ShouldMapPersonAssignmentContractTime()
		{
			var contractTime = TimeSpan.FromHours(8);
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "));
			var scheduleDay = new StubFactory().ScheduleDayStub(data.SelectedDate, SchedulePartView.MainShift, personAssignment);
			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.ContractTime()).Return(contractTime);
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay, Projection = projection } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.PersonAssignment.ContractTime.Should().Be(TimeHelper.GetLongHourMinuteTimeString(contractTime, CultureInfo.CurrentUICulture));
		}

		[Test]
		public void ShouldMapPersonAssignmentContractTimeMinutes()
		{
			var contractTime = TimeSpan.FromHours(8);
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "));
			var scheduleDay = new StubFactory().ScheduleDayStub(data.SelectedDate, SchedulePartView.MainShift, personAssignment);
			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.ContractTime()).Return(contractTime);
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay, Projection = projection } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.PersonAssignment.ContractTimeMinutes.Should().Be(contractTime.TotalMinutes);
		}

		[Test]
		public void ShouldMapPersonAssignmentTimeSpan()
		{
			data.SelectedDate = new DateOnly(2012, 2, 21);
			data.Period = new DateOnlyPeriod(data.SelectedDate, data.SelectedDate);
			var stubs = new StubFactory();
			var personAssignment = stubs.PersonAssignmentStub(new DateTimePeriod(new DateTime(2012, 2, 21, 7, 0, 0, DateTimeKind.Utc),
			                                                                     new DateTime(2012, 2, 21, 16, 0, 0, DateTimeKind.Utc)));
			var scheduleDay = stubs.ScheduleDayStub(data.SelectedDate, SchedulePartView.MainShift, personAssignment);
			data.Days = new[] {new PreferenceDayDomainData {Date = data.SelectedDate, ScheduleDay = scheduleDay}};

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.PersonAssignment.TimeSpan.Should().Be(new TimePeriod(8, 0, 17, 0).ToShortTimeString());
		}

		[Test]
		public void ShouldMapPersonAssignmentsStyleClassNameFromDisplayColor()
		{
			var stubs = new StubFactory();
			var personAssignment = stubs.PersonAssignmentStub(new DateTimePeriod(), stubs.MainShiftStub(stubs.ShiftCategoryStub(Color.Coral)));
			var scheduleDay = stubs.ScheduleDayStub(data.SelectedDate, SchedulePartView.MainShift, personAssignment);
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.BorderColor.Should().Be(Color.Coral.ToHtml());
		}

		[Test]
		public void ShouldOnlyMapPersonAssignmentWhenPersonAssignment()
		{
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "));
			personAssignment.SetMainShift(new MainShift(new ShiftCategory("shiftCategory")));
			var scheduleDay = new StubFactory().ScheduleDayStub(data.SelectedDate, SchedulePartView.MainShift, personAssignment);
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.PersonAssignment.Should().Not.Be.Null();
			result.DayViewModel(data.SelectedDate)
				.DayOff.Should().Be.Null();
			result.DayViewModel(data.SelectedDate)
				.Absence.Should().Be.Null();
		}

		[Test]
		public void ShouldMapDayOff()
		{
			var stubs = new StubFactory();
			var dayOff = stubs.PersonDayOffStub();
			var scheduleDay = stubs.ScheduleDayStub(data.SelectedDate, SchedulePartView.DayOff, dayOff);
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.DayOff.DayOff.Should().Be(dayOff.DayOff.Description.Name);
		}

		[Test]
		public void ShouldMapDayOffStyleClassName()
		{
			var stubs = new StubFactory();
			var scheduleDay = stubs.ScheduleDayStub(data.SelectedDate, SchedulePartView.DayOff, stubs.PersonDayOffStub());
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.StyleClassName.Should().Contain(StyleClasses.DayOff);
			result.DayViewModel(data.SelectedDate)
				.StyleClassName.Should().Contain(StyleClasses.Striped);
		}

		[Test]
		public void ShouldOnlyMapDayOffWhenDayOff()
		{
			var stubs = new StubFactory();
			var dayOff = stubs.PersonDayOffStub();
			var scheduleDay = stubs.ScheduleDayStub(data.SelectedDate, SchedulePartView.DayOff, dayOff);
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.DayOff.Should().Not.Be.Null();
			result.DayViewModel(data.SelectedDate)
				.PersonAssignment.Should().Be.Null();
			result.DayViewModel(data.SelectedDate)
				.Absence.Should().Be.Null();
		}

		[Test]
		public void ShouldMapAbsence()
		{
			var stubs = new StubFactory();
			var absenceToDisplay = stubs.PersonAbsenceStub();
			var lowPriorityAbsence = stubs.PersonAbsenceStub();
			var scheduleDay = stubs.ScheduleDayStub(data.SelectedDate, SchedulePartView.FullDayAbsence, new[] { absenceToDisplay, lowPriorityAbsence });
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.Absence.Absence.Should().Be(absenceToDisplay.Layer.Payload.Description.Name);
		}

		[Test]
		public void ShouldMapAbsenceStyleClassNameFromDisplayColor()
		{
			var stubs = new StubFactory();
			var personAbsence = stubs.PersonAbsenceStub(new DateTimePeriod(), stubs.AbsenceLayerStub(stubs.AbsenceStub(Color.DarkMagenta)));
			var scheduleDay = stubs.ScheduleDayStub(data.SelectedDate, SchedulePartView.FullDayAbsence, personAbsence);
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate).BorderColor.Should().Be(Color.DarkMagenta.ToHtml());
		}

		[Test]
		public void ShouldMapAbsenceStyleClassNameStripedWhenOverDayOff()
		{
			var stubs = new StubFactory();
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Now.Date, SchedulePartView.ContractDayOff, stubs.PersonAbsenceStub());
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.StyleClassName.Should().Contain(StyleClasses.Striped);
		}

		[Test]
		public void ShouldOnlyMapAbsenceWhenAbsence()
		{
			var stubs = new StubFactory();
			var absence = stubs.PersonAbsenceStub();
			var scheduleDay = stubs.ScheduleDayStub(data.SelectedDate, SchedulePartView.FullDayAbsence, new[] { absence });
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.Absence.Should().Not.Be.Null();
			result.DayViewModel(data.SelectedDate)
				.DayOff.Should().Be.Null();
			result.DayViewModel(data.SelectedDate)
				.PersonAssignment.Should().Be.Null();
		}

		[Test]
		public void ShouldMapFulfilled()
		{
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "));
			personAssignment.SetMainShift(new MainShift(new ShiftCategory("shiftCategory")));
			var scheduleDay = new StubFactory().ScheduleDayStub(data.SelectedDate, SchedulePartView.MainShift, personAssignment);
			scheduleDay.Stub(x => x.IsScheduled()).Return(true);
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay } };
			preferenceFulfilledChecker.Stub(x => x.IsPreferenceFulfilled(scheduleDay)).Return(true);

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			var fulfilled = result.DayViewModel(data.SelectedDate).Fulfilled;
			(fulfilled != null && fulfilled.Value).Should().Be(true);
		}

		[Test]
		public void ShouldMapFulfilledForScheduledDay()
		{
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "));
			personAssignment.SetMainShift(new MainShift(new ShiftCategory("shiftCategory")));
			var scheduleDay = new StubFactory().ScheduleDayStub(data.SelectedDate, SchedulePartView.MainShift, personAssignment);
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay } };
			preferenceFulfilledChecker.Stub(x => x.IsPreferenceFulfilled(scheduleDay)).Return(true);

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel(data.SelectedDate)
				.Fulfilled.Should().Be(null);
		}



		[Test]
		public void ShouldMapPreferencePeriod()
		{
			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.PreferencePeriod.Period.Should().Be.EqualTo(data.WorkflowControlSet.PreferencePeriod.DateString);
		}

		[Test]
		public void ShouldMapPreferenceInputPeriod()
		{
			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.PreferencePeriod.OpenPeriod.Should().Be.EqualTo(data.WorkflowControlSet.PreferenceInputPeriod.DateString);
		}

		[Test]
		public void ShouldMapNullPreferencePeriodWhenNoWorkflowControlSet()
		{
			data.WorkflowControlSet = null;

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.PreferencePeriod.Should().Be.Null();
		}

		[Test]
		public void ShouldMapStyleClassViewModelsFromScheduleColors()
		{
			var colors = new[] { Color.Red, Color.Blue };

			scheduleColorProvider.Stub(x => x.GetColors(data.ColorSource)).Return(colors);

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.Styles.Select(s => s.Name)
				.Should().Have.SameValuesAs(new[] { Color.Blue.ToStyleClass(), Color.Red.ToStyleClass() });
			result.Styles.Select(s => s.ColorHex)
				.Should().Have.SameValuesAs(new[] { Color.Blue.ToHtml(), Color.Red.ToHtml() });
		}

	}

	public static class Extensions
	{
		public static DayViewModel DayViewModel(this PreferenceViewModel viewModel, DateOnly date)
		{
			return (from w in viewModel.Weeks
					from d in w.Days
					where d.Date == date
					select d)
				.Single();
		}

	}

}