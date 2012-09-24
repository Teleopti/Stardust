using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.TestHelper.Fakes;
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

			virtualSchedulePeriodProvider.Stub(x => x.MissingSchedulePeriod()).Return(true);

			var result = target.Index(null);

			result.ViewName.Should().Be.EqualTo("NoSchedulePeriodPartial");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnNoPersonPeriodPartialWhenNoPersonPeriod()
		{
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var viewModelFactory = MockRepository.GenerateMock<IPreferenceViewModelFactory>();
			var target = new PreferenceController(viewModelFactory, virtualSchedulePeriodProvider, null);

			virtualSchedulePeriodProvider.Stub(x => x.MissingPersonPeriod()).Return(true);

			var result = target.Index(null);

			result.ViewName.Should().Be.EqualTo("NoPersonPeriodPartial");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldGetPreference()
		{
			var viewModelFactory = MockRepository.GenerateMock<IPreferenceViewModelFactory>();
			var target = new PreferenceController(viewModelFactory, null, null);

			viewModelFactory.Stub(x => x.CreateDayViewModel(DateOnly.Today)).Return(new PreferenceDayViewModel());

			var result = target.GetPreference(DateOnly.Today);
			var model = result.Data as PreferenceDayViewModel;

			model.Should().Not.Be.Null();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldGetNoContentInResponseWhenNoPreferenceExists()
		{
			var viewModelFactory = MockRepository.GenerateMock<IPreferenceViewModelFactory>();
			var target = new PreferenceController(viewModelFactory, null, null);
			target.ControllerContext = new ControllerContext(new FakeHttpContext("/"), new RouteData(), target);

			viewModelFactory.Stub(x => x.CreateDayViewModel(DateOnly.Today)).Return(null);

			target.GetPreference(DateOnly.Today).Should().Be.Null();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldPersistPreferenceInput()
		{
			var preferencePersister = MockRepository.GenerateMock<IPreferencePersister>();
			var input = new PreferenceDayInput();
			var resultData = new PreferenceDayViewModel();

			var target = new PreferenceController(null, null, preferencePersister);

			preferencePersister.Stub(x => x.Persist(input)).Return(resultData);

			var result = target.Preference(input);
			var data = result.Data as PreferenceDayViewModel;

			data.Should().Be.SameInstanceAs(resultData);
		}

		[Test]
		public void ShouldPersistPreferenceMustHave()
		{
			var preferencePersister = MockRepository.GenerateMock<IPreferencePersister>();
			var input = new MustHaveInput();
			var resultData = new PreferenceDayViewModel();

			var target = new PreferenceController(null, null, preferencePersister);

			preferencePersister.Stub(x => x.MustHave(input)).Return(resultData);

			var result = target.MustHave(input);
			var data = result.Data as PreferenceDayViewModel;

			data.Should().Be.SameInstanceAs(resultData);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldDeletePreference()
		{
			var preferencePersister = MockRepository.GenerateMock<IPreferencePersister>();
			var target = new PreferenceController(null, null, preferencePersister);
			var resultData = new PreferenceDayViewModel();

			preferencePersister.Stub(x => x.Delete(DateOnly.Today)).Return(resultData);

			var result = target.PreferenceDelete(DateOnly.Today);
			var data = result.Data as PreferenceDayViewModel;

			data.Should().Be.SameInstanceAs(resultData);
		}

		[Test]
		public void ShouldNotPersistDayWithMustHaveOverLimit()
		{

			var preferencePersister = MockRepository.GenerateMock<IPreferencePersister>();
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var target = new PreferenceController(null, virtualSchedulePeriodProvider, preferencePersister);
			var period = new DateOnlyPeriod();

			virtualSchedulePeriodProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today)).Return(period);
			preferencePersister.Expect(x => x.TryToggleMustHave(DateOnly.Today, true, period)).Return(false);

			var result = target.ToggleMustHave(DateOnly.Today, true);

			result.Should().Be.False();

			preferencePersister.VerifyAllExpectations();
			
		}

		[Test]
		public void ShouldPersistDayWithMustHaveUnderLimit()
		{

			var preferencePersister = MockRepository.GenerateMock<IPreferencePersister>();
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var target = new PreferenceController(null, virtualSchedulePeriodProvider, preferencePersister);
			var period = new DateOnlyPeriod();

			virtualSchedulePeriodProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today)).Return(period);
			preferencePersister.Expect(x => x.TryToggleMustHave(DateOnly.Today, true, period)).Return(true);

			var result = target.ToggleMustHave(DateOnly.Today, true);

			result.Should().Be.True();

			preferencePersister.VerifyAllExpectations();
		}
	}
}