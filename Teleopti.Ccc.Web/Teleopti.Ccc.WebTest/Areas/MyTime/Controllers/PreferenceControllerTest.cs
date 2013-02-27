using System.Collections.Generic;
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
using Teleopti.Ccc.Web.Core;
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
			var target = new PreferenceController(viewModelFactory, virtualSchedulePeriodProvider, null, null);

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
			var target = new PreferenceController(viewModelFactory, virtualSchedulePeriodProvider, null, null);
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
			var target = new PreferenceController(viewModelFactory, virtualSchedulePeriodProvider, null, null);

			virtualSchedulePeriodProvider.Stub(x => x.MissingSchedulePeriod()).Return(true);

			var result = target.Index(null);

			result.ViewName.Should().Be.EqualTo("NoSchedulePeriodPartial");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnNoPersonPeriodPartialWhenNoPersonPeriod()
		{
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var viewModelFactory = MockRepository.GenerateMock<IPreferenceViewModelFactory>();
			var target = new PreferenceController(viewModelFactory, virtualSchedulePeriodProvider, null, null);

			virtualSchedulePeriodProvider.Stub(x => x.MissingPersonPeriod()).Return(true);

			var result = target.Index(null);

			result.ViewName.Should().Be.EqualTo("NoPersonPeriodPartial");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldGetPreference()
		{
			var viewModelFactory = MockRepository.GenerateMock<IPreferenceViewModelFactory>();
			var target = new PreferenceController(viewModelFactory, null, null, null);

			viewModelFactory.Stub(x => x.CreateDayViewModel(DateOnly.Today)).Return(new PreferenceDayViewModel());

			var result = target.GetPreference(DateOnly.Today);
			var model = result.Data as PreferenceDayViewModel;

			model.Should().Not.Be.Null();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldGetNoContentInResponseWhenNoPreferenceExists()
		{
			var viewModelFactory = MockRepository.GenerateMock<IPreferenceViewModelFactory>();
			var target = new PreferenceController(viewModelFactory, null, null, null);
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

			var target = new PreferenceController(null, null, preferencePersister, null);

			preferencePersister.Stub(x => x.Persist(input)).Return(resultData);

			var result = target.Preference(input);
			var data = result.Data as PreferenceDayViewModel;

			data.Should().Be.SameInstanceAs(resultData);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldHandleModelErrorInPersistPreferenceInput()
		{
			var preferencePersister = MockRepository.GenerateMock<IPreferencePersister>();
			var response = MockRepository.GenerateStub<FakeHttpResponse>();
			var input = new PreferenceDayInput();

			var target = new PreferenceController(null, null, preferencePersister, null);
			var context = new FakeHttpContext("/");
			context.SetResponse(response);
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);
			target.ModelState.AddModelError("Error", "Error");

			var result = target.Preference(input);
			var data = result.Data as ModelStateResult;
			data.Errors.Should().Contain("Error");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldDeletePreference()
		{
			var preferencePersister = MockRepository.GenerateMock<IPreferencePersister>();
			var target = new PreferenceController(null, null, preferencePersister, null);
			var resultData = new PreferenceDayViewModel();

			preferencePersister.Stub(x => x.Delete(DateOnly.Today)).Return(resultData);

			var result = target.PreferenceDelete(DateOnly.Today);
			var data = result.Data as PreferenceDayViewModel;

			data.Should().Be.SameInstanceAs(resultData);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldPersistPreferenceMustHave()
		{
			var preferencePersister = MockRepository.GenerateMock<IPreferencePersister>();
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var target = new PreferenceController(null, virtualSchedulePeriodProvider, preferencePersister, null);
			var period = new DateOnlyPeriod();
			var input = new MustHaveInput();

			virtualSchedulePeriodProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today)).Return(period);
			preferencePersister.Stub(x => x.MustHave(input)).Return(true);

			var result = target.MustHave(input);
			result.Data.Should().Be.EqualTo(true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldGetPreferencesAndSchedules()
		{
			var viewModelFactory = MockRepository.GenerateMock<IPreferenceViewModelFactory>();
			var target = new PreferenceController(viewModelFactory, null, null, null);
			var viewModels = new PreferenceAndScheduleDayViewModel[] {};

			viewModelFactory.Stub(x => x.CreatePreferencesAndSchedulesViewModel(DateOnly.Today, DateOnly.Today.AddDays(1))).Return(viewModels);

			var result = target.PreferencesAndSchedules(DateOnly.Today, DateOnly.Today.AddDays(1));

			result.Data.Should().Be(viewModels);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldGetPreferenceTemplates()
		{
			var preferenceTemplateProvider = MockRepository.GenerateMock<IPreferenceTemplatesProvider>();
			var target = new PreferenceController(null, null, null, preferenceTemplateProvider);

			var templates = new List<IExtendedPreferenceTemplate>();
			preferenceTemplateProvider.Stub(x => x.RetrievePreferenceTemplates()).Return(templates);

			var result = target.GetPreferenceTemplates();

			result.Data.Should().Be(templates);
		}
	}
}