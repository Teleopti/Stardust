using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceViewModelMappingProfile : Profile
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly Func<IPreferenceOptionsProvider> _preferenceOptionsProvider;

		public PreferenceViewModelMappingProfile(IPermissionProvider permissionProvider, Func<IPreferenceOptionsProvider> preferenceOptionsProvider)
		{
			_permissionProvider = permissionProvider;
			_preferenceOptionsProvider = preferenceOptionsProvider;
		}

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
				.ForMember(d => d.Weeks, o => o.ResolveUsing(s =>
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
				.ForMember(d => d.ExtendedPreferencesPermission, c => c.ResolveUsing(s => _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb)))
				.ForMember(d => d.Options, c => c.ResolveUsing(s => new PreferenceOptionsViewModel(PreferenceOptions(), ActivityOptions())))
				;

			CreateMap<string, WeekDayHeader>()
				.ForMember(d => d.Title, o => o.MapFrom(s => s))
				;

			CreateMap<PreferenceDomainData, PeriodSelectionViewModel>()
				.ForMember(d => d.Date, o => o.MapFrom(s => s.SelectedDate.ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.Display, o => o.MapFrom(s => s.Period.DateString))
				.ForMember(d => d.SelectableDateRange, o => o.MapFrom(s => new DateOnlyPeriod(new DateOnly(CultureInfo.CurrentCulture.Calendar.MinSupportedDateTime), new DateOnly(CultureInfo.CurrentCulture.Calendar.MaxSupportedDateTime))))
				.ForMember(d => d.SelectedDateRange, o => o.MapFrom(s => s.Period))
				.ForMember(d => d.PeriodNavigation, o => o.MapFrom(s => s))
				;

			CreateMap<PreferenceDomainData, PeriodNavigationViewModel>()
				.ForMember(d => d.CanPickPeriod, o => o.UseValue(true))
				.ForMember(d => d.HasNextPeriod, o => o.UseValue(true))
				.ForMember(d => d.HasPrevPeriod, o => o.UseValue(true))
				.ForMember(d => d.NextPeriod, o => o.MapFrom(s => s.Period.EndDate.AddDays(1).ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.PrevPeriod, o => o.MapFrom(s => s.Period.StartDate.AddDays(-1).ToFixedClientDateOnlyFormat()))
				;

			CreateMap<DateOnlyPeriod, PeriodDateRangeViewModel>()
				.ForMember(d => d.MaxDate, o => o.MapFrom(s => s.EndDate.ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.MinDate, o => o.MapFrom(s => s.StartDate.ToFixedClientDateOnlyFormat()))
				;

			CreateMap<PreferenceWeekMappingData, WeekViewModel>()
				.ForMember(d => d.Days, o => o.ResolveUsing(s =>
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
				.ForMember(d => d.Editable, o => o.ResolveUsing(s =>
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
				.ForMember(d => d.DayDescription, o => o.ResolveUsing(s =>
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

		private PreferenceOptionGroup ActivityOptions()
		{
			return new PreferenceOptionGroup(Resources.Activity, 
											(from a in _preferenceOptionsProvider().RetrieveActivityOptions().MakeSureNotNull()
											select new PreferenceOption
											{
												Value = a.Id.ToString(),
												Text = a.Description.Name,
												Color = a.DisplayColor.ToHtml()
											}).ToList());
		}

		private IEnumerable<PreferenceOptionGroup> PreferenceOptions()
		{
			var shiftCategories =
				_preferenceOptionsProvider()
					.RetrieveShiftCategoryOptions()
					.MakeSureNotNull()
					.Select(s => new PreferenceOption
					{
						Value = s.Id.ToString(),
						Text = s.Description.Name,
						Color = s.DisplayColor.ToHtml(),
						Extended = true
					})
					.OrderBy(pref => pref.Text)
					.ToArray();

			var dayOffs = _preferenceOptionsProvider()
				.RetrieveDayOffOptions()
				.MakeSureNotNull()
				.Select(s => new PreferenceOption
				{
					Value = s.Id.ToString(),
					Text = s.Description.Name,
					Color = s.DisplayColor.ToHtml(),
					Extended = false
				})
				.OrderBy(d => d.Text)
				.ToArray();

			var absences = _preferenceOptionsProvider()
				.RetrieveAbsenceOptions()
				.MakeSureNotNull()
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
	}
}