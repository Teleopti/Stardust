using System.Web;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.ViewModelFactory
{
	[TestFixture]
	public class PreferenceViewModelFactoryTest
	{

		[Test]
		public void ShoudCreateViewModelByTwoStepMapping()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new PreferenceViewModelFactory(mapper, null);
			var domainData = new PreferenceDomainData();
			var viewModel = new PreferenceViewModel();

			mapper.Stub(x => x.Map<DateOnly, PreferenceDomainData>(DateOnly.Today)).Return(domainData);
			mapper.Stub(x => x.Map<PreferenceDomainData, PreferenceViewModel>(domainData)).Return(viewModel);

			var result = target.CreateViewModel(DateOnly.Today);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldCreateDayViewModelByMapping()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceProvider = MockRepository.GenerateMock<IPreferenceProvider>();
			var target = new PreferenceViewModelFactory(mapper, preferenceProvider);
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());
			var viewModel = new PreferenceDayViewModel();

			preferenceProvider.Stub(x => x.GetPreferencesForDate(DateOnly.Today)).Return(preferenceDay);
			mapper.Stub(x => x.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay)).Return(viewModel);

			var result = target.CreateDayViewModel(DateOnly.Today);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldThrow404IfPreferenceDoesNotExist()
		{
			var preferenceProvider = MockRepository.GenerateMock<IPreferenceProvider>();
			var target = new PreferenceViewModelFactory(null, preferenceProvider);

			preferenceProvider.Stub(x => x.GetPreferencesForDate(DateOnly.Today)).Return(null);

			var exception = Assert.Throws<HttpException>(() => target.CreateDayViewModel(DateOnly.Today));
			exception.GetHttpCode().Should().Be(404);
		}

		[Test]
		public void ShouldCreateFeedbackDayViewModelByMapping()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new PreferenceViewModelFactory(mapper, null);
			var viewModel = new PreferenceDayFeedbackViewModel();

			mapper.Stub(x => x.Map<DateOnly, PreferenceDayFeedbackViewModel>(DateOnly.Today)).Return(viewModel);

			var result = target.CreateDayFeedbackViewModel(DateOnly.Today);

			result.Should().Be.SameInstanceAs(viewModel);
		}

	}
		}
