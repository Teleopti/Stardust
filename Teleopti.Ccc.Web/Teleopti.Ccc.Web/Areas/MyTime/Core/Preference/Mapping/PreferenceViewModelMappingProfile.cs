using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceViewModelMappingProfile : Profile
	{
		private readonly Func<IMappingEngine> _mappingEngine;

		public PreferenceViewModelMappingProfile(Func<IMappingEngine> mappingEngine)
		{
			_mappingEngine = mappingEngine;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<PreferenceDomainData, PreferenceViewModel>()
				.ForMember(d => d.PeriodSelection, o => o.MapFrom(s => s))
				.ForMember(d => d.WeekDayHeaders, o => o.MapFrom(s => DateHelper.GetWeekdayNames(CultureInfo.CurrentCulture)))
				.ForMember(d => d.Weeks, o => o.MapFrom(s =>
																		{
																			var firstDatesOfWeeks = new List<DateOnly>();
																			var firstDateOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(s.Period.StartDate, CultureInfo.CurrentCulture).AddDays(-7));
																			var lastDisplayedDate = new DateOnly(DateHelper.GetLastDateInWeek(s.Period.EndDate, CultureInfo.CurrentCulture).AddDays(7));
																			while (firstDateOfWeek < lastDisplayedDate)
																			{
																				firstDatesOfWeeks.Add(new DateOnly(firstDateOfWeek));
																				firstDateOfWeek = firstDateOfWeek.AddDays(7);
																			}

																			return (from d in firstDatesOfWeeks select new PreferenceWeekDomainData { FirstDayOfWeek = d, Period = s.Period, PreferenceDays = s.PreferenceDays, WorkflowControlSet = s.WorkflowControlSet, WorkTimeMinMax = s.WorkTimeMinMax }).ToArray();
																		}))
				.ForMember(d => d.PreferencePeriod, c => c.MapFrom(s => s.WorkflowControlSet))
				;

			CreateMap<string, WeekDayHeader>()
				.ForMember(d => d.Title, o => o.MapFrom(s => s))
				;

			CreateMap<PreferenceDomainData, PeriodSelectionViewModel>()
				.ForMember(d => d.Date, o => o.MapFrom(s => s.SelectedDate.ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.Display, o => o.MapFrom(s => s.Period.DateString))
				.ForMember(d => d.SelectableDateRange, o => o.MapFrom(s => new DateOnlyPeriod(new DateOnly(CultureInfo.CurrentCulture.Calendar.MinSupportedDateTime), new DateOnly(CultureInfo.CurrentCulture.Calendar.MaxSupportedDateTime))))
				.ForMember(d => d.SelectedDateRange, o => o.MapFrom(s => s.Period))
				.ForMember(d => d.Navigation, o => o.MapFrom(s => s))
				;

			CreateMap<PreferenceDomainData, PeriodNavigationViewModel>()
				.ForMember(d => d.CanPickPeriod, o => o.UseValue(true))
				.ForMember(d => d.HasNextPeriod, o => o.UseValue(true))
				.ForMember(d => d.HasPrevPeriod, o => o.UseValue(true))
				.ForMember(d => d.FirstDateNextPeriod, o => o.MapFrom(s => s.Period.EndDate.AddDays(1).ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.LastDatePreviousPeriod, o => o.MapFrom(s => s.Period.StartDate.AddDays(-1).ToFixedClientDateOnlyFormat()))
				;

			CreateMap<DateOnlyPeriod, PeriodDateRangeViewModel>()
				.ForMember(d => d.MaxDate, o => o.MapFrom(s => s.EndDate.ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.MinDate, o => o.MapFrom(s => s.StartDate.ToFixedClientDateOnlyFormat()))
				;

			CreateMap<PreferenceWeekDomainData, WeekViewModel>()
				.ForMember(d => d.Days, o => o.MapFrom(s =>
																		{
																			var preferenceDays = s.PreferenceDays ?? new IPreferenceDay[] { };
																			var workTimeMinMaxCollection = s.WorkTimeMinMax ?? new WorkTimeMinMaxDomainData[] {};
																			var days = from d in Enumerable.Range(0, 7) select s.FirstDayOfWeek.AddDays(d);
																			return (
																						from d in days
																						let shiftCategory = (
																													from pd in preferenceDays
																													where
																														pd.Restriction != null &&
																														pd.Restriction.ShiftCategory != null &&
																														pd.RestrictionDate == d
																													select pd.Restriction.ShiftCategory
																												  ).SingleOrDefault()
																						let dayOffTemplate = (
																												from pd in preferenceDays
																												where
																													pd.Restriction != null &&
																													pd.Restriction.DayOffTemplate != null &&
																													pd.RestrictionDate == d
																												select pd.Restriction.DayOffTemplate
																											).SingleOrDefault()
																						let absence = (
																											from pd in preferenceDays
																											where
																												pd.Restriction != null &&
																												pd.Restriction.Absence != null &&
																												pd.RestrictionDate == d
																											select pd.Restriction.Absence
																										).SingleOrDefault()
																						let workTimeMinMax = (
																											from w in workTimeMinMaxCollection
																											where w.Date == d
																											select w.WorkTimeMinMax
																											).SingleOrDefault()
																						select new PreferenceDayDomainData(d, s.Period, shiftCategory, dayOffTemplate, absence, s.WorkflowControlSet, workTimeMinMax)).ToArray();
																		}))
				;

			CreateMap<PreferenceDayDomainData, DayViewModelBase>()
				.ConvertUsing(s => _mappingEngine().Map<PreferenceDayDomainData, PreferenceDayViewModel>(s))
				;

			CreateMap<PreferenceDayDomainData, PreferenceDayViewModel>()
				.ForMember(d => d.Preference, o => o.MapFrom(s =>
																				{
																					if (s.DayOffTemplate != null)
																						return s.DayOffTemplate.Description.Name;
																					if (s.Absence != null)
																						return s.Absence.Description.Name;
																					if (s.ShiftCategory != null)
																						return s.ShiftCategory.Description.Name;
																					return null;
																				}))
				.ForMember(d => d.Date, o => o.MapFrom(s => s.Date))
				.ForMember(d => d.State, o => o.MapFrom(s =>
																		{
																			if (s.WorkflowControlSet != null)
																			{
																				var isInsideSchedulePeriod = s.Period.Contains(s.Date);
																				var isInsidePreferencePeriod = s.WorkflowControlSet.PreferencePeriod.Contains(s.Date);
																				var isInsidePreferenceInputPeriod =
																					s.WorkflowControlSet.PreferenceInputPeriod.Contains(DateOnly.Today);

																				if (isInsideSchedulePeriod && isInsidePreferencePeriod && isInsidePreferenceInputPeriod)
																					return DayState.Editable;
																			}

																			return DayState.None;
																		}))
				.ForMember(d => d.Header, o => o.MapFrom(s => s))
				.ForMember(d => d.StyleClassName, o => o.Ignore())
				.ForMember(d => d.PossibleStartTimes, o => o.MapFrom(s => s.WorkTimeMinMax == null
																			? ""
																			: s.WorkTimeMinMax.StartTimeLimitation.
																				StartTimeString +
																			  "-" +
																			  s.WorkTimeMinMax.StartTimeLimitation.
																				EndTimeString))
				.ForMember(d => d.PossibleEndTimes, o => o.MapFrom(s => s.WorkTimeMinMax == null
																			? ""
																			: s.WorkTimeMinMax.EndTimeLimitation.
																				StartTimeString +
																			  "-" +
																			  s.WorkTimeMinMax.EndTimeLimitation.
																				EndTimeString))
				.ForMember(d => d.PossibleContractTimes, o => o.MapFrom(s => s.WorkTimeMinMax == null
																			? ""
																			: s.WorkTimeMinMax.WorkTimeLimitation.
																				StartTimeString +
																			  "-" +
																			  s.WorkTimeMinMax.WorkTimeLimitation.
																				EndTimeString))
				;

			CreateMap<PreferenceDayDomainData, HeaderViewModel>()
				.ForMember(d => d.DayNumber, o => o.MapFrom(s => s.Date.Day))
				.ForMember(d => d.DayDescription, o => o.MapFrom(s =>
																					{
																						var firstDisplayDate = new DateOnly(DateHelper.GetFirstDateInWeek(s.Period.StartDate, CultureInfo.CurrentCulture).AddDays(-7));
																						if (s.Date.Day == 1 || s.Date == firstDisplayDate)
																							return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(s.Date.Month);
																						return string.Empty;
																					}))
				;

			CreateMap<IWorkflowControlSet, PreferencePeriodViewModel>()
				.ForMember(d => d.Period, c => c.MapFrom(s => s.PreferencePeriod))
				.ForMember(d => d.OpenPeriod, c => c.MapFrom(s => s.PreferenceInputPeriod))
				;
		}
	}
}