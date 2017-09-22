using System;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.DataProvider
{
	[TestFixture]
	public class PreferenceTemplatePersisterTest
	{
		[Test]
		public void ShouldAddPreferenceTemplate()
		{
			var preferenceTemplateRepository = MockRepository.GenerateMock<IExtendedPreferenceTemplateRepository>();
			var target = new PreferenceTemplatePersister(preferenceTemplateRepository, new FakeLoggedOnUser(), new FakeShiftCategoryRepository(), new FakeAbsenceRepository(), new FakeDayOffTemplateRepository(), new FakeActivityRepository());
			var input = new PreferenceTemplateInput();
			
			target.Persist(input);

			preferenceTemplateRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldReturnInputResultModelOnAdd()
		{
			var target = new PreferenceTemplatePersister(MockRepository.GenerateStub<IExtendedPreferenceTemplateRepository>(), new FakeLoggedOnUser(), new FakeShiftCategoryRepository(), new FakeAbsenceRepository(), new FakeDayOffTemplateRepository(), new FakeActivityRepository());
			var input = new PreferenceTemplateInput();

			target.Persist(input).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldDeletePreferenceTemplate()
		{
			var preferenceTemplateRepository = MockRepository.GenerateMock<IExtendedPreferenceTemplateRepository>();
			var target = new PreferenceTemplatePersister(preferenceTemplateRepository, new FakeLoggedOnUser(), new FakeShiftCategoryRepository(), new FakeAbsenceRepository(), new FakeDayOffTemplateRepository(), new FakeActivityRepository());
			var id = Guid.NewGuid();
			var extendedPreferenceTemplate = MockRepository.GenerateMock<IExtendedPreferenceTemplate>();
			preferenceTemplateRepository.Stub(x => x.Find(id)).Return(extendedPreferenceTemplate);

			target.Delete(id);

			preferenceTemplateRepository.AssertWasCalled(x => x.Remove(extendedPreferenceTemplate));
		}

		[Test]
		public void ShouldThrowHttp404OIfPreferenceDoesNotExists()
		{
			var preferenceTemplateRepository = MockRepository.GenerateMock<IExtendedPreferenceTemplateRepository>();
			var id = Guid.NewGuid();
			var target = new PreferenceTemplatePersister(MockRepository.GenerateStub<IExtendedPreferenceTemplateRepository>(), new FakeLoggedOnUser(), new FakeShiftCategoryRepository(), new FakeAbsenceRepository(), new FakeDayOffTemplateRepository(), new FakeActivityRepository());

			preferenceTemplateRepository.Stub(x => x.Find(id)).Return(null);

			var exception = Assert.Throws<HttpException>(() => target.Delete(id));
			exception.GetHttpCode().Should().Be(404);
		}
	}
}