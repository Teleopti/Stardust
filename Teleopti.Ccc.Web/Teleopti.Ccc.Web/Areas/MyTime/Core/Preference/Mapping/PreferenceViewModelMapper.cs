using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceViewModelMapper
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPreferenceOptionsProvider _preferenceOptionsProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IVirtualSchedulePeriodProvider _virtualSchedulePeriodProvider;
		private readonly INow _now;

		public PreferenceViewModelMapper(IPermissionProvider permissionProvider,
			IPreferenceOptionsProvider preferenceOptionsProvider,
			INow now, IVirtualSchedulePeriodProvider virtualSchedulePeriodProvider,
			ILoggedOnUser loggedOnUser)
		{
			_permissionProvider = permissionProvider;
			_preferenceOptionsProvider = preferenceOptionsProvider;
			_now = now;
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_loggedOnUser = loggedOnUser;
		}

		private class PreferenceWeekMappingData
		{
			public DateOnly FirstDayOfWeek { get; set; }
			public DateOnlyPeriod Period { get; set; }
			public IWorkflowControlSet WorkflowControlSet { get; set; }
			public IEnumerable<DateOnly> ScheduledDays { get; set; }
		}

		private class DayMappingData
		{
			public DateOnly Date { get; set; }
			public DateOnlyPeriod Period { get; set; }
			public IWorkflowControlSet WorkflowControlSet { get; set; }
			public bool IsScheduled { get; set; }
		}

		public PreferenceViewModel Map(PreferenceDomainData s)
		{
			return new PreferenceViewModel
			{
				PeriodSelection = new PeriodSelectionViewModel
				{
					Date = s.SelectedDate.ToFixedClientDateOnlyFormat(),
					StartDate = s.Period.StartDate.Date,
					EndDate = s.Period.EndDate.Date,
					Display = s.Period.DateString,
					SelectableDateRange =
						new PeriodDateRangeViewModel
						{
							MinDate = new DateOnly(CultureInfo.CurrentCulture.Calendar.MinSupportedDateTime).ToFixedClientDateOnlyFormat(),
							MaxDate = new DateOnly(CultureInfo.CurrentCulture.Calendar.MaxSupportedDateTime).ToFixedClientDateOnlyFormat()
},
					SelectedDateRange =
						new PeriodDateRangeViewModel
						{
							MaxDate = s.Period.EndDate.ToFixedClientDateOnlyFormat(),
							MinDate = s.Period.StartDate.ToFixedClientDateOnlyFormat()
						},
					PeriodNavigation = new PeriodNavigationViewModel
					{
						CanPickPeriod = true,
						HasNextPeriod = true,
						HasPrevPeriod = true,
						NextPeriod = s.Period.EndDate.AddDays(1).ToFixedClientDateOnlyFormat(),
						PrevPeriod = s.Period.StartDate.AddDays(-1).ToFixedClientDateOnlyFormat()
					}
				},
				WeekDayHeaders = DateHelper.GetWeekPeriod(s.SelectedDate,CultureInfo.CurrentCulture).DayCollection().Select(d => new WeekDayHeader
				{
					Date = d,
					Title = CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(d.Date.DayOfWeek)
				}),											
				Weeks = weeks(s),
				PreferencePeriod = map(s.WorkflowControlSet),
				ExtendedPreferencesPermission =
					_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb),
				Options = new PreferenceOptionsViewModel(PreferenceOptions(s.SelectedDate), ActivityOptions()),
				MaxMustHave = s.MaxMustHave,
				CurrentMustHave = s.CurrentMustHave
			};			
		}

		private PreferencePeriodViewModel map(IWorkflowControlSet s)
		{
			if (s == null) return null;
			return new PreferencePeriodViewModel
			{
				Period = s.PreferencePeriod.ToString(),
				OpenPeriod = s.PreferenceInputPeriod.ToString()
			};
		}

		private DayViewModel map(DayMappingData s)
		{
			return new DayViewModel
			{
				Date = s.Date,
				Header = new HeaderViewModel
					{
						DayDescription = dayDescription(s),
						DayNumber = s.Date.Day.ToString()
					},
				Editable = editable(s),
				InPeriod = s.Period.Contains(s.Date)
			};
		}

		private string dayDescription(DayMappingData s)
		{
			var firstDisplayDate =
				DateHelper.GetFirstDateInWeek(s.Period.StartDate, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
					.AddDays(-7);
			if (s.Date.Day == 1 || s.Date == firstDisplayDate)
				return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(s.Date.Month);
			return string.Empty;
		}

		private bool editable(DayMappingData s)
		{
			if (s.WorkflowControlSet == null)
				return false;

			var isInsideSchedulePeriod = s.Period.Contains(s.Date);
			var isInsidePreferencePeriod = s.WorkflowControlSet.PreferencePeriod.Contains(s.Date);
			var isInsidePreferenceInputPeriod = s.WorkflowControlSet.PreferenceInputPeriod.Contains(_now.ServerDate_DontUse());

			return isInsideSchedulePeriod && isInsidePreferencePeriod && isInsidePreferenceInputPeriod && !s.IsScheduled;
		}

		private WeekViewModel map(PreferenceWeekMappingData s)
		{
			var datesThisWeek = Enumerable.Range(0, 7).Select(d => s.FirstDayOfWeek.AddDays(d));
			return new WeekViewModel
			{
				Days =
					datesThisWeek.Select(
							d => new DayMappingData {
								Date = d,
								Period = s.Period,
								WorkflowControlSet = s.WorkflowControlSet,
								IsScheduled =s.ScheduledDays.Contains(d)})
								.Select(map)
								.ToArray()
			};
		}
		
		private PreferenceOptionGroup ActivityOptions()
		{
			return new PreferenceOptionGroup(Resources.Activity,
			(from a in _preferenceOptionsProvider.RetrieveActivityOptions().EmptyIfNull()
				select new PreferenceOption
				{
					Value = a.Id.ToString(),
					Text = a.Description.Name,
					Color = a.DisplayColor.ToHtml()
				}).ToList());
		}

		private IEnumerable<PreferenceOptionGroup> PreferenceOptions(DateOnly date)
		{
			var shiftCategoriesFromWorkflowControlSet =
				_preferenceOptionsProvider
					.RetrieveShiftCategoryOptions()
					.EmptyIfNull();

			// Get person period include current date
			var period = _virtualSchedulePeriodProvider.GetCurrentOrNextVirtualPeriodForDate(date);
			var personPeriods = _loggedOnUser.CurrentUser().PersonPeriods(period).ToList();
			var currentPeriod =
				personPeriods.SingleOrDefault(p => p.Period.Contains(date) && p.RuleSetBag != null);

			var shiftCategoriesFromBag = currentPeriod?.RuleSetBag?.ShiftCategoriesInBag() ?? new List<IShiftCategory>();

			var availableShiftCategory =
				shiftCategoriesFromWorkflowControlSet.Where(shift => shiftCategoriesFromBag.Any(x => x.Id == shift.Id));

			var shiftCategories = availableShiftCategory.Select(s => new PreferenceOption
				{
					Value = s.Id.ToString(),
					Text = s.Description.Name,
					Color = s.DisplayColor.ToHtml(),
					Extended = true
				})
				.OrderBy(pref => pref.Text)
				.ToArray();

			var dayOffs = _preferenceOptionsProvider
				.RetrieveDayOffOptions()
				.EmptyIfNull()
				.Select(s => new PreferenceOption
				{
					Value = s.Id.ToString(),
					Text = s.Description.Name,
					Color = s.DisplayColor.ToHtml(),
					Extended = false
				})
				.OrderBy(d => d.Text)
				.ToArray();

			var absences = _preferenceOptionsProvider
				.RetrieveAbsenceOptions()
				.EmptyIfNull()
				.Select(s => new PreferenceOption
				{
					Value = s.Id.ToString(),
					Text = s.Description.Name,
					Color = s.DisplayColor.ToHtml(),
					Extended = false
				})
				.OrderBy(abs => abs.Text)
				.ToArray();

			var optionGroups = new List<PreferenceOptionGroup>
			{
				new PreferenceOptionGroup(Resources.ShiftCategory, shiftCategories)
			};
			if (dayOffs.Any())
				optionGroups.Add(new PreferenceOptionGroup(Resources.DayOff, dayOffs));
			if (absences.Any())
				optionGroups.Add(new PreferenceOptionGroup(Resources.Absence, absences));

			return optionGroups.ToList();
		}

		private WeekViewModel[] weeks(PreferenceDomainData s)
		{
			var firstDatesOfWeeks = new List<DateOnly>();
			var firstDateOfWeek =
				DateHelper.GetFirstDateInWeek(s.Period.StartDate, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
					.AddDays(-7);
			var lastDisplayedDate =
				DateHelper.GetFirstDateInWeek(s.Period.EndDate, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
					.AddDays(6)
					.AddDays(7);
			while (firstDateOfWeek < lastDisplayedDate)
			{
				firstDatesOfWeeks.Add(firstDateOfWeek);
				firstDateOfWeek = firstDateOfWeek.AddDays(7);
			}

			return firstDatesOfWeeks.Select(d => new PreferenceWeekMappingData
			{
				FirstDayOfWeek = d,
				Period = s.Period,
				WorkflowControlSet = s.WorkflowControlSet,
				ScheduledDays = s.ScheduledDays
			}).Select(map).ToArray();
		}
	}
}