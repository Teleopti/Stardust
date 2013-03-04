using AutoMapper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceTemplatePersister : IPreferenceTemplatePersister
	{
		private readonly IExtendedPreferenceTemplateRepository _preferenceTemplateRepository;
		private readonly IMappingEngine _mapper;

		public PreferenceTemplatePersister(IExtendedPreferenceTemplateRepository preferenceTemplateRepository, IMappingEngine mapper)
		{
			_preferenceTemplateRepository = preferenceTemplateRepository;
			_mapper = mapper;
		}

		public void Persist(PreferenceTemplateInput input)
		{
			_preferenceTemplateRepository.Add(_mapper.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(input));
		}
	}
}