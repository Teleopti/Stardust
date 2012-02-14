using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory
{
	public class PreferenceViewModelFactory : IPreferenceViewModelFactory
	{
		private readonly IMappingEngine _mapper;

		public PreferenceViewModelFactory(IMappingEngine mapper) {
			_mapper = mapper;
		}

		public PreferenceViewModel CreateViewModel(DateOnly date)
		{
			var preferenceDomainData = _mapper.Map<DateOnly, PreferenceDomainData>(date);
			return _mapper.Map<PreferenceDomainData, PreferenceViewModel>(preferenceDomainData);
		}
	}
}