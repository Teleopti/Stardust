using System;
using System.Globalization;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceViewModelMappingTest
	{
		private PreferenceDomainData data;
		private IPreferenceOptionsProvider preferenceOptionProvider;
		private IToggleManager toggleManager;

		[SetUp]
		public void Setup()
		{
			data = new PreferenceDomainData
					{
						SelectedDate = DateOnly.Today,
						Period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1)),
						Days = new[] { new PreferenceDayDomainData { Date = DateOnly.Today } },
						WorkflowControlSet =
							new WorkflowControlSet(null)
								{
									PreferencePeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1)),
									PreferenceInputPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today)
								},
						MaxMustHave = 8
					};
			preferenceOptionProvider = MockRepository.GenerateMock<IPreferenceOptionsProvider>();
			toggleManager = MockRepository.GenerateMock<IToggleManager>();

			Mapper.Reset();
			Mapper.Initialize(c =>
			                  	{
			                  		c.AddProfile(new PreferenceViewModelMappingProfile(new FakePermissionProvider(), () => preferenceOptionProvider, toggleManager));
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

			result.PeriodSelection.PeriodNavigation.CanPickPeriod.Should().Be.True();
			result.PeriodSelection.PeriodNavigation.HasNextPeriod.Should().Be.True();
			result.PeriodSelection.PeriodNavigation.HasPrevPeriod.Should().Be.True();
			result.PeriodSelection.PeriodNavigation.NextPeriod.Should().Be.EqualTo(data.Period.EndDate.AddDays(1).ToFixedClientDateOnlyFormat());
			result.PeriodSelection.PeriodNavigation.PrevPeriod.Should().Be.EqualTo(data.Period.StartDate.AddDays(-1).ToFixedClientDateOnlyFormat());
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
		public void ShouldMapMaxMustHave()
		{
			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			result.MaxMustHave.Should().Be.EqualTo(8);
		}

		[Test]
		public void ShouldMapPreferenceActivityOptions()
		{
			var activity =  new Activity("myActivity");
			activity.SetId(Guid.NewGuid());
			
			preferenceOptionProvider.Stub(x => x.RetrieveActivityOptions()).Return(new[] { activity });

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			assertOption(result.Options.ActivityOptions, Resources.Activity, activity.Description.Name, activity.Id.Value);
		}

		[Test]
		public void ShouldMapPreferenceOptions()
		{
			var shiftCategory = new ShiftCategory("SC");
			shiftCategory.SetId(Guid.NewGuid());
			var dayOff = new DayOffTemplate(new Description("DO"));
			dayOff.SetId(Guid.NewGuid());
			var absence = new Absence { Description = new Description("absence") };
			absence.SetId(Guid.NewGuid());

			preferenceOptionProvider.Stub(x => x.RetrieveShiftCategoryOptions()).Return(new[] { shiftCategory });
			preferenceOptionProvider.Stub(x => x.RetrieveDayOffOptions()).Return(new[] { dayOff });
			preferenceOptionProvider.Stub(x => x.RetrieveAbsenceOptions()).Return(new[] { absence });

			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			var shiftCategoriesGroup = result.Options.PreferenceOptions.First();
			var dayOffsGroup = result.Options.PreferenceOptions.ElementAt(1);
			var absencesGroup = result.Options.PreferenceOptions.Last();

			assertOption(shiftCategoriesGroup, Resources.ShiftCategory, shiftCategory.Description.Name, shiftCategory.Id.Value);
			assertOption(dayOffsGroup, Resources.DayOff, dayOff.Description.Name, dayOff.Id.Value);
			assertOption(absencesGroup, Resources.Absence, absence.Description.Name, absence.Id.Value);
		}

		[Test]
		public void ShouldMapIsWeeklyWorkTimeEnabled()
		{
			toggleManager.Stub(x => x.IsEnabled(Toggles.Preference_PreferenceAlertWhenMinOrMaxHoursBroken_25635)).Return(true);
			var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

			Assert.That(result.IsWeeklyWorkTimeEnabled, Is.True);
		}

		private void assertOption(PreferenceOptionGroup optionGroup, string groupText, string firstItemName, Guid firstItemNameId)
		{
			optionGroup.Text.Should().Be.EqualTo(groupText);
			optionGroup.Options.Count().Should().Be.EqualTo(1);
			optionGroup.Options.First().Text.Should().Be.EqualTo(firstItemName);
			optionGroup.Options.First().Value.Should().Be.EqualTo(firstItemNameId.ToString());
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