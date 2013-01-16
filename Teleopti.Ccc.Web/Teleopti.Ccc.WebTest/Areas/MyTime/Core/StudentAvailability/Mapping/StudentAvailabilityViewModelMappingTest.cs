﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.StudentAvailability.Mapping
{
	[TestFixture]
	public class StudentAvailabilityViewModelMappingTest
	{
		private DateOnly date;
		private DateOnlyPeriod period;
		private DateOnly firstDisplayedDate;
		private DateOnly lastDisplayedDate;
		private DateOnlyPeriod displayedPeriod;
		private StudentAvailabilityRestriction studentAvailabilityRestriction;
		private IStudentAvailabilityProvider studentAvailabilityProvider;
		private Person person;
		private IEnumerable<IScheduleDay> scheduleDays;

		[SetUp]
		public void Setup()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("sv-SE");
			Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("sv-SE");

			date = DateOnly.Today;
			period = new DateOnlyPeriod(date.AddDays(-7), date.AddDays(7).AddDays(-1));
			firstDisplayedDate = new DateOnly(DateHelper.GetFirstDateInWeek(period.StartDate, CultureInfo.CurrentCulture).AddDays(-7));
			lastDisplayedDate = new DateOnly(DateHelper.GetLastDateInWeek(period.EndDate, CultureInfo.CurrentCulture).AddDays(7));
			displayedPeriod = new DateOnlyPeriod(firstDisplayedDate, lastDisplayedDate);

			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			virtualSchedulePeriodProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(date)).Return(period).IgnoreArguments();

			studentAvailabilityRestriction = new StudentAvailabilityRestriction
			                                 	{
			                                 		StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(7), null),
			                                 		EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(18))
			                                 	};

			var scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			scheduleDays = new[] {scheduleDay};
			scheduleProvider.Stub(x => x.GetScheduleForPeriod(displayedPeriod)).Return(scheduleDays);
			studentAvailabilityProvider = MockRepository.GenerateMock<IStudentAvailabilityProvider>();
			studentAvailabilityProvider.Stub(x => x.GetStudentAvailabilityForDate(scheduleDays, date)).Return(studentAvailabilityRestriction);

			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			person = new Person
			             	{
			             		WorkflowControlSet =
			             			new WorkflowControlSet(null)
			             				{
			             					StudentAvailabilityPeriod = new DateOnlyPeriod(date, date.AddDays(1)),
			             					StudentAvailabilityInputPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today)
			             				}
			             	};
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new StudentAvailabilityViewModelMappingProfile(() => Mapper.Engine, () => scheduleProvider, () => studentAvailabilityProvider, () => virtualSchedulePeriodProvider, () => loggedOnUser)));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapPeriodSelectionDate()
		{
			var sa = Mapper.Map<DateOnly, StudentAvailabilityDomainData>(date);
			var result = Mapper.Map<StudentAvailabilityDomainData, PeriodSelectionViewModel>(sa);

			result.Date.Should().Be.EqualTo(date.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPeriodSelectionDisplay()
		{
			var sa = Mapper.Map<DateOnly, StudentAvailabilityDomainData>(date);
			var result = Mapper.Map<StudentAvailabilityDomainData, PeriodSelectionViewModel>(sa);

			result.Display.Should().Be.EqualTo(period.DateString);
		}

		[Test]
		public void ShouldFillPeriodSelectionNavigation()
		{
			var sa = Mapper.Map<DateOnly, StudentAvailabilityDomainData>(date);
			var result = Mapper.Map<StudentAvailabilityDomainData, PeriodSelectionViewModel>(sa);

			result.PeriodNavigation.CanPickPeriod.Should().Be.True();
			result.PeriodNavigation.HasNextPeriod.Should().Be.True();
			result.PeriodNavigation.HasPrevPeriod.Should().Be.True();
			result.PeriodNavigation.NextPeriod.Should().Be.EqualTo(period.EndDate.AddDays(1).ToFixedClientDateOnlyFormat());
			result.PeriodNavigation.PrevPeriod.Should().Be.EqualTo(period.StartDate.AddDays(-1).ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldFillPeriodSelectaleDateRange()
		{
			var sa = Mapper.Map<DateOnly, StudentAvailabilityDomainData>(date);
			var result = Mapper.Map<StudentAvailabilityDomainData, PeriodSelectionViewModel>(sa);

			result.SelectableDateRange.MinDate.Should().Be.EqualTo(new DateOnly(CultureInfo.CurrentCulture.Calendar.MinSupportedDateTime).ToFixedClientDateOnlyFormat());
			result.SelectableDateRange.MaxDate.Should().Be.EqualTo(new DateOnly(CultureInfo.CurrentCulture.Calendar.MaxSupportedDateTime).ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPeriodSelectedDateRange()
		{
			var sa = Mapper.Map<DateOnly, StudentAvailabilityDomainData>(date);
			var result = Mapper.Map<StudentAvailabilityDomainData, PeriodSelectionViewModel>(sa);

			result.SelectedDateRange.MinDate.Should().Be.EqualTo(period.StartDate.ToFixedClientDateOnlyFormat());
			result.SelectedDateRange.MaxDate.Should().Be.EqualTo(period.EndDate.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldFillWeekDayHeaders()
		{
			var result = Mapper.Map<DateOnly, StudentAvailabilityViewModel>(date);

			result.WeekDayHeaders.Should().Have.Count.EqualTo(7);
			result.WeekDayHeaders.Select(x => x.Title).Should().Have.SameSequenceAs(DateHelper.GetWeekdayNames(CultureInfo.CurrentCulture));
		}

		[Test]
		public void ShouldFillWeekViewModelsWithFullWeeksPlusMinusOneWeek()
		{
			var result = Mapper.Map<DateOnly, StudentAvailabilityViewModel>(date);

			// get number of weeks for period. Why cant working with weeks ever be easy...
			var firstDateNotShown = lastDisplayedDate.AddDays(1);
			var shownTime = firstDateNotShown.Date.Subtract(firstDisplayedDate);
			var shownDays = shownTime.Days;
			var shownWeeks = shownDays/7;

			result.Weeks.Should().Have.Count.EqualTo(shownWeeks);
		}

		[Test]
		public void ShouldFillDayViewModels()
		{
			var result = Mapper.Map<DateOnly, StudentAvailabilityViewModel>(date);

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
			var result = Mapper.Map<StudentAvailabilityDayDomainData, DayViewModelBase>(new StudentAvailabilityDayDomainData(date, new DateOnlyPeriod(2000,1,1, 2000,1,2),null,null,null ));

			result.Date.Should().Be(date);
		}

		[Test]
		public void ShouldFillDayViewHeaderDayNumber()
		{
			var input = new StudentAvailabilityDayDomainData(date, new DateOnlyPeriod(2000,1,1, 2000,1,2), null, null, null);
			var result = Mapper.Map<StudentAvailabilityDayDomainData, DayViewModelBase>(input);

			result.Header.DayNumber.Should().Be(date.Day.ToString());
		}

		[Test]
		public void ShouldFillDayViewHeaderWithMonthNameWhenFirstDayOfMonth()
		{
			var firstDateInMonth = new DateOnly(date.Year, date.Month, 1);
			var input = new StudentAvailabilityDayDomainData(firstDateInMonth, new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2), null, null, null);
			var result = Mapper.Map<StudentAvailabilityDayDomainData, DayViewModelBase>(input);

			result.Header.DayDescription.Should().Be.EqualTo(
				CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month));
		}

		[Test]
		public void ShouldNotFillDayViewHeaderWithMonthWhenNotFirstDayOfMonth()
		{
			var noDayDescriptionDate = new DateOnly(date.Year, date.Month, 2);
			if (noDayDescriptionDate == firstDisplayedDate) // because first displayed date should have month name no matter what
				noDayDescriptionDate = new DateOnly(date.Year, date.Month, 3);

			var result = Mapper.Map<StudentAvailabilityDayDomainData, DayViewModelBase>(new StudentAvailabilityDayDomainData(noDayDescriptionDate, period,null,null,null ));

			result.Header.DayDescription.Should().Be.Empty();
		}

		[Test]
		public void ShouldFillDayViewHeaderWithMonthNameForFirstDayOfDisplayedPeriod()
		{
			var firstDisplayedDateOnly = new DateOnly(firstDisplayedDate);
			var input = new StudentAvailabilityDayDomainData(firstDisplayedDateOnly, period, null, studentAvailabilityProvider, null);
			var result = Mapper.Map<StudentAvailabilityDayDomainData, DayViewModelBase>(input);

			result.Header.DayDescription.Should().Be.EqualTo(
				CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(firstDisplayedDateOnly.Month));
		}

		[Test]
		public void ShouldMapTrueInPeriod()
		{
			var result = Mapper.Map<StudentAvailabilityDayDomainData, DayViewModelBase>(new StudentAvailabilityDayDomainData(date, period, null, studentAvailabilityProvider, null));

			result.InPeriod.Should().Be.True();
		}

		[Test]
		public void ShouldMapFalseInPeriod()
		{
			var result = Mapper.Map<StudentAvailabilityDayDomainData, DayViewModelBase>(new StudentAvailabilityDayDomainData(date.AddDays(8), period, null, studentAvailabilityProvider, null));

			result.InPeriod.Should().Be.False();
		}

		[Test]
		public void ShouldFillStateEditable()
		{
			var result = Mapper.Map<StudentAvailabilityDayDomainData, DayViewModelBase>(new StudentAvailabilityDayDomainData(date, period, person, studentAvailabilityProvider, scheduleDays));

			result.Editable.Should().Be.True();
		}

		[Test]
		public void ShouldNotFillStateDeletableWhenNoStudentAvailabilityDay()
		{
			var dateWithoutStudentAvailability = date.AddDays(1);

			var result = Mapper.Map<StudentAvailabilityDayDomainData, DayViewModelBase>(new StudentAvailabilityDayDomainData(dateWithoutStudentAvailability, new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2), null, studentAvailabilityProvider, null));

			result.Editable.Should().Be.False();
		}

		[Test]
		public void ShouldNotFillEditableOrDeletableStateWhenOutsideSchedulePeriod()
		{
			var outsideDate = period.EndDate.AddDays(1);

			var result = Mapper.Map<StudentAvailabilityDayDomainData, DayViewModelBase>(new StudentAvailabilityDayDomainData(outsideDate, period, null, null, null));

			result.Editable.Should().Be.False();
		}

		[Test]
		public void ShouldNotFillEditableOrDeletableWhenNoWorkflowControlSet()
		{
			person.WorkflowControlSet = null;

			var result = Mapper.Map<StudentAvailabilityDayDomainData, DayViewModelBase>(new StudentAvailabilityDayDomainData(date, period, person, null, null));

			result.Editable.Should().Be.False();
		}

		[Test]
		public void ShouldNotFillEditableOrDeletableWhenOutsideStudentAvailabilityInputPeriod()
		{
			var closedInputPeriodDate = DateOnly.Today.AddDays(1);
			person.WorkflowControlSet.StudentAvailabilityInputPeriod = new DateOnlyPeriod(closedInputPeriodDate, closedInputPeriodDate);
			person.WorkflowControlSet.StudentAvailabilityPeriod = new DateOnlyPeriod(date, date);

			var result = Mapper.Map<StudentAvailabilityDayDomainData, DayViewModelBase>(new StudentAvailabilityDayDomainData(date, period, person, null, null));

			result.Editable.Should().Be.False();
		}

		[Test]
		public void ShouldNotFillEditableOrDeletableWhenOutsideStudentAvailabilityPeriod()
		{
			var closedPeriodDate = DateOnly.Today.AddDays(1);
			person.WorkflowControlSet.StudentAvailabilityInputPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			person.WorkflowControlSet.StudentAvailabilityPeriod = new DateOnlyPeriod(closedPeriodDate, closedPeriodDate);

			var result = Mapper.Map<StudentAvailabilityDayDomainData, DayViewModelBase>(new StudentAvailabilityDayDomainData(date, period, person, null, null));

			result.Editable.Should().Be.False();
		}

		[Test]
		public void ShouldMapStudentAvailabilityPeriod()
		{
			var result = Mapper.Map<DateOnly, StudentAvailabilityViewModel>(date);

			result.StudentAvailabilityPeriod.Period.Should().Be.EqualTo(person.WorkflowControlSet.StudentAvailabilityPeriod.DateString);
		}

		[Test]
		public void ShouldMapStudentAvailabilityInputPeriod()
		{
			var result = Mapper.Map<DateOnly, StudentAvailabilityViewModel>(date);

			result.StudentAvailabilityPeriod.OpenPeriod.Should().Be.EqualTo(person.WorkflowControlSet.StudentAvailabilityInputPeriod.DateString);
		}

		[Test]
		public void ShouldMapNullStudentAvailabilityPeriodWhenNoWorkflowControlSet()
		{
			person.WorkflowControlSet = null;

			var result = Mapper.Map<DateOnly, StudentAvailabilityViewModel>(date);

			result.StudentAvailabilityPeriod.Should().Be.Null();
		}
	}
}