using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
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
			var target = new PreferenceViewModelFactory(mapper);
			var domainData = new PreferenceDomainData();
			var viewModel = new PreferenceViewModel();

			mapper.Stub(x => x.Map<DateOnly, PreferenceDomainData>(DateOnly.Today)).Return(domainData);
			mapper.Stub(x => x.Map<PreferenceDomainData, PreferenceViewModel>(domainData)).Return(viewModel);

			var result = target.CreateViewModel(DateOnly.Today);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldCreateFeedbackDayViewModelByMapping()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new PreferenceViewModelFactory(mapper);
			var viewModel = new PreferenceDayFeedbackViewModel();

			mapper.Stub(x => x.Map<DateOnly, PreferenceDayFeedbackViewModel>(DateOnly.Today)).Return(viewModel);

			var result = target.CreateDayFeedbackViewModel(DateOnly.Today);

			result.Should().Be.SameInstanceAs(viewModel);
		}

	}

	[TestFixture]
	public class PreferencePeriodViewModelFactoryTest
	{

		[Test]
		public void ShouldGetViewModelWithPossibleResultDaysOff()
		{
			var preferencePeriodFeedbackProvider = MockRepository.GenerateMock<IPreferencePeriodFeedbackProvider>();
			preferencePeriodFeedbackProvider.Stub(x => x.PossibleResultDaysOff(DateOnly.Today)).Return(8);
			var target = new PreferencePeriodViewModelFactory(preferencePeriodFeedbackProvider);

			var result = target.CreatePeriodFeedbackViewModel(DateOnly.Today);

			result.PossibleResultDaysOff.Should().Be.EqualTo(8);
		}

		[Test]
		public void ShouldGetViewModelWithLowerTargetDaysOff()
		{
			var preferencePeriodFeedbackProvider = MockRepository.GenerateMock<IPreferencePeriodFeedbackProvider>();
			preferencePeriodFeedbackProvider.Stub(x => x.TargetDaysOff(DateOnly.Today)).Return(new MinMax<int>(3, 4));
			var target = new PreferencePeriodViewModelFactory(preferencePeriodFeedbackProvider);

			var result = target.CreatePeriodFeedbackViewModel(DateOnly.Today);

			result.TargetDaysOff.Lower.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldGetViewModelWithUpperTargetDaysOff()
		{
			var preferencePeriodFeedbackProvider = MockRepository.GenerateMock<IPreferencePeriodFeedbackProvider>();
			preferencePeriodFeedbackProvider.Stub(x => x.TargetDaysOff(DateOnly.Today)).Return(new MinMax<int>(3, 4));
			var target = new PreferencePeriodViewModelFactory(preferencePeriodFeedbackProvider);

			var result = target.CreatePeriodFeedbackViewModel(DateOnly.Today);

			result.TargetDaysOff.Upper.Should().Be.EqualTo(4);
		}


	}
}
