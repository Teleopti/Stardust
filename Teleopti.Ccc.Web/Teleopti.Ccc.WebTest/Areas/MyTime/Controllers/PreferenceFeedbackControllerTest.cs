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
		public void ShouldReturnDaysOff()
		{
			var preferencePeriodFeedbackProvider = MockRepository.GenerateMock<IPreferencePeriodFeedbackProvider>();
			var target = new PreferenceFeedbackController(null, preferencePeriodFeedbackProvider);
			var daysOff = new DaysOffViewModel();
			var date = DateOnly.Today.AddDays(new Random().Next(0, 1000));

			preferencePeriodFeedbackProvider.Stub(x => x.ShouldHaveDaysOff(date)).Return(daysOff);

			target.ShouldHaveDaysOffTask(date);
			var result = target.ShouldHaveDaysOffCompleted(
				target.AsyncManager.Parameters["daysOff"] as DaysOffViewModel,
				Task.Factory.StartNew(() => { })
				);

			result.Data.Should().Be(daysOff);
		}
	}
}