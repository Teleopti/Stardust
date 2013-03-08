using System;
using System.Web;
using AutoMapper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
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

		public PreferenceTemplateViewModel Persist(PreferenceTemplateInput input)
		{
			var extendedPreferenceTemplate = _mapper.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(input);
			_preferenceTemplateRepository.Add(extendedPreferenceTemplate);
			return _mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);
		}

		public void Delete(Guid templateId)
		{
			var template = _preferenceTemplateRepository.Find(templateId);
			if (template == null)
				throw new HttpException(404, "Preference template not found");
			_preferenceTemplateRepository.Remove(template);
		}
	}
}