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
		private class PreferenceWeekMappingData
		{
			public DateOnly FirstDayOfWeek { get; set; }
			public DateOnlyPeriod Period { get; set; }
			public IWorkflowControlSet WorkflowControlSet { get; set; }
		}

		private class DayMappingData
		{
			public DateOnly Date { get; set; }
			public DateOnlyPeriod Period { get; set; }
			public IWorkflowControlSet WorkflowControlSet { get; set; }
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
				                                        		        			WorkflowControlSet = s.WorkflowControlSet,
				                                        		        		}).ToArray();
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

			CreateMap<PreferenceWeekMappingData, WeekViewModel>()
				.ForMember(d => d.Days, o => o.MapFrom(s =>
				                                       	{
				                                       		var datesThisWeek = from d in Enumerable.Range(0, 7) select s.FirstDayOfWeek.AddDays(d);
				                                       		return (
				                                       		       	from d in datesThisWeek
				                                       		       	select
				                                       		       		new DayMappingData
				                                       		       			{
				                                       		       				Date = d,
				                                       		       				Period = s.Period,
				                                       		       				WorkflowControlSet = s.WorkflowControlSet,
				                                       		       			}
				                                       		       ).ToArray();
				                                       	}))
				;

			CreateMap<DayMappingData, DayViewModel>()
				.ForMember(d => d.Date, o => o.MapFrom(s => s.Date))
				.ForMember(d => d.Header, o => o.MapFrom(s => s))
				.ForMember(d => d.Editable, o => o.MapFrom(s =>
				                                           	{
				                                           		if (s.WorkflowControlSet == null)
				                                           			return false;

				                                           		var isInsideSchedulePeriod = s.Period.Contains(s.Date);
				                                           		var isInsidePreferencePeriod = s.WorkflowControlSet.PreferencePeriod.Contains(s.Date);
				                                           		var isInsidePreferenceInputPeriod = s.WorkflowControlSet.PreferenceInputPeriod.Contains(DateOnly.Today);

				                                           		return isInsideSchedulePeriod && isInsidePreferencePeriod && isInsidePreferenceInputPeriod;
				                                           	}))
				.ForMember(d => d.InPeriod, o => o.MapFrom(s => s.Period.Contains(s.Date)))
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
	}
}