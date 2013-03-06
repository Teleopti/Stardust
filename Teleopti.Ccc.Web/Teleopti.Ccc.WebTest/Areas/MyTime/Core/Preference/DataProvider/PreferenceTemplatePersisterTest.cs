using System;
using System.Web;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
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

		[Test]
		public void ShouldReturnInputResultModelOnAdd()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var template = MockRepository.GenerateMock<IExtendedPreferenceTemplate>();
			var target = new PreferenceTemplatePersister(MockRepository.GenerateStub<IExtendedPreferenceTemplateRepository>(), mapper);
			var input = new PreferenceTemplateInput();
			var inputResult = new PreferenceTemplateViewModel();

			mapper.Stub(x => x.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(input)).Return(template);
			mapper.Stub(x => x.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(template)).Return(inputResult);

			var result = target.Persist(input);

			result.Should().Be.SameInstanceAs(inputResult);
		}

		[Test]
		public void ShouldDeletePreferenceTemplate()
		{
			var preferenceTemplateRepository = MockRepository.GenerateMock<IExtendedPreferenceTemplateRepository>();
			var target = new PreferenceTemplatePersister(preferenceTemplateRepository, null);
			var id = Guid.NewGuid();
			var extendedPreferenceTemplate = MockRepository.GenerateMock<IExtendedPreferenceTemplate>();
			preferenceTemplateRepository.Stub(x => x.Load(id)).Return(extendedPreferenceTemplate);

			target.Delete(id);

			preferenceTemplateRepository.AssertWasCalled(x => x.Remove(extendedPreferenceTemplate));
		}

		[Test]
		public void ShouldThrowHttp404OIfPreferenceDoesNotExists()
		{
			var preferenceTemplateRepository = MockRepository.GenerateMock<IExtendedPreferenceTemplateRepository>();
			var id = Guid.NewGuid();
			var target = new PreferenceTemplatePersister(MockRepository.GenerateStub<IExtendedPreferenceTemplateRepository>(), null);

			preferenceTemplateRepository.Stub(x => x.Load(id)).Return(null);

			var exception = Assert.Throws<HttpException>(() => target.Delete(id));
			exception.GetHttpCode().Should().Be(404);
		}

		[Test]
		public void ShouldThrowHttp404OIfRequestDeniedOrApproved()
		{
			var preferenceTemplateRepository = MockRepository.GenerateMock<IExtendedPreferenceTemplateRepository>();
			var extendedPreferenceTemplate = MockRepository.GenerateMock<IExtendedPreferenceTemplate>();
			var id = Guid.NewGuid();
			var target = new PreferenceTemplatePersister(MockRepository.GenerateStub<IExtendedPreferenceTemplateRepository>(), null);

			preferenceTemplateRepository.Stub(x => x.Load(id)).Return(extendedPreferenceTemplate);

			preferenceTemplateRepository.Stub(x => x.Remove(extendedPreferenceTemplate)).Throw(new DataSourceException());

			var exception = Assert.Throws<HttpException>(() => target.Delete(id));
			exception.GetHttpCode().Should().Be(404);
		}
	}
}