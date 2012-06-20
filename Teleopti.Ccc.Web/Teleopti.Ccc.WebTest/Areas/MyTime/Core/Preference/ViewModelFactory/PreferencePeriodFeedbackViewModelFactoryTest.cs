using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Core.RequestContext;
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
			var target = new PreferencePeriodFeedbackViewModelFactory(preferencePeriodFeedbackProvider, MockRepository.GenerateMock<ITimeFormatter>());

			var result = target.CreatePeriodFeedbackViewModel(DateOnly.Today);

			result.PossibleResultDaysOff.Should().Be.EqualTo(8);
		}

		[Test]
		public void ShouldGetViewModelWithLowerTargetDaysOff()
		{
			var preferencePeriodFeedbackProvider = MockRepository.GenerateMock<IPreferencePeriodFeedbackProvider>();
			preferencePeriodFeedbackProvider.Stub(x => x.PeriodFeedback(DateOnly.Today)).Return(new PeriodFeedback {TargetDaysOff = new MinMax<int>(3, 4)});
			var target = new PreferencePeriodFeedbackViewModelFactory(preferencePeriodFeedbackProvider, MockRepository.GenerateMock<ITimeFormatter>());

			var result = target.CreatePeriodFeedbackViewModel(DateOnly.Today);

			result.TargetDaysOff.Lower.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldGetViewModelWithUpperTargetDaysOff()
		{
			var preferencePeriodFeedbackProvider = MockRepository.GenerateMock<IPreferencePeriodFeedbackProvider>();
			preferencePeriodFeedbackProvider.Stub(x => x.PeriodFeedback(DateOnly.Today)).Return(new PeriodFeedback {TargetDaysOff = new MinMax<int>(3, 4)});
			var target = new PreferencePeriodFeedbackViewModelFactory(preferencePeriodFeedbackProvider, MockRepository.GenerateMock<ITimeFormatter>());

			var result = target.CreatePeriodFeedbackViewModel(DateOnly.Today);

			result.TargetDaysOff.Upper.Should().Be.EqualTo(4);
		}

		[Test]
		public void ShouldGetViewModelWithTargetContractTimeLower()
		{
			var preferencePeriodFeedbackProvider = MockRepository.GenerateMock<IPreferencePeriodFeedbackProvider>();
			preferencePeriodFeedbackProvider.Stub(x => x.PeriodFeedback(DateOnly.Today))
				.Return(new PeriodFeedback {TargetTime = new MinMax<TimeSpan>(TimeSpan.FromDays(2), TimeSpan.FromDays(5))});
			var timeFormatter = MockRepository.GenerateMock<ITimeFormatter>();
			timeFormatter.Stub(x => x.GetLongHourMinuteTimeString(TimeSpan.FromDays(2))).Return("48:00");
			var target = new PreferencePeriodFeedbackViewModelFactory(preferencePeriodFeedbackProvider, timeFormatter);

			var result = target.CreatePeriodFeedbackViewModel(DateOnly.Today);

			result.TargetContractTime.Lower.Should().Be.EqualTo("48:00");
		}

		[Test]
		public void ShouldGetViewModelWithTargetContractTimeUpper()
		{
			var preferencePeriodFeedbackProvider = MockRepository.GenerateMock<IPreferencePeriodFeedbackProvider>();
			preferencePeriodFeedbackProvider.Stub(x => x.PeriodFeedback(DateOnly.Today))
				.Return(new PeriodFeedback {TargetTime = new MinMax<TimeSpan>(TimeSpan.FromDays(2), TimeSpan.FromDays(5))});
			var timeFormatter = MockRepository.GenerateMock<ITimeFormatter>();
			timeFormatter.Stub(x => x.GetLongHourMinuteTimeString(TimeSpan.FromDays(5))).Return("60:00");
			var target = new PreferencePeriodFeedbackViewModelFactory(preferencePeriodFeedbackProvider, timeFormatter);

			var result = target.CreatePeriodFeedbackViewModel(DateOnly.Today);

			result.TargetContractTime.Upper.Should().Be.EqualTo("60:00");
		}

	}
}