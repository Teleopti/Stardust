using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class PreferenceFeedbackControllerTest
	{
		[Test]
		public void ShouldReturnFeedback()
		{
			var viewModelFactory = MockRepository.GenerateMock<IPreferenceViewModelFactory>();
			var model = new PreferenceDayFeedbackViewModel();
			var target = new PreferenceFeedbackController(viewModelFactory);
			viewModelFactory.Stub(x => x.CreateDayFeedbackViewModel(DateOnly.Today)).Return(model);

			target.FeedbackTask(DateOnly.Today);
			var result = target.FeedbackCompleted(target.AsyncManager.Parameters["model"] as PreferenceDayFeedbackViewModel, null);

			result.Data.Should().Be(model);
		}
	}
}