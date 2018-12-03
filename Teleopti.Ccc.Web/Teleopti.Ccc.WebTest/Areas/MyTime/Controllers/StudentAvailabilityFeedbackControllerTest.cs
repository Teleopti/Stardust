using System.Threading.Tasks;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class StudentAvailabilityFeedbackControllerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnFeedback()
		{
			var viewModelFactory = MockRepository.GenerateMock<IStudentAvailabilityViewModelFactory>();
			var model = new StudentAvailabilityDayFeedbackViewModel();
			var target = new StudentAvailabilityFeedbackController(viewModelFactory, null);
			viewModelFactory.Stub(x => x.CreateDayFeedbackViewModel(DateOnly.Today)).Return(model);

			target.FeedbackTask(DateOnly.Today);
			var result = target.FeedbackCompleted(
				target.AsyncManager.Parameters["model"] as StudentAvailabilityDayFeedbackViewModel,
				Task.FromResult(false)
				);

			result.Data.Should().Be(model);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnPeriodFeedback()
		{
			var studentAvailabilityPeriodViewModelFactory =
				MockRepository.GenerateMock<IStudentAvailabilityPeriodFeedbackViewModelFactory>();
			var target = new StudentAvailabilityFeedbackController(null, studentAvailabilityPeriodViewModelFactory);
			var model = new StudentAvailabilityPeriodFeedbackViewModel();

			studentAvailabilityPeriodViewModelFactory.Stub(x => x.CreatePeriodFeedbackViewModel(DateOnly.Today)).Return(model);

			target.PeriodFeedbackTask(DateOnly.Today);
			var result = target.PeriodFeedbackCompleted(
				target.AsyncManager.Parameters["model"] as StudentAvailabilityPeriodFeedbackViewModel,
				Task.FromResult(false)
				);

			result.Data.Should().Be(model);
		}
	}
}