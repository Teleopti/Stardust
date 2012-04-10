using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceViewModelMappingProfile : Profile
	{
		private readonly IResolve<IScheduleColorProvider> _scheduleColorProvider;
		private readonly IResolve<IHasDayOffUnderFullDayAbsence> _hasDayOffUnderFullDayAbsence;

		public PreferenceViewModelMappingProfile(IResolve<IScheduleColorProvider> scheduleColorProvider, IResolve<IHasDayOffUnderFullDayAbsence> hasDayOffUnderFullDayAbsence)
		{
			_scheduleColorProvider = scheduleColorProvider;
			_hasDayOffUnderFullDayAbsence = hasDayOffUnderFullDayAbsence;
		}

		private class PreferenceWeekMappingData
		{
			public DateOnly FirstDayOfWeek { get; set; }
			public DateOnlyPeriod Period { get; set; }
			public IEnumerable<PreferenceDayDomainData> Days { get; set; }
			public IWorkflowControlSet WorkflowControlSet { get; set; }
		}

		private class DayMappingData
		{
			public DateOnly Date { get; set; }
			public DateOnlyPeriod Period { get; set; }

			public IScheduleDay ScheduleDay { get; set; }
			public SchedulePartView SignificantPart { get; set; }
			public bool HasDayOffUnderAbsence { get; set; }
			public IVisualLayerCollection Projection { get; set; }

			public IWorkflowControlSet WorkflowControlSet { get; set; }
			public IShiftCategory ShiftCategory { get; set; }
			public IDayOffTemplate DayOffTemplate { get; set; }
			public IAbsence Absence { get; set; }
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

				                                        		return (from d in firstDatesOfWeeks
				                                        		        select
				                                        		        	new PreferenceWeekMappingData
				                                        		        		{
				                                        		        			FirstDayOfWeek = d,
				                                        		        			Period = s.Period,
				                                        		        			Days = s.Days,
				                                        		        			WorkflowControlSet = s.WorkflowControlSet,
				                                        		        		}).ToArray();
				                                        	}))
				.ForMember(d => d.PreferencePeriod, c => c.MapFrom(s => s.WorkflowControlSet))
				.ForMember(d => d.Styles, c => c.MapFrom(s => _scheduleColorProvider.Invoke().GetColors(s.ColorSource)))
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

			CreateMap<PreferenceWeekMappingData, WeekViewModel>()
				.ForMember(d => d.Days, o => o.MapFrom(s =>
				                                       	{
				                                       		var days = s.Days ?? new PreferenceDayDomainData[] {};
				                                       		var datesThisWeek = from d in Enumerable.Range(0, 7) select s.FirstDayOfWeek.AddDays(d);
				                                       		return (
				                                       		       	from d in datesThisWeek
				                                       		       	let day = (from day in days where day.Date == d select day).SingleOrDefault()
				                                       		       	let preferenceDay = day == null ? null : day.PreferenceDay
				                                       		       	let restriction = preferenceDay == null ? null : preferenceDay.Restriction
				                                       		       	let shiftCategory = restriction == null ? null : restriction.ShiftCategory
				                                       		       	let dayOffTemplate = restriction == null ? null : restriction.DayOffTemplate
				                                       		       	let absence = restriction == null ? null : restriction.Absence
				                                       		       	let projection = day == null ? null : day.Projection
				                                       		       	let scheduleDay = day == null ? null : day.ScheduleDay
				                                       		       	let significantPart = scheduleDay == null ? SchedulePartView.None : scheduleDay.SignificantPartForDisplay()
																	let hasDayOffUnderAbsence = scheduleDay != null && _hasDayOffUnderFullDayAbsence.Invoke().HasDayOff(scheduleDay)
				                                       		       	select
				                                       		       		new DayMappingData
				                                       		       			{
				                                       		       				Date = d,
				                                       		       				Period = s.Period,

				                                       		       				Projection = projection,
				                                       		       				ScheduleDay = scheduleDay,
																				SignificantPart = significantPart,
																				HasDayOffUnderAbsence = hasDayOffUnderAbsence,

				                                       		       				ShiftCategory = shiftCategory,
				                                       		       				DayOffTemplate = dayOffTemplate,
				                                       		       				Absence = absence,
				                                       		       				WorkflowControlSet = s.WorkflowControlSet,
				                                       		       			}
				                                       		       ).ToArray();
				                                       	}))
				;

			CreateMap<DayMappingData, DayViewModel>()
				.ForMember(d => d.Date, o => o.MapFrom(s => s.Date))
				.ForMember(d => d.Editable, o => o.MapFrom(s =>
				                                           	{
				                                           		if (s.ScheduleDay != null)
				                                           			return false;
				                                           		if (s.WorkflowControlSet == null)
				                                           			return false;

				                                           		var isInsideSchedulePeriod = s.Period.Contains(s.Date);
				                                           		var isInsidePreferencePeriod = s.WorkflowControlSet.PreferencePeriod.Contains(s.Date);
				                                           		var isInsidePreferenceInputPeriod = s.WorkflowControlSet.PreferenceInputPeriod.Contains(DateOnly.Today);

				                                           		return isInsideSchedulePeriod && isInsidePreferencePeriod && isInsidePreferenceInputPeriod;
				                                           	}))
				.ForMember(d => d.Header, o => o.MapFrom(s => s))
				.ForMember(d => d.StyleClassName, o => o.MapFrom(s =>
				                                                 	{
																		if (s.ScheduleDay != null)
																		{
																			if (s.HasDayOffUnderAbsence)
																				return s.ScheduleDay.PersonAbsenceCollection().First().Layer.Payload.DisplayColor.ToStyleClass()
																					   + " " + StyleClasses.Striped;
																			if (s.SignificantPart == SchedulePartView.FullDayAbsence)
																				return s.ScheduleDay.PersonAbsenceCollection().First().Layer.Payload.DisplayColor.ToStyleClass();
																			if (s.SignificantPart == SchedulePartView.MainShift)
																				return s.ScheduleDay.AssignmentHighZOrder().MainShift.ShiftCategory.DisplayColor.ToStyleClass();
																			if (s.SignificantPart == SchedulePartView.DayOff)
																				return StyleClasses.DayOff + " " + StyleClasses.Striped;
																		}
																		if (s.ShiftCategory != null)
																			return s.ShiftCategory.DisplayColor.ToStyleClass();
																		if (s.Absence != null)
																			return s.Absence.DisplayColor.ToStyleClass();
																		if (s.DayOffTemplate != null)
																			return s.DayOffTemplate.DisplayColor.ToStyleClass();
				                                                 		return null;
				                                                 	}))
				.ForMember(d => d.Preference, o => o.MapFrom(s => s.SignificantPart == SchedulePartView.None ? s : null))
				.ForMember(d => d.PersonAssignment, o => o.MapFrom(s => s.SignificantPart == SchedulePartView.MainShift ? s : null))
				.ForMember(d => d.DayOff, o => o.MapFrom(s => s.SignificantPart == SchedulePartView.DayOff ? s : null))
				.ForMember(d => d.Absence, o => o.MapFrom(s => s.SignificantPart == SchedulePartView.FullDayAbsence ? s : null))
				.ForMember(d => d.Feedback, o => o.MapFrom(s => s.Period.Contains(s.Date)))
				;
			
			CreateMap<DayMappingData, PersonAssignmentDayViewModel>()
				.ForMember(d => d.ContractTime, o => o.MapFrom(s => TimeHelper.GetLongHourMinuteTimeString(s.Projection.ContractTime(), CultureInfo.CurrentUICulture)))
				.ForMember(d => d.ShiftCategory, o => o.MapFrom(s => s.ScheduleDay.AssignmentHighZOrder().MainShift.ShiftCategory.Description.Name))
				.ForMember(d => d.TimeSpan, o => o.MapFrom(s => s.ScheduleDay.AssignmentHighZOrder().Period.TimePeriod(s.ScheduleDay.TimeZone).ToShortTimeString()))
				;

			CreateMap<DayMappingData, DayOffDayViewModel>()
				.ForMember(d => d.DayOff, o => o.MapFrom(s => s.ScheduleDay.PersonDayOffCollection().Single().DayOff.Description.Name))
				;

			CreateMap<DayMappingData, AbsenceDayViewModel>()
				.ForMember(d => d.Absence, o => o.MapFrom(s => s.ScheduleDay.PersonAbsenceCollection().First().Layer.Payload.Description.Name))
				;

			CreateMap<DayMappingData, PreferenceDayViewModel>()
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
				;

			CreateMap<DayMappingData, HeaderViewModel>()
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

		private static bool IsDayEditable(DayMappingData s)
		{
			if (s.WorkflowControlSet != null)
			{
				var isInsideSchedulePeriod = s.Period.Contains(s.Date);
				var isInsidePreferencePeriod = s.WorkflowControlSet.PreferencePeriod.Contains(s.Date);
				var isInsidePreferenceInputPeriod =
					s.WorkflowControlSet.PreferenceInputPeriod.Contains(DateOnly.Today);

				if (isInsideSchedulePeriod && isInsidePreferencePeriod && isInsidePreferenceInputPeriod)
					return true;
			}

			return false;
		}
	}

}