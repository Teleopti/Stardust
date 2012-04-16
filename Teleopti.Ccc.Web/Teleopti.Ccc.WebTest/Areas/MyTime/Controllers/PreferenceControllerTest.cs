using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class PreferenceControllerTest
	{

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnPreferencePartialView()
		{
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var viewModelFactory = MockRepository.GenerateMock<IPreferenceViewModelFactory>();
			var target = new PreferenceController(viewModelFactory, virtualSchedulePeriodProvider, null);

			virtualSchedulePeriodProvider.Stub(x => x.HasSchedulePeriod()).Return(true);
			viewModelFactory.Stub(x => x.CreateViewModel(DateOnly.Today)).Return(new PreferenceViewModel());

			var result = target.Index(DateOnly.Today);
			var model = result.Model as PreferenceViewModel;

			result.ViewName.Should().Be.EqualTo("PreferencePartial");
			model.Should().Not.Be.Null();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldUseDefaultDateWhenNotSpecified()
		{
			var viewModelFactory = MockRepository.GenerateMock<IPreferenceViewModelFactory>();
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var target = new PreferenceController(viewModelFactory, virtualSchedulePeriodProvider, null);
			var defaultDate = DateOnly.Today.AddDays(23);

			virtualSchedulePeriodProvider.Stub(x => x.HasSchedulePeriod()).Return(true);
			virtualSchedulePeriodProvider.Stub(x => x.CalculatePreferenceDefaultDate()).Return(defaultDate);

			target.Index(null);

			viewModelFactory.AssertWasCalled(x => x.CreateViewModel(defaultDate));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnNoSchedulePeriodPartialWhenNoSchedulePeriod()
		{
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var viewModelFactory = MockRepository.GenerateMock<IPreferenceViewModelFactory>();
			var target = new PreferenceController(viewModelFactory, virtualSchedulePeriodProvider, null);

			virtualSchedulePeriodProvider.Stub(x => x.HasSchedulePeriod()).Return(false);

			var result = target.Index(null);

			result.ViewName.Should().Be.EqualTo("NoSchedulePeriodPartial");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldPersistPreferenceInput()
		{
			var preferencePersister = MockRepository.GenerateMock<IPreferencePersister>();
			var input = new PreferenceDayInput();
			var resultData = new PreferenceDayInputResult();

			var target = new PreferenceController(null, null, preferencePersister);

			preferencePersister.Stub(x => x.Persist(input)).Return(resultData);

			var result = target.Preference(input);
			var data = result.Data as PreferenceDayInputResult;

			data.Should().Be.SameInstanceAs(resultData);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldDeletePreference()
		{
			var preferencePersister = MockRepository.GenerateMock<IPreferencePersister>();
			var target = new PreferenceController(null, null, preferencePersister);
			var resultData = new PreferenceDayInputResult();

			preferencePersister.Stub(x => x.Delete(DateOnly.Today)).Return(resultData);

			var result = target.PreferenceDelete(DateOnly.Today);
			var data = result.Data as PreferenceDayInputResult;

			data.Should().Be.SameInstanceAs(resultData);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldGetFeedback()
		{
			var viewModelFactory = MockRepository.GenerateMock<IPreferenceViewModelFactory>();
			var target = new PreferenceController(viewModelFactory, null, null);
			var resultData = new PreferenceDayFeedbackViewModel();

			viewModelFactory.Stub(x => x.CreateDayFeedbackViewModel(DateOnly.Today)).Return(resultData);

			var result = target.Feedback(DateOnly.Today);
			var data = result.Data as PreferenceDayFeedbackViewModel;

			data.Should().Be.SameInstanceAs(resultData);
		}
	}
}