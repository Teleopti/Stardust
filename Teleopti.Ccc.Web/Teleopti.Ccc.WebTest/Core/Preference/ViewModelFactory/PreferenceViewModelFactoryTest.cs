using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Preference.ViewModelFactory
{
	[TestFixture]
	public class PreferenceViewModelFactoryTest
	{

		[Test]
		public void ShoudCreateViewModelByTwoStepMapping()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new PreferenceViewModelFactory(mapper);
			var domainData = new PreferenceDomainData();
			var viewModel = new PreferenceViewModel();

			mapper.Stub(x => x.Map<DateOnly, PreferenceDomainData>(DateOnly.Today)).Return(domainData);
			mapper.Stub(x => x.Map<PreferenceDomainData, PreferenceViewModel>(domainData)).Return(viewModel);

			var result = target.CreateViewModel(DateOnly.Today);

			result.Should().Be.SameInstanceAs(viewModel);
		}

	}
}
