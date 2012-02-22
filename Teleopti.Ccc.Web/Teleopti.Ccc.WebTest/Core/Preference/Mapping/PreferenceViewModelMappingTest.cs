﻿using System;
using System.Collections.Generic;
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
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Preference.Mapping
{

	[TestFixture]
	public class PreferenceViewModelMappingTest
	{
		private PreferenceDomainData data;

		[SetUp]
		public void Setup()
		{

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
			Mapper.Initialize(c => c.AddProfile(new PreferenceViewModelMappingProfile()));
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
			var dayData = new PreferenceViewModelMappingProfile.PreferenceDayMappingData
			           	{
							Date = data.SelectedDate,
							Period = data.Period
			           	};

			var result = Mapper.Map<PreferenceViewModelMappingProfile.PreferenceDayMappingData, PreferenceDayViewModel>(dayData);

			result.Date.Should().Be(data.SelectedDate);
		}

		[Test]
		public void ShouldFillDayViewHeaderDayNumber()
		{
			var dayData = new PreferenceViewModelMappingProfile.PreferenceDayMappingData
			              	{
			              		Date = data.SelectedDate,
			              		Period = data.Period
			              	};

			var result = Mapper.Map<PreferenceViewModelMappingProfile.PreferenceDayMappingData, DayViewModelBase>(dayData);

			result.Header.DayNumber.Should().Be(data.SelectedDate.Day.ToString());
		}

		[Test]
		public void ShouldFillDayViewHeaderWithMonthNameWhenFirstDayOfMonth()
		{
			var firstDateInMonth = new DateOnly(data.SelectedDate.Year, data.SelectedDate.Month, 1);
			var dayData = new PreferenceViewModelMappingProfile.PreferenceDayMappingData
			              	{
			              		Date = firstDateInMonth,
			              		Period = data.Period
			              	};

			var result = Mapper.Map<PreferenceViewModelMappingProfile.PreferenceDayMappingData, DayViewModelBase>(dayData);

			result.Header.DayDescription.Should().Be.EqualTo(
				CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(data.SelectedDate.Month));
		}

		[Test]
		public void ShouldNotFillDayViewHeaderWithMonthWhenNotFirstDayOfMonth()
		{
			var dateThatIsNotTheFirstInThePeriodAndNotFirstOfMonth = new DateOnly(2011, 9, 8);
			data.SelectedDate = dateThatIsNotTheFirstInThePeriodAndNotFirstOfMonth;
			data.Period = new DateOnlyPeriod(2011, 9, 5, 2011, 9, 11);
			var dayData = new PreferenceViewModelMappingProfile.PreferenceDayMappingData
			              	{
			              		Date = data.SelectedDate,
			              		Period = data.Period
			              	};

			var result = Mapper.Map<PreferenceViewModelMappingProfile.PreferenceDayMappingData, DayViewModelBase>(dayData);

			result.Header.DayDescription.Should().Be.Empty();
		}

		[Test]
		public void ShouldFillDayViewHeaderWithMonthNameForFirstDayOfDisplayedPeriod()
		{
			var firstDisplayedDate = new DateOnly(DateHelper.GetFirstDateInWeek(data.Period.StartDate, CultureInfo.CurrentCulture).AddDays(-7));
			var firstDisplayedDateOnly = new DateOnly(firstDisplayedDate);
			var dayData = new PreferenceViewModelMappingProfile.PreferenceDayMappingData
			              	{
			              		Date = firstDisplayedDateOnly,
			              		Period = data.Period
			              	};

			var result = Mapper.Map<PreferenceViewModelMappingProfile.PreferenceDayMappingData, DayViewModelBase>(dayData);

			result.Header.DayDescription.Should().Be.EqualTo(
				CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(firstDisplayedDateOnly.Month));
		}

		[Test]
		public void ShouldSetEditable()
		{
			var dayData = new PreferenceViewModelMappingProfile.PreferenceDayMappingData
			              	{
			              		Date = data.SelectedDate,
			              		Period = data.Period,
			              		WorkflowControlSet = data.WorkflowControlSet
			              	};

			var result = Mapper.Map<PreferenceViewModelMappingProfile.PreferenceDayMappingData, DayViewModelBase>(dayData);

			result.Editable.Should().Be.True();
		}

		[Test]
		public void ShouldNotSetEditableWhenScheduled()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(data.SelectedDate);
			data.Days = new[] {new PreferenceDayDomainData {Date = data.SelectedDate, ScheduleDay = scheduleDay}};

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel<ScheduledDayViewModel>(data.SelectedDate).Editable.Should().Be.False();
		}

		[Test]
		public void ShouldNotBeEditablWhenOutsideSchedulePeriod()
		{
			var outsideDate = data.Period.EndDate.AddDays(1);
			var dayData = new PreferenceViewModelMappingProfile.PreferenceDayMappingData
			              	{
			              		Date = outsideDate,
			              		Period = data.Period
			              	};

			var result = Mapper.Map<PreferenceViewModelMappingProfile.PreferenceDayMappingData, DayViewModelBase>(dayData);

			result.Editable.Should().Be.False();
		}

		[Test]
		public void ShouldNotBeEditablWhenNoWorkflowControlSet()
		{
			data.WorkflowControlSet = null;

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel<PreferenceDayViewModel>(data.SelectedDate).Editable.Should().Be.False();
		}

		[Test]
		public void ShouldNotBeEditableWhenOutsidePreferenceInputPeriod()
		{
			var closedDate = DateOnly.Today.AddDays(1);
			data.WorkflowControlSet.PreferenceInputPeriod = new DateOnlyPeriod(closedDate, closedDate);
			data.WorkflowControlSet.PreferencePeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel<PreferenceDayViewModel>(data.SelectedDate).Editable.Should().Be.False();
		}

		[Test]
		public void ShouldNotBeEditableWhenOutsidePreferencePeriod()
		{
			var closedDate = DateOnly.Today.AddDays(1);
			data.WorkflowControlSet.PreferenceInputPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			data.WorkflowControlSet.PreferencePeriod = new DateOnlyPeriod(closedDate, closedDate);

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel<PreferenceDayViewModel>(data.SelectedDate).Editable.Should().Be.False();
		}

		[Test]
		public void ShouldMapShiftCategory()
		{
			var shiftCategory = new ShiftCategory("PM");
			var dayData = new PreferenceViewModelMappingProfile.PreferenceDayMappingData
			              	{
			              		Date = data.SelectedDate,
			              		Period = data.Period,
			              		ShiftCategory = shiftCategory
			              	};

			var result = Mapper.Map<PreferenceViewModelMappingProfile.PreferenceDayMappingData, PreferenceDayViewModel>(dayData);

			result.Preference.Should().Be(shiftCategory.Description.Name);
		}

		[Test]
		public void ShouldMapShiftCategoryFromFullDomainData()
		{
			var shiftCategory = new ShiftCategory("PM");
			var preferenceRestriction = new PreferenceRestriction { ShiftCategory = shiftCategory };
			var preferenceDay = new PreferenceDay(null, data.SelectedDate, preferenceRestriction);
			data.Days = new[] { new PreferenceDayDomainData {Date = data.SelectedDate, PreferenceDay = preferenceDay} };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel<PreferenceDayViewModel>(data.SelectedDate).Preference.Should().Be(shiftCategory.Description.Name);
		}

		[Test]
		public void ShouldMapEmptyShiftCategoryViewModelWhenNoShiftCategory()
		{
			var dayData = new PreferenceViewModelMappingProfile.PreferenceDayMappingData
			              	{
			              		Date = data.SelectedDate,
			              		Period = data.Period,
			              	};

			var result = Mapper.Map<PreferenceViewModelMappingProfile.PreferenceDayMappingData, PreferenceDayViewModel>(dayData);

			result.Preference.Should().Be.Null();
		}

		[Test]
		public void ShouldMapDayOff()
		{
			var dayOffTemplate = new DayOffTemplate(new Description("Day off", "DO"));
			var dayData = new PreferenceViewModelMappingProfile.PreferenceDayMappingData
			              	{
			              		Date = data.SelectedDate,
			              		Period = data.Period,
			              		DayOffTemplate = dayOffTemplate
			              	};

			var result = Mapper.Map<PreferenceViewModelMappingProfile.PreferenceDayMappingData, PreferenceDayViewModel>(dayData);

			result.Preference.Should().Be(dayOffTemplate.Description.Name);
		}


		[Test]
		public void ShouldMapDayOffTemplateFromFullDomainData()
		{
			var dayOffTemplate = new DayOffTemplate(new Description("Day off", "DO"));
			var preferenceRestriction = new PreferenceRestriction { DayOffTemplate = dayOffTemplate };
			var preferenceDay = new PreferenceDay(null, data.SelectedDate, preferenceRestriction);
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, PreferenceDay = preferenceDay } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel<PreferenceDayViewModel>(data.SelectedDate).Preference.Should().Be(dayOffTemplate.Description.Name);
		}

		[Test]
		public void ShouldMapAbsence()
		{
			var absence = new Absence { Description = new Description("Ill") };
			var dayData = new PreferenceViewModelMappingProfile.PreferenceDayMappingData
			              	{
			              		Date = data.SelectedDate,
			              		Period = data.Period,
			              		Absence = absence
			              	};

			var result = Mapper.Map<PreferenceViewModelMappingProfile.PreferenceDayMappingData, PreferenceDayViewModel>(dayData);

			result.Preference.Should().Be(absence.Description.Name);
		}

		[Test]
		public void ShouldMapAbsenceFromFullDomainData()
		{
			var absence = new Absence { Description = new Description("Ill") };
			var preferenceRestriction = new PreferenceRestriction { Absence = absence };
			var preferenceDay = new PreferenceDay(null, data.SelectedDate, preferenceRestriction);
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, PreferenceDay = preferenceDay } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel<PreferenceDayViewModel>(data.SelectedDate).Preference.Should().Be(absence.Description.Name);
		}

		[Test]
		public void ShouldMapScheduledContractTime()
		{
			var contractTime = TimeSpan.FromHours(8);
			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.ContractTime()).Return(contractTime);

			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, Projection = projection } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel<ScheduledDayViewModel>(data.SelectedDate).ContractTime.Should().Be(TimeHelper.GetLongHourMinuteTimeString(contractTime, CultureInfo.CurrentUICulture));
		}

		[Test]
		public void ShouldMapScheduledShiftCategory()
		{
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "));
			personAssignment.SetMainShift(new MainShift(new ShiftCategory("shiftCategory")));
			var scheduleDay = new StubFactory().ScheduleDayStub(data.SelectedDate, SchedulePartView.MainShift, personAssignment);

			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, ScheduleDay = scheduleDay} };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel<ScheduledDayViewModel>(data.SelectedDate).ShiftCategory.Should().Be(personAssignment.MainShift.ShiftCategory.Description.Name);
		}

		[Test]
		public void ShouldMapScheduledTimeSpan()
		{
			data.SelectedDate = new DateOnly(2012, 2, 21);
			data.Period = new DateOnlyPeriod(data.SelectedDate, data.SelectedDate);
			var stubs = new StubFactory();
			var personAssignment = stubs.PersonAssignmentStub(new DateTimePeriod(new DateTime(2012, 2, 21, 7, 0, 0, DateTimeKind.Utc),
			                                                                     new DateTime(2012, 2, 21, 16, 0, 0, DateTimeKind.Utc)));
			var scheduleDay = stubs.ScheduleDayStub(data.SelectedDate, SchedulePartView.MainShift, personAssignment);

			data.Days = new[] {new PreferenceDayDomainData {Date = data.SelectedDate, ScheduleDay = scheduleDay}};

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel<ScheduledDayViewModel>(data.SelectedDate).TimeSpan.Should().Be(new TimePeriod(8, 0, 17, 0).ToShortTimeString());
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
		public void ShouldMapPossibleStartTime()
		{
			var earliest = new TimeSpan(8, 0, 0);
			var latest = new TimeSpan(10, 0, 0);
			var workTimeMinMax = new WorkTimeMinMax {StartTimeLimitation = new StartTimeLimitation(earliest, latest)};
			data.Days = new[] {new PreferenceDayDomainData {Date = data.SelectedDate, WorkTimeMinMax = workTimeMinMax}};

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel<PreferenceDayViewModel>(data.SelectedDate).PossibleStartTimes
				.Should().Be.EqualTo(
					workTimeMinMax.StartTimeLimitation.StartTimeString + "-" + workTimeMinMax.StartTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapPossibleEndTime()
		{
			var earliest = new TimeSpan(16, 0, 0);
			var latest = new TimeSpan(19, 0, 0);
			var workTimeMinMax = new WorkTimeMinMax {EndTimeLimitation = new EndTimeLimitation(earliest, latest)};
			data.Days = new[] {new PreferenceDayDomainData {Date = data.SelectedDate, WorkTimeMinMax = workTimeMinMax}};

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel<PreferenceDayViewModel>(data.SelectedDate).PossibleEndTimes
				.Should().Be.EqualTo(
					workTimeMinMax.EndTimeLimitation.StartTimeString + "-" + workTimeMinMax.EndTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapPossibleContractTime()
		{
			var earliest = new TimeSpan(6, 0, 0);
			var latest = new TimeSpan(9, 0, 0);
			var workTimeMinMax = new WorkTimeMinMax { WorkTimeLimitation = new WorkTimeLimitation(earliest, latest) };
			data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, WorkTimeMinMax = workTimeMinMax } };

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.DayViewModel<PreferenceDayViewModel>(data.SelectedDate).PossibleContractTimes
				.Should().Be.EqualTo(
					workTimeMinMax.WorkTimeLimitation.StartTimeString + "-" + workTimeMinMax.WorkTimeLimitation.EndTimeString);
		}
	}

	public static class Extensions
	{
		public static T DayViewModel<T>(this PreferenceViewModel viewModel, DateOnly date)
		{
			return (from w in viewModel.Weeks
					from d in w.Days
					where d.Date == date
					select d)
				.Cast<T>()
				.Single();
		}

	}

}