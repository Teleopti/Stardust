using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class StudentAvailabilityControllerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnStudentAvailabilityPartialView()
		{
			var viewModelFactory = MockRepository.GenerateMock<IStudentAvailabilityViewModelFactory>();
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var target = new AvailabilityController(viewModelFactory, virtualSchedulePeriodProvider, null);

			viewModelFactory.Stub(x => x.CreateViewModel(DateOnly.Today)).Return(new StudentAvailabilityViewModel());

			var result = target.Index(DateOnly.Today) as ViewResult;
			var model = result.Model as StudentAvailabilityViewModel;

			result.ViewName.Should().Be.EqualTo("StudentAvailabilityPartial");
			model.Should().Not.Be.Null();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldUseDefaultDateWhenNotSpecified()
		{
			var viewModelFactory = MockRepository.GenerateMock<IStudentAvailabilityViewModelFactory>();
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var target = new AvailabilityController(viewModelFactory, virtualSchedulePeriodProvider, null);
			var defaultDate = DateOnly.Today.AddDays(23);

			virtualSchedulePeriodProvider.Stub(x => x.CalculateStudentAvailabilityDefaultDate()).Return(defaultDate);

			target.Index(null);

			viewModelFactory.AssertWasCalled(x => x.CreateViewModel(defaultDate));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnNoSchedulePeriodPartialWhenNoSchedulePeriod()
		{
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var target = new AvailabilityController(null, virtualSchedulePeriodProvider, null);

			virtualSchedulePeriodProvider.Stub(x => x.MissingSchedulePeriod()).Return(true);

			var result = target.Index(null) as ViewResult;

			result.ViewName.Should().Be.EqualTo("NoSchedulePeriodPartial");
		}

        	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnNoPersonPeriodPartialWhenNoPersonPeriod()
		{
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var viewModelFactory = MockRepository.GenerateMock<IStudentAvailabilityViewModelFactory>();
			var target = new AvailabilityController(viewModelFactory, virtualSchedulePeriodProvider, null);

			virtualSchedulePeriodProvider.Stub(x => x.MissingPersonPeriod(new DateOnly())).Return(true);

            var result = target.Index(null) as ViewResult;

			result.ViewName.Should().Be.EqualTo("NoPersonPeriodPartial");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnStudentAvailabilitiesAndSchedules()
		{
			var viewModelFactory = MockRepository.GenerateMock<IStudentAvailabilityViewModelFactory>();
			var target = new AvailabilityController(viewModelFactory, null, null);
			var list = new List<StudentAvailabilityDayViewModel>();

			viewModelFactory.Stub(x => x.CreateStudentAvailabilityAndSchedulesViewModels(DateOnly.Today, DateOnly.Today.AddDays(1))).Return(list);

			var result = target.StudentAvailabilitiesAndSchedules(DateOnly.Today, DateOnly.Today.AddDays(1));

			result.Data.Should().Be.SameInstanceAs(list);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnStudentAvailabilityForDate()
		{
			var viewModelFactory = MockRepository.GenerateMock<IStudentAvailabilityViewModelFactory>();

			var target = new AvailabilityController(viewModelFactory, null, null);
			var model = new StudentAvailabilityDayViewModel();

			viewModelFactory.Stub(x => x.CreateDayViewModel(DateOnly.Today)).Return(model);

			var result = target.StudentAvailability(DateOnly.Today);

			result.Data.Should().Be.SameInstanceAs(model);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldPersistStudentAvailabilityForm()
		{ 
			var studentAvailabilityPersister = MockRepository.GenerateMock<IStudentAvailabilityPersister>();
			var form = new StudentAvailabilityDayInput();
			var resultData = new StudentAvailabilityDayViewModel();

			var target = new AvailabilityController(null, null, studentAvailabilityPersister);

			studentAvailabilityPersister.Stub(x => x.Persist(form)).Return(resultData);

			var result = target.StudentAvailability(form);
			var data = result.Data as StudentAvailabilityDayViewModel;

			data.Should().Be.SameInstanceAs(resultData);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnErrorMessageOnInvalidModel()
		{
			var target = new AvailabilityController(null, null, null);
			const string message = "Test model validation error";

			target.ModelState.AddModelError("Test", message);

			var result = target.StudentAvailability(new StudentAvailabilityDayInput());
			var data = result.Data as ModelStateResult;

			Assert.That(data.Errors.Single(), Is.EqualTo(message));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldDeleteStudentAvailability()
		{
			var studentAvailabilityPersister = MockRepository.GenerateMock<IStudentAvailabilityPersister>();
			var target = new AvailabilityController(null, null, studentAvailabilityPersister);
			var resultData = new StudentAvailabilityDayViewModel();

			studentAvailabilityPersister.Stub(x => x.Delete(DateOnly.Today)).Return(resultData);

			var result = target.StudentAvailabilityDelete(DateOnly.Today);
			var data = result.Data as StudentAvailabilityDayViewModel;

			data.Should().Be.SameInstanceAs(resultData);
		}
	}

	[TestFixture]
	[DomainTest]
	[WebTest]
	public class AvailabilityControllerTest : IIsolateSystem
	{
		public AvailabilityController Target;
		public FakeLoggedOnUser User;
		public FakePersonRepository PersonRepository;
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public FakeBankHolidayCalendarSiteRepository BankHolidayCalendarSiteRepository;
		public FakeBankHolidayCalendarRepository BankHolidayCalendarRepository;
		public FakeBankHolidayDateRepository BankHolidayDateRepository;
		private ISite _site;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeBankHolidayCalendarRepository>().For<IBankHolidayCalendarRepository>();
			isolate.UseTestDouble<FakeBankHolidayDateRepository>().For<IBankHolidayDateRepository>();
			isolate.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			isolate.UseTestDouble<FakeBankHolidayCalendarSiteRepository>().For<IBankHolidayCalendarSiteRepository>();
			isolate.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}

		[Test]
		public void ShouldGetBankHolidayCalendar()
		{
			var date = DateOnly.Today;
			setupLoggedOnUser(date);
			var calendarId = Guid.NewGuid();
			var calendarName = "ChinaBankHoliday";
			var description = "New Year";
			createBankHolidayCalendarData(date, calendarId, calendarName, description);

			var availabilityDays = Target.StudentAvailabilitiesAndSchedules(date, date).Data as IEnumerable<StudentAvailabilityDayViewModel>;

			var result = availabilityDays.First(m => m.Date == date.ToFixedClientDateOnlyFormat());
			result.BankHolidayCalendar.CalendarId.Should().Be.EqualTo(calendarId);
			result.BankHolidayCalendar.CalendarName.Should().Be.EqualTo(calendarName);
			result.BankHolidayCalendar.DateDescription.Should().Be.EqualTo(description);
		}

		[Test]
		public void ShouldGetNullForBankHolidayCalendar()
		{
			var date = DateOnly.Today;
			setupLoggedOnUser(date);

			var availabilityDays = Target.StudentAvailabilitiesAndSchedules(date, date).Data as IEnumerable<StudentAvailabilityDayViewModel>;

			var result = availabilityDays.First(m => m.Date == date.ToFixedClientDateOnlyFormat());
			result.BankHolidayCalendar.Should().Be.Null();
		}

		private void setupLoggedOnUser(DateOnly date)
		{
			var logonUser = PersonFactory.CreatePersonWithGuid("logon", "user");
			User.SetFakeLoggedOnUser(logonUser);
			PersonRepository.Add(logonUser);
			_site = new Site("site").WithId();
			SiteRepository.Add(_site);
			var team = new Team { Site = _site }.WithDescription(new Description("team")).WithId();
			TeamRepository.Add(team);
			logonUser.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, team));
		}

		private void createBankHolidayCalendarData(DateOnly date, Guid calendarId, string calendarName, string description)
		{
			var calendar = new BankHolidayCalendar { Name = calendarName };
			calendar.SetId(calendarId);
			BankHolidayCalendarRepository.Add(calendar);
			var calendarDate = new BankHolidayDate { Calendar = calendar, Date = date, Description = description };
			BankHolidayDateRepository.Add(calendarDate);
			BankHolidayCalendarSiteRepository.Add(new BankHolidayCalendarSite { Site = _site, Calendar = calendar });
		}
	}
}