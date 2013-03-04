using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.DataProvider
{
	[TestFixture]
	public class PreferenceTemplatePersisterTest
	{
		[Test]
		public void ShouldAddPreferenceTemplate()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceTemplateRepository = MockRepository.GenerateMock<IExtendedPreferenceTemplateRepository>();
			var target = new PreferenceTemplatePersister(preferenceTemplateRepository, mapper);
			var input = new PreferenceTemplateInput();
			var extendedPreferenceTemplate = MockRepository.GenerateMock<IExtendedPreferenceTemplate>();
			mapper.Stub(x => x.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(input)).Return(extendedPreferenceTemplate);

			target.Persist(input);

			preferenceTemplateRepository.AssertWasCalled(x => x.Add(extendedPreferenceTemplate));
		}
	}
}