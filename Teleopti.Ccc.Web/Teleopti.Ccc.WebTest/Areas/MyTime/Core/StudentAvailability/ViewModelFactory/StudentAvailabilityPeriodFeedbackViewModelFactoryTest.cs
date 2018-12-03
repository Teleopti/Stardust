using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.StudentAvailability.ViewModelFactory
{
	[TestFixture]
	public class StudentAvailabilityPeriodFeedbackViewModelFactoryTest
	{
		[Test]
		public void ShouldGetViewModelWithTargetContractTimeLowerMinutes()
		{
			var studentAvailabilityPeriodFeedbackProvider =
				MockRepository.GenerateMock<IStudentAvailabilityPeriodFeedbackProvider>();
			studentAvailabilityPeriodFeedbackProvider.Stub(x => x.PeriodFeedback(DateOnly.Today))
				.Return(new PeriodFeedback {TargetTime = new TimePeriod(TimeSpan.FromDays(2), TimeSpan.FromDays(5))});
			var target = new StudentAvailabilityPeriodFeedbackViewModelFactory(studentAvailabilityPeriodFeedbackProvider);

			var result = target.CreatePeriodFeedbackViewModel(DateOnly.Today);

			result.TargetContractTime.LowerMinutes.Should().Be.EqualTo(2880);
		}

		[Test]
		public void ShouldGetViewModelWithTargetContractTimeUpperMinutes()
		{
			var studentAvailabilityPeriodFeedbackProvider =
				MockRepository.GenerateMock<IStudentAvailabilityPeriodFeedbackProvider>();
			studentAvailabilityPeriodFeedbackProvider.Stub(x => x.PeriodFeedback(DateOnly.Today))
				.Return(new PeriodFeedback {TargetTime = new TimePeriod(TimeSpan.FromDays(2), TimeSpan.FromDays(5))});
			var target = new StudentAvailabilityPeriodFeedbackViewModelFactory(studentAvailabilityPeriodFeedbackProvider);

			var result = target.CreatePeriodFeedbackViewModel(DateOnly.Today);

			result.TargetContractTime.UpperMinutes.Should().Be.EqualTo(7200);
		}
	}
}