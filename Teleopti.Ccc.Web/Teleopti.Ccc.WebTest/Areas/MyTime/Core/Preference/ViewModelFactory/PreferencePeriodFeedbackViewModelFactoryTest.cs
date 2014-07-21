using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.ViewModelFactory
{
	[TestFixture]
	public class PreferencePeriodFeedbackViewModelFactoryTest
	{

		[Test]
		public void ShouldGetViewModelWithPossibleResultDaysOff()
		{
			var preferencePeriodFeedbackProvider = MockRepository.GenerateMock<IPreferencePeriodFeedbackProvider>();
			preferencePeriodFeedbackProvider.Stub(x => x.PeriodFeedback(DateOnly.Today)).Return(new PeriodFeedback {PossibleResultDaysOff = 8});
			var target = new PreferencePeriodFeedbackViewModelFactory(preferencePeriodFeedbackProvider);

			var result = target.CreatePeriodFeedbackViewModel(DateOnly.Today);

			result.PossibleResultDaysOff.Should().Be.EqualTo(8);
		}

		[Test]
		public void ShouldGetViewModelWithLowerTargetDaysOff()
		{
			var preferencePeriodFeedbackProvider = MockRepository.GenerateMock<IPreferencePeriodFeedbackProvider>();
			preferencePeriodFeedbackProvider.Stub(x => x.PeriodFeedback(DateOnly.Today)).Return(new PeriodFeedback {TargetDaysOff = new MinMax<int>(3, 4)});
			var target = new PreferencePeriodFeedbackViewModelFactory(preferencePeriodFeedbackProvider);

			var result = target.CreatePeriodFeedbackViewModel(DateOnly.Today);

			result.TargetDaysOff.Lower.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldGetViewModelWithUpperTargetDaysOff()
		{
			var preferencePeriodFeedbackProvider = MockRepository.GenerateMock<IPreferencePeriodFeedbackProvider>();
			preferencePeriodFeedbackProvider.Stub(x => x.PeriodFeedback(DateOnly.Today)).Return(new PeriodFeedback {TargetDaysOff = new MinMax<int>(3, 4)});
			var target = new PreferencePeriodFeedbackViewModelFactory(preferencePeriodFeedbackProvider);

			var result = target.CreatePeriodFeedbackViewModel(DateOnly.Today);

			result.TargetDaysOff.Upper.Should().Be.EqualTo(4);
		}

		[Test]
		public void ShouldGetViewModelWithTargetContractTimeLowerMinutes()
		{
			var preferencePeriodFeedbackProvider = MockRepository.GenerateMock<IPreferencePeriodFeedbackProvider>();
			preferencePeriodFeedbackProvider.Stub(x => x.PeriodFeedback(DateOnly.Today))
				.Return(new PeriodFeedback { TargetTime = new MinMax<TimeSpan>(TimeSpan.FromDays(2), TimeSpan.FromDays(5)) });
			var target = new PreferencePeriodFeedbackViewModelFactory(preferencePeriodFeedbackProvider);

			var result = target.CreatePeriodFeedbackViewModel(DateOnly.Today);

			result.TargetContractTime.LowerMinutes.Should().Be.EqualTo(2880);
		}

		[Test]
		public void ShouldGetViewModelWithTargetContractTimeUpperMinutes()
		{
			var preferencePeriodFeedbackProvider = MockRepository.GenerateMock<IPreferencePeriodFeedbackProvider>();
			preferencePeriodFeedbackProvider.Stub(x => x.PeriodFeedback(DateOnly.Today))
				.Return(new PeriodFeedback { TargetTime = new MinMax<TimeSpan>(TimeSpan.FromDays(2), TimeSpan.FromDays(5)) });
			var target = new PreferencePeriodFeedbackViewModelFactory(preferencePeriodFeedbackProvider);

			var result = target.CreatePeriodFeedbackViewModel(DateOnly.Today);

			result.TargetContractTime.UpperMinutes.Should().Be.EqualTo(7200);
		}
	}
}