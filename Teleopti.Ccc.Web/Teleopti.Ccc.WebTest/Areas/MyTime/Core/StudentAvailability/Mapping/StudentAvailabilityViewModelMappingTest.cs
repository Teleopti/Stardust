using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;


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
		private StudentAvailabilityViewModelMapper Mapper;

		[SetUp]
		public void Setup()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("sv-SE");
			Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("sv-SE");

			date = new DateOnly(2017, 03, 05); // require the period to cover first-of-month
			period = new DateOnlyPeriod(date.AddDays(-7), date.AddDays(7).AddDays(-1));
			firstDisplayedDate = DateHelper.GetFirstDateInWeek(period.StartDate, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek).AddDays(-7);
			lastDisplayedDate = DateHelper.GetFirstDateInWeek(period.EndDate, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek).AddDays(6).AddDays(7);
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
			             					StudentAvailabilityInputPeriod = DateOnly.Today.ToDateOnlyPeriod()
			             				}
			             	};
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			Mapper = new StudentAvailabilityViewModelMapper(scheduleProvider, studentAvailabilityProvider, virtualSchedulePeriodProvider, loggedOnUser, new Now());
		}
		
		[Test]
		public void ShouldMapPeriodSelectionDate()
		{
			var result = Mapper.Map(date);

			result.PeriodSelection.Date.Should().Be.EqualTo(date.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPeriodSelectionDisplay()
		{
			var result = Mapper.Map(date);

			result.PeriodSelection.Display.Should().Be.EqualTo(period.DateString);
		}

		[Test]
		public void ShouldFillPeriodSelectionNavigation()
		{
			var result = Mapper.Map(date);

			result.PeriodSelection.PeriodNavigation.CanPickPeriod.Should().Be.True();
			result.PeriodSelection.PeriodNavigation.HasNextPeriod.Should().Be.True();
			result.PeriodSelection.PeriodNavigation.HasPrevPeriod.Should().Be.True();
			result.PeriodSelection.PeriodNavigation.NextPeriod.Should().Be.EqualTo(period.EndDate.AddDays(1).ToFixedClientDateOnlyFormat());
			result.PeriodSelection.PeriodNavigation.PrevPeriod.Should().Be.EqualTo(period.StartDate.AddDays(-1).ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldFillPeriodSelectableDateRange()
		{
			var result = Mapper.Map(date);

			result.PeriodSelection.SelectableDateRange.MinDate.Should().Be.EqualTo(DateOnly.MinValue.ToFixedClientDateOnlyFormat());
			result.PeriodSelection.SelectableDateRange.MaxDate.Should().Be.EqualTo(DateOnly.MaxValue.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPeriodSelectedDateRange()
		{
			var result = Mapper.Map(date);

			result.PeriodSelection.SelectedDateRange.MinDate.Should().Be.EqualTo(period.StartDate.ToFixedClientDateOnlyFormat());
			result.PeriodSelection.SelectedDateRange.MaxDate.Should().Be.EqualTo(period.EndDate.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldFillWeekDayHeaders()
		{
			var result = Mapper.Map(date);

			result.WeekDayHeaders.Should().Have.Count.EqualTo(7);
			result.WeekDayHeaders.Select(x => x.Title).Should().Have.SameSequenceAs(DateHelper.GetWeekdayNames(CultureInfo.CurrentCulture));
		}

		[Test]
		public void ShouldFillWeekViewModelsWithFullWeeksPlusMinusOneWeek()
		{
			var result = Mapper.Map(date);

			// get number of weeks for period. Why cant working with weeks ever be easy...
			var firstDateNotShown = lastDisplayedDate.AddDays(1);
			var shownTime = firstDateNotShown.Subtract(firstDisplayedDate);
			var shownDays = shownTime.Days;
			var shownWeeks = shownDays/7;

			result.Weeks.Should().Have.Count.EqualTo(shownWeeks);
		}

		[Test]
		public void ShouldFillDayViewModels()
		{
			var result = Mapper.Map(date);

			result.Weeks.ForEach(
				week =>
					{
						week.Days.Should().Have.Count.EqualTo(7);
						week.Days.ElementAt(0).Date.DayOfWeek.Should().Be(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
					}
				);
		}
		
		[Test]
		public void ShouldFillDayViewHeaderDayNumber()
		{
			var result = Mapper.Map(date);

			result.Weeks.SelectMany(x => x.Days).First(d => d.Date == date).Header.DayNumber.Should().Be(date.Day.ToString());
		}

		[Test]
		public void ShouldFillDayViewHeaderWithMonthNameWhenFirstDayOfMonth()
		{
			var firstDateInMonth = new DateOnly(date.Year, date.Month, 1);
			var result = Mapper.Map(date);

			result.Weeks.SelectMany(x => x.Days).First(d => d.Date == firstDateInMonth).Header.DayDescription.Should().Be.EqualTo(
				CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month));
		}

		[Test]
		public void ShouldNotFillDayViewHeaderWithMonthWhenNotFirstDayOfMonth()
		{
			var noDayDescriptionDate = new DateOnly(date.Year, date.Month, 2);
			if (noDayDescriptionDate == firstDisplayedDate) // because first displayed date should have month name no matter what
				noDayDescriptionDate = new DateOnly(date.Year, date.Month, 3);

			var result = Mapper.Map(date);

			result.Weeks.SelectMany(x => x.Days).First(d => d.Date == noDayDescriptionDate).Header.DayDescription.Should().Be.Empty();
		}

		[Test]
		public void ShouldFillDayViewHeaderWithMonthNameForFirstDayOfDisplayedPeriod()
		{
			var firstDisplayedDateOnly = firstDisplayedDate;
			var result = Mapper.Map(date);

			result.Weeks.SelectMany(x => x.Days).First(d => d.Date == firstDisplayedDateOnly).Header.DayDescription.Should().Be.EqualTo(
				CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(firstDisplayedDateOnly.Month));
		}

		[Test]
		public void ShouldMapTrueInPeriod()
		{
			var result = Mapper.Map(date);

			result.Weeks.SelectMany(x => x.Days).First(d => d.Date == date).InPeriod.Should().Be.True();
		}

		[Test]
		public void ShouldMapFalseInPeriod()
		{
			var result = Mapper.Map(date);

			result.Weeks.SelectMany(x => x.Days).First(d => d.Date == date.AddDays(8)).InPeriod.Should().Be.False();
		}

		[Test]
		public void ShouldFillStateEditable()
		{
			var result = Mapper.Map(date);

			result.Weeks.SelectMany(x => x.Days).First(d => d.Date == date).Editable.Should().Be.True();
		}

		[Test]
		public void ShouldNotFillStateDeletableWhenNoStudentAvailabilityDay()
		{
			var dateWithoutStudentAvailability = date.AddDays(2);

			var result = Mapper.Map(dateWithoutStudentAvailability);

			result.Weeks.SelectMany(x => x.Days).First(d => d.Date == dateWithoutStudentAvailability).Editable.Should().Be.False();
		}

		[Test]
		public void ShouldNotFillEditableOrDeletableStateWhenOutsideSchedulePeriod()
		{
			var outsideDate = period.EndDate.AddDays(1);

			var result = Mapper.Map(date);

			result.Weeks.SelectMany(x => x.Days).First(d => d.Date == outsideDate).Editable.Should().Be.False();
		}

		[Test]
		public void ShouldNotFillEditableOrDeletableWhenNoWorkflowControlSet()
		{
			person.WorkflowControlSet = null;

			var result = Mapper.Map(date);

			result.Weeks.SelectMany(x => x.Days).First(d => d.Date == date).Editable.Should().Be.False();
		}

		[Test]
		public void ShouldNotFillEditableOrDeletableWhenOutsideStudentAvailabilityInputPeriod()
		{
			var closedInputPeriodDate = DateOnly.Today.AddDays(1);
			person.WorkflowControlSet.StudentAvailabilityInputPeriod = new DateOnlyPeriod(closedInputPeriodDate, closedInputPeriodDate);
			person.WorkflowControlSet.StudentAvailabilityPeriod = new DateOnlyPeriod(date, date);

			var result = Mapper.Map(date);

			result.Weeks.SelectMany(x => x.Days).First(d => d.Date == date).Editable.Should().Be.False();
		}

		[Test]
		public void ShouldNotFillEditableOrDeletableWhenOutsideStudentAvailabilityPeriod()
		{
			var closedPeriodDate = DateOnly.Today.AddDays(1);
			person.WorkflowControlSet.StudentAvailabilityInputPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			person.WorkflowControlSet.StudentAvailabilityPeriod = new DateOnlyPeriod(closedPeriodDate, closedPeriodDate);

			var result = Mapper.Map(date);

			result.Weeks.SelectMany(x => x.Days).First(d => d.Date == date).Editable.Should().Be.False();
		}

		[Test]
		public void ShouldMapStudentAvailabilityPeriod()
		{
			var result = Mapper.Map(date);

			result.StudentAvailabilityPeriod.Period.Should().Be.EqualTo(person.WorkflowControlSet.StudentAvailabilityPeriod.DateString);
		}

		[Test]
		public void ShouldMapStudentAvailabilityInputPeriod()
		{
			var result = Mapper.Map(date);

			result.StudentAvailabilityPeriod.OpenPeriod.Should().Be.EqualTo(person.WorkflowControlSet.StudentAvailabilityInputPeriod.DateString);
		}

		[Test]
		public void ShouldMapNullStudentAvailabilityPeriodWhenNoWorkflowControlSet()
		{
			person.WorkflowControlSet = null;

			var result = Mapper.Map(date);

			result.StudentAvailabilityPeriod.Should().Be.Null();
		}
	}
}