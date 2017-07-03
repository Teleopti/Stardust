using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Data;
using Teleopti.Ccc.WebTest.Core.Requests.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.ViewModelFactory
{
	[TestFixture]
	public class RequestsViewModelFactoryTest
	{
		[Test]
		public void ShouldRetrieveDatePickerFormatForPersonForViewModel()
		{
			IPerson person = new Person();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			var loggedOnUser = new FakeLoggedOnUser(person);

			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();
			var target = new RequestsViewModelFactory(null,
				getFakeAbsenceTypesProvider(loggedOnUser),
				new FakePermissionProvider(), null, null, null, null,
				loggedOnUser, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);

			var result = target.CreatePageViewModel();
			var expectedFormat = person.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;
			result.DatePickerFormat.Should().Be.EqualTo(expectedFormat);
		}

		[Test]
		public void ShouldRetrieveAbsenceTypesforViewModel()
		{
			IPerson person = new Person();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			var loggedOnUser = new FakeLoggedOnUser(person);
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();

			var absence = new Absence { Description = new Description("Vacation"), Requestable = true }.WithId();

			var absenceRepository = new FakeAbsenceRepository();
			absenceRepository.Add(absence);

			var absenceTypesProvider = new AbsenceTypesProvider(absenceRepository, loggedOnUser);

			var target = new RequestsViewModelFactory(null, absenceTypesProvider, new FakePermissionProvider(), null, null, null,
													  null, loggedOnUser, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);

			var result = target.CreatePageViewModel();

			var absenceType = result.AbsenceTypes.FirstOrDefault();
			absenceType.Name.Should().Be.EqualTo(absence.Description.Name);
			absenceType.Id.Should().Be.EqualTo(absence.Id);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRetrieveOvertimeTypesforViewModel()
		{
			IPerson person = new Person();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			var loggedOnUser = new FakeLoggedOnUser(person);
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();

			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("overtime paid", MultiplicatorType.Overtime).WithId();

			var multiplicatorDefinitionSetRepository = new FakeMultiplicatorDefinitionSetRepository();
			multiplicatorDefinitionSetRepository.Add(multiplicatorDefinitionSet);

			var multiplicatorDefinitionSetProvider = MockRepository.GenerateMock<IMultiplicatorDefinitionSetProvider>();
			multiplicatorDefinitionSetProvider.Stub(x => x.GetDefinitionSetsForCurrentUser())
				.Return(new List<MultiplicatorDefinitionSetViewModel>() {new MultiplicatorDefinitionSetViewModel()
				{
					Name = multiplicatorDefinitionSet.Name,
					Id = multiplicatorDefinitionSet.Id.Value
				}});

			var target = new RequestsViewModelFactory(null, getFakeAbsenceTypesProvider(loggedOnUser), new FakePermissionProvider(), null, null, null,
													  null, loggedOnUser, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), multiplicatorDefinitionSetProvider);

			var result = target.CreatePageViewModel();

			var overtimeType = result.OvertimeTypes.FirstOrDefault();
			overtimeType.Name.Should().Be.EqualTo(multiplicatorDefinitionSet.Name);
			overtimeType.Id.Should().Be.EqualTo(multiplicatorDefinitionSet.Id);
		}

		[Test]
		public void ShouldReturnEmptyAbsenceAccountModelWhenNoMatchingRequestableAbsenceFound()
		{
			IPerson person = new Person();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			var loggedOnUser = new FakeLoggedOnUser(person);
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();

			var absence = new Absence { Description = new Description("Vacation"), Requestable = true }.WithId();

			var absenceRepository = new FakeAbsenceRepository();
			absenceRepository.Add(absence);

			var absenceTypesProvider = new AbsenceTypesProvider(absenceRepository, loggedOnUser);

			var target = new RequestsViewModelFactory(null, absenceTypesProvider, new FakePermissionProvider(), null, null,
				null, null, loggedOnUser, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);

			var result = target.GetAbsenceAccountViewModel(Guid.NewGuid(), new DateOnly(2013, 1, 1));
			result.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnEmptyAbsenceAccountModelWhenNoMatchingAccountFound()
		{
			IPerson person = new Person();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			var loggedOnUser = new FakeLoggedOnUser(person);
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();

			var absence = new Absence { Description = new Description("Vacation"), Requestable = true }.WithId();

			var absenceRepository = new FakeAbsenceRepository();
			absenceRepository.Add(absence);

			var absenceTypesProvider = new AbsenceTypesProvider(absenceRepository, loggedOnUser);

			var target = new RequestsViewModelFactory(null, absenceTypesProvider, new FakePermissionProvider(), null, null,
				null, null, loggedOnUser, null, new AbsenceAccountProvider(loggedOnUser, new FakePersonAbsenceAccountRepository()), new FakePersonRequestRepository(),
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);

			var result = target.GetAbsenceAccountViewModel(absence.Id.Value, new DateOnly(2013, 1, 1));
			result.Should().Be.Null();
		}

		[Test]
		public void ShouldRetrieveReportableAbsenceTypesforViewModel()
		{
			var absence = new Absence { Description = new Description("Vacation") }.WithId();

			var wfcs = new WorkflowControlSet();
			wfcs.AddAllowedAbsenceForReport(absence);

			var person = new Person { WorkflowControlSet = wfcs };
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			var loggedOnUser = new FakeLoggedOnUser(person);
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();

			var target = new RequestsViewModelFactory(null,
				getFakeAbsenceTypesProvider(loggedOnUser), new FakePermissionProvider(), null, null, null,
				null, loggedOnUser, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);

			var result = target.CreatePageViewModel();

			result.Should().Not.Be.Null();
			result.AbsenceTypesForReport.Should().Not.Be.Null();

			var firstAbsenceType = result.AbsenceTypesForReport.FirstOrDefault();
			firstAbsenceType.Id.Should().Be.EqualTo(absence.Id);
			firstAbsenceType.Name.Should().Be.EqualTo(absence.Description.Name);
		}

		[Test]
		public void ShouldRetrieveTextRequestPermissionForViewModel()
		{
			IPerson person = new Person();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			var loggedOnUser = new FakeLoggedOnUser(person);
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();

			var absenceTypesProvider = MockRepository.GenerateMock<IAbsenceTypesProvider>();
			absenceTypesProvider.Stub(x => x.GetRequestableAbsences()).Return(new List<IAbsence>());
			absenceTypesProvider.Stub(x => x.GetReportableAbsences()).Return(new List<IAbsence>());

			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests)).
				Return(true);

			var target = new RequestsViewModelFactory(null, absenceTypesProvider, permissionProvider, null, null, null,
				null, loggedOnUser, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);
			var result = target.CreatePageViewModel();

			result.RequestPermission.TextRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldRetrieveAbsenceRequestPermissionForViewModel()
		{
			IPerson person = new Person();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			var loggedOnUser = new FakeLoggedOnUser(person);
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();

			var permissionProvider = new FakePermissionProvider();

			var target = new RequestsViewModelFactory(null,
				getFakeAbsenceTypesProvider(loggedOnUser), permissionProvider, null, null, null, null,
				loggedOnUser, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);
			var result = target.CreatePageViewModel();

			result.RequestPermission.AbsenceRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldRetrievePersonRequestsForPagingViewModel()
		{
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();
			var personRequestProvider = MockRepository.GenerateMock<IPersonRequestProvider>();
			var target = new RequestsViewModelFactory(personRequestProvider, null,
				null, null, null, null, null, null, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);
			var paging = new Paging();
			var filter = new RequestListFilter { HideOldRequest = false, IsSortByUpdateDate = true };
			personRequestProvider.Stub(x => x.RetrieveRequestsForLoggedOnUser(paging, filter)).Return(new IPersonRequest[] { });


			target.CreatePagingViewModel(paging, filter);

			personRequestProvider.AssertWasCalled(x => x.RetrieveRequestsForLoggedOnUser(paging, filter));
		}

		[Test]
		public void ShouldRetrievePersonRequestsAfterSpecificDateForPagingViewModel()
		{
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();
			var personRequestProvider = MockRepository.GenerateMock<IPersonRequestProvider>();
			var target = new RequestsViewModelFactory(personRequestProvider, null,
				null, null, null, null, null, null, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);
			var paging = new Paging();
			var filter = new RequestListFilter() { HideOldRequest = true, IsSortByUpdateDate = true };
			personRequestProvider.Stub(x => x.RetrieveRequestsForLoggedOnUser(paging, filter)).Return(new IPersonRequest[] { });

			target.CreatePagingViewModel(paging, filter);

			personRequestProvider.AssertWasCalled(x => x.RetrieveRequestsForLoggedOnUser(paging, filter));
		}

		[Test]
		public void ShouldMapToViewModelForPagingViewModel()
		{
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();
			var filter = new RequestListFilter { HideOldRequest = false, IsSortByUpdateDate = true };
			var paging = new Paging();
			var personRequestProvider = MockRepository.GenerateMock<IPersonRequestProvider>();
			personRequestProvider.Stub(x => x.RetrieveRequestsForLoggedOnUser(paging, filter)).Return(new List<IPersonRequest> { new PersonRequest(new Person(), new ShiftTradeRequest(new List<IShiftTradeSwapDetail>())) });
			var target = new RequestsViewModelFactory(personRequestProvider, null, null,
				null, null, null, null, null, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);
			var result = target.CreatePagingViewModel(paging, filter);

			result.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldMapToViewModelForId()
		{
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();
			var provider = MockRepository.GenerateMock<IPersonRequestProvider>();
			var target = new RequestsViewModelFactory(provider, null, null, null, null, null, null, null, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);
			var personRequest = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			var id = Guid.NewGuid();

			personRequest.SetId(id);

			provider.Stub(p => p.RetrieveRequest(id)).Return(personRequest);

			target.CreateRequestViewModel(id).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRetrieveShiftTradePeriodViewModel()
		{
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();
			var mapper = MockRepository.GenerateMock<IShiftTradePeriodViewModelMapper>();
			var provider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			var now = MockRepository.GenerateMock<INow>();
			var target = new RequestsViewModelFactory(null, null, null, provider, mapper, null, now, null, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);
			var shiftTradePeriodViewModel = new ShiftTradeRequestsPeriodViewModel();
			var workflowControlSet = new WorkflowControlSet();

			provider.Stub(p => p.RetrieveUserWorkflowControlSet()).Return(workflowControlSet);
			mapper.Stub(x => x.Map(workflowControlSet, now)).Return(shiftTradePeriodViewModel);

			var result = target.CreateShiftTradePeriodViewModel();

			result.Should().Be.SameInstanceAs(shiftTradePeriodViewModel);
		}

		[Test]
		public void ShouldSetMiscSettingsFalseWhenNoOfferInShiftTrade()
		{
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();
			var mapper = MockRepository.GenerateMock<IShiftTradePeriodViewModelMapper>();
			var provider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			var now = MockRepository.GenerateMock<INow>();
			var requestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var shiftTradeRequest = MockRepository.GenerateMock<IShiftTradeRequest>();
			requestRepository.Stub(x => x.Find(new Guid())).IgnoreArguments().Return(new PersonRequest(new Person()) { Request = shiftTradeRequest });
			var target = new RequestsViewModelFactory(null, null, null, provider, mapper, null, now, null, null, null, requestRepository,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);
			var shiftTradePeriodViewModel = new ShiftTradeRequestsPeriodViewModel { MiscSetting = new ShiftTradeRequestMiscSetting { AnonymousTrading = true } };
			var workflowControlSet = new WorkflowControlSet();

			provider.Stub(p => p.RetrieveUserWorkflowControlSet()).Return(workflowControlSet);
			mapper.Stub(x => x.Map(workflowControlSet, now)).Return(shiftTradePeriodViewModel);

			var result = target.CreateShiftTradePeriodViewModel(new Guid());

			result.MiscSetting.AnonymousTrading.Should().Be.False();
		}

		[Test]
		public void ShouldRetrieveMyTeamId()
		{
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();
			var provider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			var shiftTradeDate = DateOnly.Today;
			Guid? myTeamId = Guid.NewGuid();

			provider.Stub(x => x.RetrieveMyTeamId(shiftTradeDate)).Return(myTeamId);

			var target = new RequestsViewModelFactory(null, null, null, provider,
													  MockRepository.GenerateMock<IShiftTradePeriodViewModelMapper>(), null,
													  MockRepository.GenerateMock<INow>(), null, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);

			target.CreateShiftTradeMyTeamSimpleViewModel(shiftTradeDate).Should().Be.EqualTo(myTeamId.ToString());
		}

		[Test]
		public void ShouldRetrieveMySiteId()
		{
			var provider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			var shiftTradeDate = DateOnly.Today;
			Guid? mySiteId = Guid.NewGuid();

			provider.Stub(x => x.RetrieveMySiteId(shiftTradeDate)).Return(mySiteId);
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();
			var target = new RequestsViewModelFactory(null, null, null, provider,
													  MockRepository.GenerateMock<IShiftTradePeriodViewModelMapper>(), null,
													  MockRepository.GenerateMock<INow>(), null, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);

			target.CreateShiftTradeMySiteIdViewModel(shiftTradeDate).Should().Be.EqualTo(mySiteId.ToString());
		}

		[Test]
		public void ShouldRetrieveEmptyStringWhenNotBelongingToATeam()
		{
			var provider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			var shiftTradeDate = DateOnly.Today;

			provider.Stub(x => x.RetrieveMyTeamId(shiftTradeDate)).Return(null);
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();
			var target = new RequestsViewModelFactory(null, null, null, provider,
													  MockRepository.GenerateMock<IShiftTradePeriodViewModelMapper>(), null,
													  MockRepository.GenerateMock<INow>(), null, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);

			target.CreateShiftTradeMyTeamSimpleViewModel(shiftTradeDate).Should().Be.EqualTo(string.Empty);
		}

		[Test]
		public void ShouldRetrieveEmptyStringWhenNotBelongingToASite()
		{
			var provider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			var shiftTradeDate = DateOnly.Today;

			provider.Stub(x => x.RetrieveMyTeamId(shiftTradeDate)).Return(null);
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();
			var target = new RequestsViewModelFactory(null, null, null, provider,
													  MockRepository.GenerateMock<IShiftTradePeriodViewModelMapper>(), null,
													  MockRepository.GenerateMock<INow>(), null, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);

			target.CreateShiftTradeMyTeamSimpleViewModel(shiftTradeDate).Should().Be.EqualTo(string.Empty);
		}

		[Test]
		public void ShouldRetrieveShiftTradeScheduleViewModel()
		{
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();
			var mapper = MockRepository.GenerateMock<IShiftTradeScheduleViewModelMapper>();
			var target = new RequestsViewModelFactory(null, null, null, null, null, null, null, null, mapper, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);
			var viewModel = new ShiftTradeScheduleViewModel();
			var teamId = Guid.NewGuid();
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = DateOnly.Today,
				TeamIdList = new List<Guid> { teamId }
			};

			mapper.Stub(x => x.Map(Arg<ShiftTradeScheduleViewModelData>.Is.Anything)).Return(viewModel);

			var result = target.CreateShiftTradeScheduleViewModel(data);
			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldRetriveShiftTradeSwapDetailsViewModel()
		{
			var personRequestId = new Guid();
			var requestCheckSum = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();
			var personRequestProvider = MockRepository.GenerateMock<IPersonRequestProvider>();
			var shiftTrade = MockRepository.GenerateStub<IShiftTradeRequest>();
			var personRequest = MockRepository.GenerateStub<IPersonRequest>();
			personRequest.Request = shiftTrade;

			var detail = MockRepository.GenerateMock<IShiftTradeSwapDetail>();
			var details = new ReadOnlyCollection<IShiftTradeSwapDetail>(new List<IShiftTradeSwapDetail> { detail });
			var loggedOnUser = new FakeLoggedOnUser(new Person());
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();
			var target = new RequestsViewModelFactory(personRequestProvider, null, null, null, null, requestCheckSum,
													  null, loggedOnUser, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);

			personRequestProvider.Expect(p => p.RetrieveRequest(personRequestId)).Return(personRequest);
			shiftTrade.Stub(x => x.ShiftTradeSwapDetails).Return(details);
			requestCheckSum.Expect(s => s.Check(shiftTrade));
			detail.Stub(x => x.Parent).Return(shiftTrade);

			var result = target.CreateShiftTradeRequestSwapDetails(personRequestId);
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldRetrieveIsMyScheduleForLoggedOnUser()
		{
			var personRequestId = new Guid();
			var requestCheckSum = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();
			var personRequestProvider = MockRepository.GenerateMock<IPersonRequestProvider>();
			var personRequest = MockRepository.GenerateStub<IPersonRequest>();

			var personFrom = new Person();
			var personTo = new Person();
			var loggedOnUser = new FakeLoggedOnUser(personTo);
			var detail = new ShiftTradeSwapDetail(personFrom, personTo, DateOnly.Today, DateOnly.Today);
			var shiftTradeSwapDetails = new List<IShiftTradeSwapDetail> { detail };
			var shiftTrade = new ShiftTradeRequest(shiftTradeSwapDetails);
			personRequest.Request = shiftTrade;
			var timeZone = new FakeUserTimeZone();
			var threadCulture = new ThreadCulture();

			var target = new RequestsViewModelFactory(personRequestProvider, null, null, null, null, requestCheckSum,
													  null, loggedOnUser, null, null, null,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new ShiftTradeSwapDetailViewModelMapper(
					new ShiftTradeTimeLineHoursViewModelFactory(new CreateHourText(timeZone, threadCulture),
						timeZone), new ProjectionProvider(), threadCulture, timeZone,
					new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))), new PersonAccountViewModelMapper(timeZone), null);

			personRequestProvider.Expect(p => p.RetrieveRequest(personRequestId)).Return(personRequest);
			requestCheckSum.Expect(s => s.Check(shiftTrade));

			var result = target.CreateShiftTradeRequestSwapDetails(personRequestId);
			result.First().To.IsMySchedule.Should().Be.True();
		}

		private static AbsenceTypesProvider getFakeAbsenceTypesProvider(FakeLoggedOnUser loggedOnUser)
		{
			return new AbsenceTypesProvider(new FakeAbsenceRepository(), loggedOnUser);
		}
	}
}