using System.Collections.Generic;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory
{
	public class PreferenceViewModelFactory : IPreferenceViewModelFactory
	{
		private readonly IMappingEngine _mapper;
		private readonly IPreferenceProvider _preferenceProvider;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IPreferenceTemplateProvider _preferenceTemplateProvider;
		private readonly IPreferenceWeeklyWorkTimeSettingProvider _preferenceWeeklyWorkTimeSettingProvider;

		public PreferenceViewModelFactory(IMappingEngine mapper, IPreferenceProvider preferenceProvider, IScheduleProvider scheduleProvider, IPreferenceTemplateProvider preferenceTemplateProvider, IPreferenceWeeklyWorkTimeSettingProvider preferenceWeeklyWorkTimeSettingProvider)
		{
			_mapper = mapper;
			_preferenceProvider = preferenceProvider;
			_scheduleProvider = scheduleProvider;
			_preferenceTemplateProvider = preferenceTemplateProvider;
			_preferenceWeeklyWorkTimeSettingProvider = preferenceWeeklyWorkTimeSettingProvider;
		}

		public PreferenceViewModel CreateViewModel(DateOnly date)
		{
			var preferenceDomainData = _mapper.Map<DateOnly, PreferenceDomainData>(date);
			return _mapper.Map<PreferenceDomainData, PreferenceViewModel>(preferenceDomainData);
		}

		public PreferenceDayFeedbackViewModel CreateDayFeedbackViewModel(DateOnly date)
		{
			return _mapper.Map<DateOnly, PreferenceDayFeedbackViewModel>(date);
		}

		public PreferenceDayViewModel CreateDayViewModel(DateOnly date)
		{
			var preferenceDay = _preferenceProvider.GetPreferencesForDate(date);
			if (preferenceDay == null) return null;

			return _mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);
		}

		public IEnumerable<PreferenceAndScheduleDayViewModel> CreatePreferencesAndSchedulesViewModel(DateOnly @from, DateOnly to)
		{
			var period = new DateOnlyPeriod(@from, to);
			var scheduleDays = _scheduleProvider.GetScheduleForPeriod(period);
			return _mapper.Map<IEnumerable<IScheduleDay>, IEnumerable<PreferenceAndScheduleDayViewModel>>(scheduleDays);
		}

		public IEnumerable<PreferenceTemplateViewModel> CreatePreferenceTemplateViewModels()
		{
			var templates = _preferenceTemplateProvider.RetrievePreferenceTemplates();
			return _mapper.Map<IEnumerable<IExtendedPreferenceTemplate>, IEnumerable<PreferenceTemplateViewModel>>(templates);
		}

		public PreferenceWeeklyWorkTimeViewModel CreatePreferenceWeeklyWorkTimeViewModel(DateOnly date)
		{
			 var setting = _preferenceWeeklyWorkTimeSettingProvider.RetrieveSetting(date);
			 return new PreferenceWeeklyWorkTimeViewModel
			 {
				  MaxWorkTimePerWeekMinutes = setting.MaxWorkTimePerWeekMinutes,
				  MinWorkTimePerWeekMinutes = setting.MinWorkTimePerWeekMinutes,
			 };
		}
	}
}