using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Ccc.WebTest.Core.Requests.DataProvider;
using Teleopti.Interfaces.Domain;
using IRequestsViewModelFactory = Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory.IRequestsViewModelFactory;

namespace Teleopti.Ccc.WebTest.Core.Requests.ViewModelFactory
{
	[SetCulture("en-US")]
	[MyTimeWebTest]
	[TestFixture]
	public class RequestsViewModelFactoryTest : IIsolateSystem
	{
		public IPersonScheduleDayReadModelFinder PersonScheduleDayReadModelFinder;
		public IPermissionProvider PermissionProvider;
		public INow Now;
		public IRequestsViewModelFactory RequestsViewModelFactory;
		public ILoggedOnUser LoggedOnUser;
		public FakeAbsenceRepository FakeAbsenceRepository;
		public FakeMultiplicatorDefinitionSetRepository FakeMultiplicatorDefinitionSetRepository;
		public FakePermissionProvider FakePermissionProvider;
		public FakePersonRequestRepository FakePersonRequestRepository;
		public ICurrentDataSource CurrentDataSource;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeAbsenceRepository>().For<IAbsenceRepository>();
			isolate.UseTestDouble<FakeMultiplicatorDefinitionSetRepository>().For<IMultiplicatorDefinitionSetRepository>();
			isolate.UseTestDouble<FakePermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble<FakeLinkProvider>().For<ILinkProvider>();
		}

		[Test]
		public void ShouldRetrieveDatePickerFormatForPersonForViewModel()
		{
			setUpWorkFlowControlSet();

			currentUser().PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));

			var result = RequestsViewModelFactory.CreatePageViewModel();

			var expectedFormat = currentUser().PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;
			result.DatePickerFormat.Should().Be.EqualTo(expectedFormat);
		}

		[Test]
		public void ShouldRetrieveAbsenceTypesforViewModel()
		{
			setUpWorkFlowControlSet();

			var absence = new Absence { Description = new Description("Vacation"), Requestable = true }.WithId();
			FakeAbsenceRepository.Add(absence);

			var result = RequestsViewModelFactory.CreatePageViewModel();

			var absenceType = result.AbsenceTypes.FirstOrDefault();
			absenceType.Name.Should().Be.EqualTo(absence.Description.Name);
			absenceType.Id.Should().Be.EqualTo(absence.Id);
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRetrieveOvertimeTypesforViewModel()
		{
			setUpWorkFlowControlSet();

			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("overtime paid", MultiplicatorType.Overtime).WithId();
			var personContract = new PersonContract(new Contract("sd"), new PartTimePercentage("d"), new ContractSchedule("d"));
			personContract.Contract.AddMultiplicatorDefinitionSetCollection(multiplicatorDefinitionSet);
			currentUser().AddPersonPeriod(new PersonPeriod(new DateOnly(Now.UtcDateTime()), personContract, new Team()));
			FakeMultiplicatorDefinitionSetRepository.Add(multiplicatorDefinitionSet);

			var result = RequestsViewModelFactory.CreatePageViewModel();

			Assert.AreEqual(result.OvertimeTypes.GetType(), typeof(List<OvertimeTypeViewModel>));
			Assert.AreEqual(result.OvertimeTypes.FirstOrDefault().Name, multiplicatorDefinitionSet.Name);
			Assert.AreEqual(result.OvertimeTypes.FirstOrDefault().Id, multiplicatorDefinitionSet.Id);
		}

		[Test]
		public void ShouldReturnEmptyAbsenceAccountModelWhenNoMatchingRequestableAbsenceFound()
		{
			var absence = new Absence { Description = new Description("Vacation"), Requestable = true }.WithId();
			FakeAbsenceRepository.Add(absence);
			var result = RequestsViewModelFactory.GetAbsenceAccountViewModel(Guid.NewGuid(), new DateOnly(Now.UtcDateTime()));
			result.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnEmptyAbsenceAccountModelWhenNoMatchingAccountFound()
		{
			var absence = new Absence { Description = new Description("Vacation"), Requestable = true }.WithId();
			FakeAbsenceRepository.Add(absence);

			var result = RequestsViewModelFactory.GetAbsenceAccountViewModel(absence.Id.Value, new DateOnly(2013, 1, 1));
			result.Should().Be.Null();
		}

		[Test]
		public void ShouldRetrieveReportableAbsenceTypesforViewModel()
		{
			var absence = new Absence { Description = new Description("Vacation") }.WithId();
			var wfcs = new WorkflowControlSet();
			wfcs.AddAllowedAbsenceForReport(absence);
			currentUser().WorkflowControlSet = wfcs;
			var result = RequestsViewModelFactory.CreatePageViewModel();

			result.Should().Not.Be.Null();
			result.AbsenceTypesForReport.Should().Not.Be.Null();

			var firstAbsenceType = result.AbsenceTypesForReport.FirstOrDefault();
			firstAbsenceType.Id.Should().Be.EqualTo(absence.Id);
			firstAbsenceType.Name.Should().Be.EqualTo(absence.Description.Name);
		}

		[Test]
		public void ShouldRetrieveTextRequestPermissionForViewModel()
		{
			FakePermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.TextRequests,
				new DateOnly(Now.UtcDateTime()), currentUser());

			var result = RequestsViewModelFactory.CreatePageViewModel();

			result.RequestPermission.TextRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldRetrieveAbsenceRequestPermissionForViewModel()
		{
			FakePermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb, 
				new DateOnly(Now.UtcDateTime()), currentUser());

			var result = RequestsViewModelFactory.CreatePageViewModel();

			result.RequestPermission.AbsenceRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldRetrievePersonRequestsForPagingViewModel()
		{
			var personRequestFactory = new PersonRequestFactory();
			var personRequest1 = personRequestFactory.CreatePersonRequest(currentUser()).WithId();
			var personRequest2 = personRequestFactory.CreatePersonRequest(currentUser()).WithId();
			FakePersonRequestRepository.Add(personRequest1);
			FakePersonRequestRepository.Add(personRequest2);

			var paging = new Paging {Skip = 0, Take = 1};
			var filter = new RequestListFilter { HideOldRequest = false, IsSortByUpdateDate = true };
			var result = RequestsViewModelFactory.CreatePagingViewModel(paging, filter);

			result.Count().Should().Be(1);
			result.FirstOrDefault().Id.Should().Be(personRequest1.Id.ToString());
		}

		[Test]
		public void ShouldRetrievePersonRequestsAfterSpecificDateForPagingViewModel()
		{
			var personRequestFactory = new PersonRequestFactory { Person = currentUser() };
			var personRequest1 = personRequestFactory.CreatePersonRequest().WithId();
			var textRequest1 = new TextRequest(new DateTimePeriod(2014, 1, 1, 2014, 1, 2)).WithId();
			personRequest1.Request = textRequest1;
			FakePersonRequestRepository.Add(personRequest1);

			var personRequest2 = personRequestFactory.CreatePersonRequest().WithId();
			var textRequest2 = new TextRequest(new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddHours(1))).WithId();
			personRequest2.Request = textRequest2;
			FakePersonRequestRepository.Add(personRequest2);

			var paging = new Paging { Skip = 0, Take = 2 };
			var filter = new RequestListFilter { HideOldRequest = true, IsSortByUpdateDate = true };
			var result = RequestsViewModelFactory.CreatePagingViewModel(paging, filter);

			result.Count().Should().Be(1);
			result.FirstOrDefault().Id.Should().Be(personRequest2.Id.ToString());
		}

		[Test]
		public void ShouldRetrieveShiftTradePeriodViewModel()
		{
			setUpWorkFlowControlSet();

			var result = RequestsViewModelFactory.CreateShiftTradePeriodViewModel();

			result.HasWorkflowControlSet.Should().Be.EqualTo(true);
			result.MiscSetting.AnonymousTrading.Should().Be.EqualTo(true);
			result.OpenPeriodRelativeStart.Should().Be.EqualTo(1);
			result.OpenPeriodRelativeEnd.Should().Be.EqualTo(3);
			result.NowYear.Should().Be.EqualTo(Now.UtcDateTime().Year);
			result.NowMonth.Should().Be.EqualTo(Now.UtcDateTime().Month);
			result.NowDay.Should().Be.EqualTo(Now.UtcDateTime().Day);
		}

		[Test]
		public void ShouldNotThrowExpcetionIfRequestNotFoundWhenRetrieveShiftTradePeriodViewModel()
		{
			setUpWorkFlowControlSet();

			var result = RequestsViewModelFactory.CreateShiftTradePeriodViewModel(Guid.NewGuid());

			result.HasWorkflowControlSet.Should().Be.EqualTo(true);
			result.MiscSetting.AnonymousTrading.Should().Be.EqualTo(true);
			result.OpenPeriodRelativeStart.Should().Be.EqualTo(1);
			result.OpenPeriodRelativeEnd.Should().Be.EqualTo(3);
			result.NowYear.Should().Be.EqualTo(Now.UtcDateTime().Year);
			result.NowMonth.Should().Be.EqualTo(Now.UtcDateTime().Month);
			result.NowDay.Should().Be.EqualTo(Now.UtcDateTime().Day);
		}

		[Test]
		public void ShouldSetMiscSettingsFalseWhenNoOfferInShiftTrade()
		{
			setUpWorkFlowControlSet();

			var personRequestFactory = new PersonRequestFactory { Person = currentUser() };
			var personRequest =
				personRequestFactory.CreatePersonShiftTradeRequest(currentUser(), new Person(), new DateOnly(Now.UtcDateTime())).WithId();
			FakePersonRequestRepository.Add(personRequest);

			var result = RequestsViewModelFactory.CreateShiftTradePeriodViewModel(personRequest.Id);

			result.MiscSetting.AnonymousTrading.Should().Be.False();
		}

		[Test]
		public void ShouldRetrieveMyTeamId()
		{
			var myTeam = new Team().WithId();
			LoggedOnUser.CurrentUser().AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today,myTeam));

			RequestsViewModelFactory.CreateShiftTradeMyTeamSimpleViewModel(DateOnly.Today).Should().Be.EqualTo(myTeam.Id.ToString());
		}

		[Test]
		public void ShouldRetrieveMySiteId()
		{
			var site = new Site("site").WithId();
			var myTeam = new Team
			{
				Site= site
			}.WithId();
			LoggedOnUser.CurrentUser().AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today, myTeam));

			RequestsViewModelFactory.CreateShiftTradeMySiteIdViewModel(DateOnly.Today).Should().Be.EqualTo(site.Id.ToString());
		}

		[Test]
		public void ShouldRetrieveEmptyStringWhenNotBelongingToATeam()
		{
			RequestsViewModelFactory.CreateShiftTradeMyTeamSimpleViewModel(DateOnly.Today).Should().Be.EqualTo(string.Empty);
		}

		[Test]
		public void ShouldRetrieveEmptyStringWhenNotBelongingToASite()
		{
			var myTeam = new Team().WithId();
			LoggedOnUser.CurrentUser().AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today, myTeam));

			RequestsViewModelFactory.CreateShiftTradeMySiteIdViewModel(DateOnly.Today).Should().Be.EqualTo(string.Empty);
		}

		[Test]
		public void ShouldRetrieveShiftTradeScheduleViewModel()
		{
			var teamId = Guid.NewGuid();
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = DateOnly.Today,
				TeamIdList = new List<Guid> { teamId }
			};

			var result = RequestsViewModelFactory.CreateShiftTradeScheduleViewModel(data);
			result.PageCount.Should().Be(0);
			result.MySchedule.Should().Be(null);
		}

		[Test]
		public void ShouldRetriveShiftTradeSwapDetailsViewModel()
		{
			var personRequestFactory = new PersonRequestFactory { Person = currentUser() };
			var personRequest =
				personRequestFactory.CreatePersonShiftTradeRequest(currentUser(), new Person(), new DateOnly(Now.UtcDateTime())).WithId();
			FakePersonRequestRepository.Add(personRequest);

			var result = RequestsViewModelFactory.CreateShiftTradeRequestSwapDetails(personRequest.Id.Value);
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldRetrieveIsMyScheduleForLoggedOnUser()
		{
			var personRequestFactory = new PersonRequestFactory { Person = currentUser() };
			var personRequest =
				personRequestFactory.CreatePersonShiftTradeRequest( new Person(), currentUser(), new DateOnly(Now.UtcDateTime())).WithId();
			FakePersonRequestRepository.Add(personRequest);

			var result = RequestsViewModelFactory.CreateShiftTradeRequestSwapDetails(personRequest.Id.Value);
			result.First().To.IsMySchedule.Should().Be.True();
		}

		[Test]
		public void ShouldRetrieveRequestLicenseForRequestsViewModel()
		{
			var result = RequestsViewModelFactory.CreatePageViewModel();

			result.RequestLicense.IsOvertimeAvailabilityEnabled.Should().Be(true);
			result.RequestLicense.IsOvertimeRequestEnabled.Should().Be(true);
		}

		[Test]
		public void ShouldRetrieveRequestLicenseWithOvertimeAvailabilityDisabled()
		{
			var licenseActivator = LicenseProvider.GetLicenseActivator(new OvertimeFakeLicenseService(false, true));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var result = RequestsViewModelFactory.CreatePageViewModel();

			result.RequestLicense.IsOvertimeAvailabilityEnabled.Should().Be(false);
			result.RequestLicense.IsOvertimeRequestEnabled.Should().Be(true);
		}

		private void setUpWorkFlowControlSet()
		{
			var workflowControlSet = new WorkflowControlSet { AnonymousTrading = true, ShiftTradeOpenPeriodDaysForward = new MinMax<int>(1, 3) };
			currentUser().WorkflowControlSet = workflowControlSet;
			currentUser().PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
		}

		private IPerson currentUser()
		{
			return LoggedOnUser.CurrentUser();
		}
	}
}