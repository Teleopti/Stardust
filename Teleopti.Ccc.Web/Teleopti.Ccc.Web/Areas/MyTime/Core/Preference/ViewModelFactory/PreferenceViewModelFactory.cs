using System.Web;
using AutoMapper;
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

		public PreferenceViewModelFactory(IMappingEngine mapper, IPreferenceProvider preferenceProvider)
		{
			_mapper = mapper;
			_preferenceProvider = preferenceProvider;
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
			if (preferenceDay == null)
				throw new HttpException(404, "Preference not found");
			return _mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);
		}
	}
}