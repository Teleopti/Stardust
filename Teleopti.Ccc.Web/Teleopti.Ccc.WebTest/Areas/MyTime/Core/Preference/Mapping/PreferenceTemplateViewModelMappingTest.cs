using System.Drawing;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceTemplateViewModelMappingTest
	{
		[SetUp]
		public void Setup()
		{
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new PreferenceTemplateViewModelMappingProfile()));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapName()
		{
			var restriction = MockRepository.GenerateStub<IPreferenceRestrictionTemplate>();
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, restriction, "name", Color.Red);
			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.Name.Should().Be.EqualTo("name");
		}
	}
}