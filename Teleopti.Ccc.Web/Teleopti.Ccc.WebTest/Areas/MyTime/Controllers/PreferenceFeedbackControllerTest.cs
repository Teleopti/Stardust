using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class PreferenceFeedbackControllerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnFeedback()
		{
			var viewModelFactory = MockRepository.GenerateMock<IPreferenceViewModelFactory>();
			var model = new PreferenceDayFeedbackViewModel();
			var target = new PreferenceFeedbackController(viewModelFactory, null);
			viewModelFactory.Stub(x => x.CreateDayFeedbackViewModel(DateOnly.Today)).Return(model);

			target.FeedbackTask(DateOnly.Today);
			var result = target.FeedbackCompleted(
				target.AsyncManager.Parameters["model"] as PreferenceDayFeedbackViewModel,
				Task.Factory.StartNew(() => { })
				);

			result.Data.Should().Be(model);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnPeriodFeedback()
		{
			var preferencePeriodViewModelFactory = MockRepository.GenerateMock<IPreferencePeriodViewModelFactory>();
			var target = new PreferenceFeedbackController(null, preferencePeriodViewModelFactory);
			var model = new PreferencePeriodFeedbackViewModel();

			preferencePeriodViewModelFactory.Stub(x => x.CreatePeriodFeedbackViewModel(DateOnly.Today)).Return(model);

			target.PeriodFeedbackTask(DateOnly.Today);
			var result = target.PeriodFeedbackCompleted(
				target.AsyncManager.Parameters["model"] as PreferencePeriodFeedbackViewModel,
				Task.Factory.StartNew(() => { })
				);

			result.Data.Should().Be(model);
		}

	}
}