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
		private readonly IPreferenceTemplatesProvider _preferenceTemplatesProvider;

		public PreferenceViewModelFactory(IMappingEngine mapper, IPreferenceProvider preferenceProvider, IScheduleProvider scheduleProvider, IPreferenceTemplatesProvider preferenceTemplatesProvider)
		{
			_mapper = mapper;
			_preferenceProvider = preferenceProvider;
			_scheduleProvider = scheduleProvider;
			_preferenceTemplatesProvider = preferenceTemplatesProvider;
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
			var templates = _preferenceTemplatesProvider.RetrievePreferenceTemplates();
			return _mapper.Map<IEnumerable<IExtendedPreferenceTemplate>, IEnumerable<PreferenceTemplateViewModel>>(templates);
		}
	}
}