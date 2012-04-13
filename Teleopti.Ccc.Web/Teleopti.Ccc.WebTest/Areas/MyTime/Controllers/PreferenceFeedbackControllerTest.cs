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
		public void ShouldReturnFeedbackAsync()
		{
			var viewModelFactory = MockRepository.GenerateMock<IPreferenceViewModelFactory>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var model = new PreferenceDayFeedbackViewModel();
			var target = new PreferenceFeedbackController(viewModelFactory, unitOfWorkFactory);
			viewModelFactory.Stub(x => x.CreateDayFeedbackViewModel(DateOnly.Today)).Return(model);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());

			target.FeedbackAsync(DateOnly.Today).Wait();
			var result = target.FeedbackCompleted(target.AsyncManager.Parameters["model"] as PreferenceDayFeedbackViewModel, null);

			result.Data.Should().Be(model);
		}
	}
}