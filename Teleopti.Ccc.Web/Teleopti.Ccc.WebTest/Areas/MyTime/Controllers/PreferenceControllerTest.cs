using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[DomainTest]
	[WebTest]
	public class PreferenceControllerTest : IIsolateSystem
	{
		public PreferenceController Target;
		public FakeBankHolidayCalendarSiteRepository BankHolidayCalendarSiteRepository;
		public FakeScheduleProvider ScheduleProvider;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakePersonRepository PersonRepository;
		public FakeBankHolidayCalendarRepository BankHolidayCalendarRepository;
		public FakeBankHolidayDateRepository BankHolidayDateRepository;
		private ISite _site;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeBankHolidayCalendarSiteRepository>().For<IBankHolidayCalendarSiteRepository>();
			isolate.UseTestDouble<FakeBankHolidayCalendarRepository>().For<IBankHolidayCalendarRepository>();
			isolate.UseTestDouble<FakeBankHolidayDateRepository>().For<IBankHolidayDateRepository>();
			isolate.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			isolate.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			isolate.UseTestDouble<FakeScheduleProvider>().For<IScheduleProvider>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}

		private void prepareData(DateOnly date)
		{
			var logonUser = PersonFactory.CreatePersonWithGuid("logon", "user");
			LoggedOnUser.SetFakeLoggedOnUser(logonUser);
			PersonRepository.Add(logonUser);
			_site = new Site("site").WithId();
			SiteRepository.Add(_site);
			var team = new Team { Site = _site }.WithDescription(new Description("team")).WithId();
			TeamRepository.Add(team);
			logonUser.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, team));

		}

		[Test]
		public void ShouldGetBankHolidayCalendar()
		{
			var date = DateOnly.Today;
			prepareData(date);

			var schedule = ScheduleDayFactory.Create(date, LoggedOnUser.CurrentUser());
			ScheduleProvider.AddScheduleDay(schedule);
			var calendarId = Guid.NewGuid();
			var calendarName = "ChinaBankHoliday";
			var calendar = new BankHolidayCalendar { Name = calendarName };
			calendar.SetId(calendarId);
			BankHolidayCalendarRepository.Add(calendar);
			var description = "New Year";
			var calendarDate = new BankHolidayDate { Calendar = calendar, Date = date, Description = description };
			BankHolidayDateRepository.Add(calendarDate);
			BankHolidayCalendarSiteRepository.Add(new BankHolidayCalendarSite { Site = _site, Calendar = calendar });

			var result = Target.PreferencesAndSchedules(date.AddDays(-1), date.AddDays(1)).Data as IEnumerable<PreferenceAndScheduleDayViewModel>;
			result.First().BankHolidayCalendar.CalendarId.Should().Be.EqualTo(calendarId);
			result.First().BankHolidayCalendar.CalendarName.Should().Be.EqualTo(calendarName);
			result.First().BankHolidayCalendar.DateDescription.Should().Be.EqualTo(description);
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnPreferencePartialView()
		{
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var viewModelFactory = MockRepository.GenerateMock<IPreferenceViewModelFactory>();
			var target = new PreferenceController(viewModelFactory, virtualSchedulePeriodProvider, null, null);

			viewModelFactory.Stub(x => x.CreateViewModel(DateOnly.Today)).Return(new PreferenceViewModel()).IgnoreArguments();

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

			virtualSchedulePeriodProvider.Stub(x => x.MissingPersonPeriod(new DateOnly())).Return(true);

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
			var viewModels = new PreferenceAndScheduleDayViewModel[] { };

			viewModelFactory.Stub(x => x.CreatePreferencesAndSchedulesViewModel(DateOnly.Today, DateOnly.Today.AddDays(1))).Return(viewModels);

			var result = target.PreferencesAndSchedules(DateOnly.Today, DateOnly.Today.AddDays(1));

			result.Data.Should().Be(viewModels);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldGetPreferenceTemplates()
		{
			var viewModelFactory = MockRepository.GenerateMock<IPreferenceViewModelFactory>();
			var target = new PreferenceController(viewModelFactory, null, null, null);
			var templates = new List<PreferenceTemplateViewModel>();
			viewModelFactory.Stub(x => x.CreatePreferenceTemplateViewModels()).Return(templates);

			var result = target.GetPreferenceTemplates();

			result.Data.Should().Be(templates);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldPersistPreferenceTemplate()
		{
			var preferenceTemplatePersister = MockRepository.GenerateMock<IPreferenceTemplatePersister>();
			var target = new PreferenceController(null, null, null, preferenceTemplatePersister);
			var template = new PreferenceTemplateViewModel();
			var input = new PreferenceTemplateInput();
			preferenceTemplatePersister.Stub(x => x.Persist(input)).Return(template);

			var result = target.PreferenceTemplate(input);

			result.Data.Should().Be(template);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldDeletePreferenceTemplate()
		{
			var preferenceTemplatePersister = MockRepository.GenerateMock<IPreferenceTemplatePersister>();
			var target = new PreferenceController(null, null, null, preferenceTemplatePersister);
			var id = Guid.NewGuid();

			target.PreferenceTemplateDelete(id);

			preferenceTemplatePersister.AssertWasCalled(x => x.Delete(id));

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldCheckPreferenceValidation()
		{
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var viewModelFactory = MockRepository.GenerateMock<IPreferenceViewModelFactory>();
			var target = new PreferenceController(viewModelFactory, virtualSchedulePeriodProvider, null, null);

			var shiftCategoryId = Guid.NewGuid();
			var preferenceOptions = new[]
			{
				new PreferenceOption{Value = shiftCategoryId.ToString()}
			};
			var preferenceOptionsGroup = new PreferenceOptionGroup("ShiftCategory", preferenceOptions);

			var activityOptions = new[]
			{
				new PreferenceOption{Value = Guid.NewGuid().ToString()}
			};
			var activityOptionsGroup = new PreferenceOptionGroup("Activity", activityOptions);

			var preferenceViewModel = new PreferenceViewModel
			{
				Options = new PreferenceOptionsViewModel(new[] { preferenceOptionsGroup }, activityOptionsGroup)
			};

			viewModelFactory.Stub(x => x.CreateViewModel(DateOnly.Today)).Return(preferenceViewModel);

			var result = target.CheckPreferenceValidation(DateOnly.Today, shiftCategoryId);
			dynamic data = result.Data;
			Assert.AreEqual(data.isValid, true);
			Assert.AreEqual(data.message, string.Empty);

			result = target.CheckPreferenceValidation(DateOnly.Today, Guid.NewGuid());
			data = result.Data;
			Assert.AreEqual(data.isValid, false);
			Assert.AreEqual(data.message, Resources.CannotAddPreferenceSelectedItemNotAvailable);
		}
	}
}