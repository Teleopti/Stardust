using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping
{
	public class StudentAvailabilityViewModelMappingProfile : Profile
	{
		private readonly Func<IMappingEngine> _mapper;
		private readonly Func<IScheduleProvider> _scheduleProvider;
		private readonly Func<IStudentAvailabilityProvider> _studentAvailabilityProvider;
		private readonly Func<IVirtualSchedulePeriodProvider> _virtualSchedulePeriodProvider;
		private readonly Func<ILoggedOnUser> _loggedOnUser;

		public StudentAvailabilityViewModelMappingProfile(
			Func<IMappingEngine> mapper, 
			Func<IScheduleProvider> scheduleProvider, 
			Func<IStudentAvailabilityProvider> studentAvailabilityProvider, 
			Func<IVirtualSchedulePeriodProvider> virtualSchedulePeriodProvider,
			Func<ILoggedOnUser> loggedOnUser)
		{
			_mapper = mapper;
			_scheduleProvider = scheduleProvider;
			_studentAvailabilityProvider = studentAvailabilityProvider;
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_loggedOnUser = loggedOnUser;
		}

		private TDestination twoStepMapping<TSource, TRelay, TDestination>(TSource source)
		{
			var mapper = _mapper();
			var mappingData = mapper.Map<TSource, TRelay>(source);
			return mapper.Map<TRelay, TDestination>(mappingData);
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<DateOnly, StudentAvailabilityViewModel>()
				.ConvertUsing(twoStepMapping<DateOnly, StudentAvailabilityDomainData, StudentAvailabilityViewModel>);

			// for testing purposes
			CreateMap<DateOnly, PeriodSelectionViewModel>()
				.ConvertUsing(twoStepMapping<DateOnly, StudentAvailabilityDomainData, PeriodSelectionViewModel>);
			CreateMap<DateOnly, DayViewModelBase>()
				.ConvertUsing(twoStepMapping<DateOnly, StudentAvailabilityDomainData, DayViewModelBase>);
			CreateMap<DateOnly, AvailableDayViewModel>()
				.ConvertUsing(twoStepMapping<DateOnly, StudentAvailabilityDomainData, AvailableDayViewModel>);


			CreateMap<DateOnly, StudentAvailabilityDomainData>()
				.ConstructUsing(s => new StudentAvailabilityDomainData(_scheduleProvider(), _loggedOnUser()))
				.ForMember(d => d.ChoosenDate, c => c.MapFrom(s => s))
				.ForMember(d => d.Period, c => c.MapFrom(s => _virtualSchedulePeriodProvider().GetCurrentOrNextVirtualPeriodForDate(s)))
				.ForMember(d => d.ScheduleDays, c => c.Ignore())
				.ForMember(d => d.Person, c => c.Ignore())
				;


			CreateMap<StudentAvailabilityDomainData, StudentAvailabilityViewModel>()
				.ForMember(d => d.PeriodSelection, c => c.MapFrom(s => s))
				.ForMember(d => d.Styles, c => c.UseValue(new StyleClassViewModel[] {}))
				.ForMember(d => d.WeekDayHeaders, c => c.MapFrom(s => (from n in DateHelper.GetWeekdayNames(CultureInfo.CurrentCulture) select new WeekDayHeader {Title = n}).ToList()))
				.ForMember(d => d.Weeks, c => c.MapFrom(s =>
				                                        	{
				                                        		var firstDatesOfWeeks = new List<DateOnly>();
				                                        		var firstDateOfWeek = s.DisplayedPeriod.StartDate;
				                                        		while (firstDateOfWeek < s.DisplayedPeriod.EndDate)
				                                        		{
				                                        			firstDatesOfWeeks.Add(new DateOnly(firstDateOfWeek));
				                                        			firstDateOfWeek = firstDateOfWeek.AddDays(7);
				                                        		}
				                                        		var mappingDatas = firstDatesOfWeeks.Select(d =>
				                                        				new StudentAvailabilityWeekDomainData(d, s.Person, s.Period, s.ScheduleDays));
				                                        		return mappingDatas.ToArray();
				                                        	}))
				.ForMember(d => d.PeriodSummary, c => c.UseValue(new PeriodSummaryViewModel()))
				.ForMember(d => d.StudentAvailabilityPeriod, c => c.MapFrom(s => s.Person.WorkflowControlSet))
				;

			CreateMap<StudentAvailabilityWeekDomainData, WeekViewModel>()
				.ForMember(d => d.Summary, c => c.UseValue(new WeekSummaryViewModel()))
				.ForMember(d => d.Days, c => c.MapFrom(s =>
																		{
																			var dates = s.FirstDateOfWeek.DateRange(7);
																			var dateOnlys = dates.Select(d => new DateOnly(d));
																			var mappingDatas = dateOnlys.Select(d =>
																			                                new StudentAvailabilityDayDomainData(d, s.Period, s.Person, _studentAvailabilityProvider(), s.ScheduleDays));
																			return mappingDatas.ToArray();
																		}))
				;

			CreateMap<StudentAvailabilityDayDomainData, DayViewModelBase>()
				.ConvertUsing(s => _mapper().Map<StudentAvailabilityDayDomainData, AvailableDayViewModel>(s))
				;

			CreateMap<StudentAvailabilityDayDomainData, AvailableDayViewModel>()
				.ForMember(d => d.Date, c => c.MapFrom(s => s.Date))
				.ForMember(d => d.Header, c => c.MapFrom(s => s))
				.ForMember(d => d.State, c => c.MapFrom(s =>
				                                        	{
				                                        		var state = DayState.None;
				                                        		var insideSchedulePeriod = s.Date >= s.Period.StartDate && s.Date <= s.Period.EndDate;
				                                        		var hasWorkflowControlSet = s.Person.WorkflowControlSet != null;
				                                        		if (insideSchedulePeriod && hasWorkflowControlSet)
				                                        		{
				                                        			var insideInputPeriod = s.Person.WorkflowControlSet.StudentAvailabilityInputPeriod.Contains(DateOnly.Today);
				                                        			var insideStudentAvailabilityPeriod = s.Person.WorkflowControlSet.StudentAvailabilityPeriod.Contains(s.Date);
				                                        			if (insideInputPeriod && insideStudentAvailabilityPeriod)
				                                        			{
				                                        				state = state | DayState.Editable;
				                                        				if (s.StudentAvailability != null)
				                                        					state = state | DayState.Deletable;
				                                        			}
				                                        		}
				                                        		return state;
				                                        	}))
				.ForMember(d => d.StyleClassName, c => c.Ignore())
				.ForMember(d => d.AvailableTimeSpan, c => c.MapFrom(s =>
				                                                    	{
				                                                    		var availabilityRestriction = s.StudentAvailability;
				                                                    		return availabilityRestriction == null ? string.Empty : availabilityRestriction.FormatLimitationTimes();
				                                                    	}))
				.ForMember(d => d.StartTimeSpan, c => c.Ignore())
				.ForMember(d => d.EndTimeSpan, c => c.Ignore())
				.ForMember(d => d.WorkTimeSpan, c => c.Ignore())
				;

			CreateMap<StudentAvailabilityDayDomainData, HeaderViewModel>()
				.ForMember(d => d.DayDescription, c => c.MapFrom(s =>
				                                                 	{
				                                                 		if (s.Date.Day.Equals(1))
				                                                 			return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(s.Date.Month);
				                                                 		if (s.Date.Date.Equals(s.DisplayedPeriod.StartDate))
				                                                 			return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(s.Date.Month);
				                                                 		return string.Empty;
				                                                 	}))
				.ForMember(d => d.DayNumber, c => c.MapFrom(s => s.Date.Day.ToString()))
				;

			CreateMap<StudentAvailabilityDomainData, PeriodSelectionViewModel>()
				.ForMember(d => d.Date, c => c.MapFrom(s => s.ChoosenDate.ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.Display, c => c.MapFrom(s => s.Period.DateString))
				.ForMember(d => d.Navigation, c => c.MapFrom(s => s))
				.ForMember(d => d.SelectableDateRange, c => c.MapFrom(s => new PeriodDateRangeViewModel
				                                                           	{
																				MinDate = new DateOnly(CultureInfo.CurrentCulture.Calendar.MinSupportedDateTime).ToFixedClientDateOnlyFormat(),
																				MaxDate = new DateOnly(CultureInfo.CurrentCulture.Calendar.MaxSupportedDateTime).ToFixedClientDateOnlyFormat()
				                                                           	}))
				.ForMember(d => d.SelectedDateRange, c => c.MapFrom(s => new PeriodDateRangeViewModel
				                                                         	{
																				MinDate = s.Period.StartDate.ToFixedClientDateOnlyFormat(),
																				MaxDate = s.Period.EndDate.ToFixedClientDateOnlyFormat()
				                                                         	}))
				;

			CreateMap<StudentAvailabilityDomainData, PeriodNavigationViewModel>()
				.ForMember(d => d.CanPickPeriod, c => c.UseValue(true))
				.ForMember(d => d.HasNextPeriod, c => c.UseValue(true))
				.ForMember(d => d.HasPrevPeriod, c => c.UseValue(true))
				.ForMember(d => d.FirstDateNextPeriod, c => c.MapFrom(s => s.Period.EndDate.AddDays(1).ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.LastDatePreviousPeriod, c => c.MapFrom(s => s.Period.StartDate.AddDays(-1).ToFixedClientDateOnlyFormat()))
				;

			CreateMap<IWorkflowControlSet, StudentAvailabilityPeriodViewModel>()
				.ForMember(d => d.Period, c => c.MapFrom(s => s.StudentAvailabilityPeriod))
				.ForMember(d => d.OpenPeriod, c => c.MapFrom(s => s.StudentAvailabilityInputPeriod))
				;
		}
	}
}