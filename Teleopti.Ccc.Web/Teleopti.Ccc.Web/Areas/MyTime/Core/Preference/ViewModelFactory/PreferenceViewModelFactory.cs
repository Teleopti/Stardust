using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory
{
	public class PreferenceViewModelFactory : IPreferenceViewModelFactory
	{
		private readonly IPreferenceProvider _preferenceProvider;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IPreferenceTemplateProvider _preferenceTemplateProvider;
		private readonly IPreferenceWeeklyWorkTimeSettingProvider _preferenceWeeklyWorkTimeSettingProvider;
		private readonly PreferenceAndScheduleDayViewModelMapper _preferenceAndScheduleMapper;
		private readonly PreferenceDayViewModelMapper _preferenceDayMapper;
		private readonly PreferenceDayFeedbackViewModelMapper _feedbackMapper;
		private readonly PreferenceViewModelMapper _preferenceViewMapper;
		private readonly PreferenceDomainDataMapper _preferenceDomainMapper;
		private readonly ExtendedPreferenceTemplateMapper _templateMapper;

		public PreferenceViewModelFactory(IPreferenceProvider preferenceProvider, IScheduleProvider scheduleProvider,
			IPreferenceTemplateProvider preferenceTemplateProvider,
			IPreferenceWeeklyWorkTimeSettingProvider preferenceWeeklyWorkTimeSettingProvider,
			PreferenceAndScheduleDayViewModelMapper preferenceAndScheduleMapper, PreferenceDayViewModelMapper preferenceDayMapper,
			PreferenceDayFeedbackViewModelMapper feedbackMapper, PreferenceViewModelMapper preferenceViewMapper,
			PreferenceDomainDataMapper preferenceDomainMapper, ExtendedPreferenceTemplateMapper templateMapper)
		{
			_preferenceProvider = preferenceProvider;
			_scheduleProvider = scheduleProvider;
			_preferenceTemplateProvider = preferenceTemplateProvider;
			_preferenceWeeklyWorkTimeSettingProvider = preferenceWeeklyWorkTimeSettingProvider;
			_preferenceAndScheduleMapper = preferenceAndScheduleMapper;
			_preferenceDayMapper = preferenceDayMapper;
			_feedbackMapper = feedbackMapper;
			_preferenceViewMapper = preferenceViewMapper;
			_preferenceDomainMapper = preferenceDomainMapper;
			_templateMapper = templateMapper;
		}

		public PreferenceViewModel CreateViewModel(DateOnly date)
		{
			var preferenceDomainData = _preferenceDomainMapper.Map(date);
			return _preferenceViewMapper.Map(preferenceDomainData);
		}

		public PreferenceDayFeedbackViewModel CreateDayFeedbackViewModel(DateOnly date)
		{
			return _feedbackMapper.Map(date);
		}

		public IEnumerable<PreferenceDayFeedbackViewModel> CreateDayFeedbackViewModel(DateOnlyPeriod period)
		{
			return _feedbackMapper.Map(period);
		}

		public PreferenceDayViewModel CreateDayViewModel(DateOnly date)
		{
			var preferenceDay = _preferenceProvider.GetPreferencesForDate(date);
			if (preferenceDay == null) return null;

			return _preferenceDayMapper.Map(preferenceDay);
		}

		public IEnumerable<PreferenceAndScheduleDayViewModel> CreatePreferencesAndSchedulesViewModel(DateOnly from, DateOnly to)
		{
			var period = new DateOnlyPeriod(from, to);
			var scheduleDays = _scheduleProvider.GetScheduleForPeriod(period);
			return scheduleDays.Select(_preferenceAndScheduleMapper.Map).ToArray();
		}

		public IEnumerable<PreferenceTemplateViewModel> CreatePreferenceTemplateViewModels()
		{
			var templates = _preferenceTemplateProvider.RetrievePreferenceTemplates();
			return templates.Select(_templateMapper.Map).ToArray();
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

		public IDictionary<DateOnly, PreferenceWeeklyWorkTimeViewModel> CreatePreferenceWeeklyWorkTimeViewModels(
			IEnumerable<DateOnly> dates)
		{
			if (dates.IsNullOrEmpty())
				return new Dictionary<DateOnly, PreferenceWeeklyWorkTimeViewModel>();
			return dates.Distinct().ToDictionary(date => date, CreatePreferenceWeeklyWorkTimeViewModel);
		}
	}
}