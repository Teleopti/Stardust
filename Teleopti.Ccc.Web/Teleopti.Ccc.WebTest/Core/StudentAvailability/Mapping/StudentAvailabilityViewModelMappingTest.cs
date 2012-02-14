using System;
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

namespace Teleopti.Ccc.WebTest.Core.StudentAvailability.Mapping
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
		private Person person;

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
			var scheduleDays = new[] {scheduleDay};
			scheduleProvider.Stub(x => x.GetScheduleForPeriod(displayedPeriod)).Return(scheduleDays);
			var studentAvailabilityProvider = MockRepository.GenerateMock<IStudentAvailabilityProvider>();
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
			var result = Mapper.Map<DateOnly, PeriodSelectionViewModel>(date);

			result.Date.Should().Be.EqualTo(date.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPeriodSelectionDisplay()
		{
			var result = Mapper.Map<DateOnly, PeriodSelectionViewModel>(date);

			result.Display.Should().Be.EqualTo(period.DateString);
		}

		[Test]
		public void ShouldFillPeriodSelectionNavigation()
		{
			var result = Mapper.Map<DateOnly, PeriodSelectionViewModel>(date);

			result.Navigation.CanPickPeriod.Should().Be.True();
			result.Navigation.HasNextPeriod.Should().Be.True();
			result.Navigation.HasPrevPeriod.Should().Be.True();
			result.Navigation.FirstDateNextPeriod.Should().Be.EqualTo(period.EndDate.AddDays(1).ToFixedClientDateOnlyFormat());
			result.Navigation.LastDatePreviousPeriod.Should().Be.EqualTo(period.StartDate.AddDays(-1).ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldFillPeriodSelectaleDateRange()
		{
			var result = Mapper.Map<DateOnly, PeriodSelectionViewModel>(date);

			result.SelectableDateRange.MinDate.Should().Be.EqualTo(new DateOnly(CultureInfo.CurrentCulture.Calendar.MinSupportedDateTime).ToFixedClientDateOnlyFormat());
			result.SelectableDateRange.MaxDate.Should().Be.EqualTo(new DateOnly(CultureInfo.CurrentCulture.Calendar.MaxSupportedDateTime).ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPeriodSelectedDateRange()
		{
			var result = Mapper.Map<DateOnly, PeriodSelectionViewModel>(date);

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
			var result = Mapper.Map<DateOnly, DayViewModelBase>(date);

			result.Date.Should().Be(date);
		}

		[Test]
		public void ShouldFillDayViewHeaderDayNumber()
		{
			var result = Mapper.Map<DateOnly, DayViewModelBase>(date);

			result.Header.DayNumber.Should().Be(date.Day.ToString());
		}

		[Test]
		public void ShouldFillDayViewHeaderWithMonthNameWhenFirstDayOfMonth()
		{
			var firstDateInMonth = new DateOnly(date.Year, date.Month, 1);

			var result = Mapper.Map<DateOnly, DayViewModelBase>(firstDateInMonth);

			result.Header.DayDescription.Should().Be.EqualTo(
				CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month));
		}

		[Test]
		public void ShouldNotFillDayViewHeaderWithMonthWhenNotFirstDayOfMonth()
		{
			var noDayDescriptionDate = new DateOnly(date.Year, date.Month, 2);
			if (noDayDescriptionDate == firstDisplayedDate) // because first displayed date should have month name no matter what
				noDayDescriptionDate = new DateOnly(date.Year, date.Month, 3);

			var result = Mapper.Map<DateOnly, DayViewModelBase>(noDayDescriptionDate);

			result.Header.DayDescription.Should().Be.Empty();
		}

		[Test]
		public void ShouldFillDayViewHeaderWithMonthNameForFirstDayOfDisplayedPeriod()
		{
			var firstDisplayedDateOnly = new DateOnly(firstDisplayedDate);

			var result = Mapper.Map<DateOnly, DayViewModelBase>(firstDisplayedDateOnly);

			result.Header.DayDescription.Should().Be.EqualTo(
				CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(firstDisplayedDateOnly.Month));
		}

		[Test]
		public void ShouldFillStateEditable()
		{
			var result = Mapper.Map<DateOnly, DayViewModelBase>(date);

			var actual = DayState.Editable & result.State;
			actual.Should().Be(DayState.Editable);
		}

		[Test]
		public void ShouldFillStateDeletableWhenStudentAvailabilityDayExists()
		{
			var result = Mapper.Map<DateOnly, DayViewModelBase>(date);

			var actual = DayState.Deletable & result.State;
			actual.Should().Be(DayState.Deletable);
		}

		[Test]
		public void ShouldNotFillStateDeletableWhenNoStudentAvailabilityDay()
		{
			var dateWithoutStudentAvailability = date.AddDays(1);

			var result = Mapper.Map<DateOnly, DayViewModelBase>(dateWithoutStudentAvailability);

			var actual = DayState.Deletable & result.State;
			actual.Should().Be(DayState.None);
		}

		[Test]
		public void ShouldNotFillEditableOrDeletableStateWhenOutsideSchedulePeriod()
		{
			var outsideDate = period.EndDate.AddDays(1);

			var result = Mapper.Map<DateOnly, DayViewModelBase>(outsideDate);

			result.State.Should().Be(DayState.None);
		}

		[Test]
		public void ShouldNotFillEditableOrDeletableWhenNoWorkflowControlSet()
		{
			person.WorkflowControlSet = null;

			var result = Mapper.Map<DateOnly, DayViewModelBase>(date);

			result.State.Should().Be(DayState.None);
		}

		[Test]
		public void ShouldNotFillEditableOrDeletableWhenOutsideStudentAvailabilityInputPeriod()
		{
			var closedInputPeriodDate = DateOnly.Today.AddDays(1);
			person.WorkflowControlSet.StudentAvailabilityInputPeriod = new DateOnlyPeriod(closedInputPeriodDate, closedInputPeriodDate);
			person.WorkflowControlSet.StudentAvailabilityPeriod = new DateOnlyPeriod(date, date);

			var result = Mapper.Map<DateOnly, DayViewModelBase>(date);

			result.State.Should().Be(DayState.None);
		}

		[Test]
		public void ShouldNotFillEditableOrDeletableWhenOutsideStudentAvailabilityPeriod()
		{
			var closedPeriodDate = DateOnly.Today.AddDays(1);
			person.WorkflowControlSet.StudentAvailabilityInputPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			person.WorkflowControlSet.StudentAvailabilityPeriod = new DateOnlyPeriod(closedPeriodDate, closedPeriodDate);

			var result = Mapper.Map<DateOnly, DayViewModelBase>(date);

			result.State.Should().Be(DayState.None);
		}

		[Test]
		public void ShouldMapAvailableDayAvailableTimeSpan()
		{
			var result = Mapper.Map<DateOnly, AvailableDayViewModel>(date);

			result.AvailableTimeSpan.Should().Be(studentAvailabilityRestriction.StartTimeLimitation.StartTimeString + " - " + studentAvailabilityRestriction.EndTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapEmptyAvailableDayViewModelWhenNoStudentAvailability()
		{
			var dateWithoutStudentAvailability = date.AddDays(1);

			var result = Mapper.Map<DateOnly, AvailableDayViewModel>(dateWithoutStudentAvailability);

			result.AvailableTimeSpan.Should().Be.Empty();
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

		[Test, Ignore]
		public void ShouldMapStyleClasses() { Assert.Fail("sets empty array today just to make it work"); }

		[Test, Ignore]
		public void ShouldMapPeriodSummaryViewModel() { Assert.Fail("sets an empty instance today to make it work"); }

		[Test, Ignore]
		public void ShouldMapWeekSummary() { Assert.Fail("sets an empty instance today to make it work"); }
	}
}