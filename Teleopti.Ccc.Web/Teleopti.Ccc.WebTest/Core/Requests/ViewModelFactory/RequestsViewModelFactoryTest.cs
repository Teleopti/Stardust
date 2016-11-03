﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
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

			var target = new RequestsViewModelFactory(null, null,
				new AbsenceTypesProvider(new FakeAbsenceRepository(), loggedOnUser),
				new FakePermissionProvider(), null, null, null, null,
				loggedOnUser, null, null, null);

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

			var absence = new Absence { Description = new Description("Vacation") ,Requestable = true}.WithId();

			var absenceRepository = new FakeAbsenceRepository();
			absenceRepository.Add(absence);

			var absenceTypesProvider = new AbsenceTypesProvider(absenceRepository, loggedOnUser);

			var target = new RequestsViewModelFactory(null, null, absenceTypesProvider, new FakePermissionProvider(), null, null, null,
			                                          null, loggedOnUser, null, null, null);

			var result = target.CreatePageViewModel();

			var absenceType = result.AbsenceTypes.FirstOrDefault();
			absenceType.Name.Should().Be.EqualTo(absence.Description.Name);
			absenceType.Id.Should().Be.EqualTo(absence.Id);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnEmptyAbsenceAccountModelWhenNoMatchingRequestableAbsenceFound()
		{
			IPerson person = new Person();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			var loggedOnUser = new FakeLoggedOnUser(person);

			var absence = new Absence { Description = new Description("Vacation"), Requestable = true }.WithId();

			var absenceRepository = new FakeAbsenceRepository();
			absenceRepository.Add(absence);

			var absenceTypesProvider = new AbsenceTypesProvider(absenceRepository, loggedOnUser);

			var target = new RequestsViewModelFactory(null, null, absenceTypesProvider, new FakePermissionProvider(), null, null,
				null, null, loggedOnUser, null, null, null);

			var result = target.GetAbsenceAccountViewModel(Guid.NewGuid(), new DateOnly(2013, 1, 1));
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

			var target = new RequestsViewModelFactory(null, null,
				new AbsenceTypesProvider(new FakeAbsenceRepository(), loggedOnUser), new FakePermissionProvider(), null, null, null,
				null, loggedOnUser, null, null, null);

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

			var absenceTypesProvider = MockRepository.GenerateMock<IAbsenceTypesProvider>();
			absenceTypesProvider.Stub(x => x.GetRequestableAbsences()).Return(new List<IAbsence>());
			absenceTypesProvider.Stub(x => x.GetReportableAbsences()).Return(new List<IAbsence>());

			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests)).
				Return(true);

			var target = new RequestsViewModelFactory(null, null, absenceTypesProvider, permissionProvider, null, null, null,
				null, loggedOnUser, null, null, null);
			var result = target.CreatePageViewModel();

			result.RequestPermission.TextRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldRetrieveAbsenceRequestPermissionForViewModel()
		{
			IPerson person = new Person();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			var loggedOnUser = new FakeLoggedOnUser(person);

			var permissionProvider = new FakePermissionProvider();

			var target = new RequestsViewModelFactory(null, null,
				new AbsenceTypesProvider(new FakeAbsenceRepository(), loggedOnUser), permissionProvider, null, null, null, null,
				loggedOnUser, null, null, null);
			var result = target.CreatePageViewModel();

			result.RequestPermission.AbsenceRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldRetrievePersonRequestsForPagingViewModel()
		{
			var personRequestProvider = MockRepository.GenerateMock<IPersonRequestProvider>();
			var target = new RequestsViewModelFactory(personRequestProvider, MockRepository.GenerateMock<IMappingEngine>(), null,
				null, null, null, null, null, null, null, null, null);
			var paging = new Paging();
			personRequestProvider.Stub(x => x.RetrieveRequestsForLoggedOnUser(paging, false)).Return(new IPersonRequest[] { });
			var filter = new RequestListFilter() { HideOldRequest = false, SortByUpdateDate = true };

			target.CreatePagingViewModel(paging, filter);

			personRequestProvider.AssertWasCalled(x => x.RetrieveRequestsForLoggedOnUser(paging, false));
		}

		[Test]
		public void ShouldRetrievePersonRequestsAfterSpecificDateForPagingViewModel()
		{
			var personRequestProvider = MockRepository.GenerateMock<IPersonRequestProvider>();
			var target = new RequestsViewModelFactory(personRequestProvider, MockRepository.GenerateMock<IMappingEngine>(), null,
				null, null, null, null, null, null, null, null, null);
			var paging = new Paging();
			personRequestProvider.Stub(x => x.RetrieveRequestsForLoggedOnUser(paging, true)).Return(new IPersonRequest[] { });
			var filter = new RequestListFilter() { HideOldRequest = true, SortByUpdateDate = true };

			var result = target.CreatePagingViewModel(paging, filter);

			personRequestProvider.AssertWasCalled(x => x.RetrieveRequestsForLoggedOnUser(paging, true));
		}

		[Test]
		public void ShouldMapToViewModelForPagingViewModel()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new RequestsViewModelFactory(MockRepository.GenerateMock<IPersonRequestProvider>(), mapper, null, null,
				null, null, null, null, null, null, null, null);
			var requests = new RequestViewModel[] {};
			var filter = new RequestListFilter() { HideOldRequest = false, SortByUpdateDate = true };

			mapper.Stub(x => x.Map<IEnumerable<IPersonRequest>, IEnumerable<RequestViewModel>>(null)).Return(requests);

			var result = target.CreatePagingViewModel(new Paging(), filter);

			result.Should().Be.SameInstanceAs(requests);
		}

		[Test]
		public void ShouldMapToViewModelForId()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var provider = MockRepository.GenerateMock<IPersonRequestProvider>();
			var target = new RequestsViewModelFactory(provider, mapper, null, null, null, null, null, null, null, null, null, null);
			var personRequest = new PersonRequest(new Person());
			var id = Guid.NewGuid();
			var requestViewModel = new RequestViewModel();

			personRequest.SetId(id);

			provider.Stub(p => p.RetrieveRequest(id)).Return(personRequest);
			mapper.Stub(x => x.Map<IPersonRequest, RequestViewModel>(personRequest)).Return(requestViewModel);

			var result = target.CreateRequestViewModel(id);

			result.Should().Be.SameInstanceAs(requestViewModel);
		}

		[Test]
		public void ShouldRetrieveShiftTradePeriodViewModel()
		{
			var mapper = MockRepository.GenerateMock<IShiftTradePeriodViewModelMapper>();
			var provider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			var now = MockRepository.GenerateMock<INow>();
			var target = new RequestsViewModelFactory(null, null, null, null, provider, mapper, null, now, null, null, null, null);
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
			var mapper = MockRepository.GenerateMock<IShiftTradePeriodViewModelMapper>();
			var provider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			var now = MockRepository.GenerateMock<INow>();
			var requestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var shiftTradeRequest = MockRepository.GenerateMock<IShiftTradeRequest>();
			requestRepository.Stub(x => x.Find(new Guid())).IgnoreArguments().Return(new PersonRequest(new Person()){Request = shiftTradeRequest});
			var target = new RequestsViewModelFactory(null, null, null, null, provider, mapper, null, now, null, null, null, requestRepository);
			var shiftTradePeriodViewModel = new ShiftTradeRequestsPeriodViewModel(){MiscSetting = new ShiftTradeRequestMiscSetting(){AnonymousTrading = true}};
			var workflowControlSet = new WorkflowControlSet();

			provider.Stub(p => p.RetrieveUserWorkflowControlSet()).Return(workflowControlSet);
			mapper.Stub(x => x.Map(workflowControlSet, now)).Return(shiftTradePeriodViewModel);

			var result = target.CreateShiftTradePeriodViewModel(new Guid());

			result.MiscSetting.AnonymousTrading.Should().Be.False();
		}

		[Test]
		public void ShouldRetrieveMyTeamId()
		{
			var provider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			var shiftTradeDate = DateOnly.Today;
			Guid? myTeamId = Guid.NewGuid();

			provider.Stub(x => x.RetrieveMyTeamId(shiftTradeDate)).Return(myTeamId);

			var target = new RequestsViewModelFactory(null, null, null, null, provider,
			                                          MockRepository.GenerateMock<IShiftTradePeriodViewModelMapper>(), null,
													  MockRepository.GenerateMock<INow>(), null, null, null, null);

			target.CreateShiftTradeMyTeamSimpleViewModel(shiftTradeDate).Should().Be.EqualTo(myTeamId.ToString());
		}

		[Test]
		public void ShouldRetrieveMySiteId()
		{
			var provider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			var shiftTradeDate = DateOnly.Today;
			Guid? mySiteId = Guid.NewGuid();

			provider.Stub(x => x.RetrieveMySiteId(shiftTradeDate)).Return(mySiteId);

			var target = new RequestsViewModelFactory(null, null, null, null, provider,
			                                          MockRepository.GenerateMock<IShiftTradePeriodViewModelMapper>(), null,
													  MockRepository.GenerateMock<INow>(), null, null, null, null);

			target.CreateShiftTradeMySiteIdViewModel(shiftTradeDate).Should().Be.EqualTo(mySiteId.ToString());
		}

		[Test]
		public void ShouldRetrieveEmptyStringWhenNotBelongingToATeam()
		{
			var provider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			var shiftTradeDate = DateOnly.Today;

			provider.Stub(x => x.RetrieveMyTeamId(shiftTradeDate)).Return(null);

			var target = new RequestsViewModelFactory(null, null, null, null, provider,
													  MockRepository.GenerateMock<IShiftTradePeriodViewModelMapper>(), null,
													  MockRepository.GenerateMock<INow>(), null, null, null, null);

			target.CreateShiftTradeMyTeamSimpleViewModel(shiftTradeDate).Should().Be.EqualTo(string.Empty);
		}

		[Test]
		public void ShouldRetrieveEmptyStringWhenNotBelongingToASite()
		{
			var provider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			var shiftTradeDate = DateOnly.Today;

			provider.Stub(x => x.RetrieveMyTeamId(shiftTradeDate)).Return(null);

			var target = new RequestsViewModelFactory(null, null, null, null, provider,
													  MockRepository.GenerateMock<IShiftTradePeriodViewModelMapper>(), null,
													  MockRepository.GenerateMock<INow>(), null, null, null, null);

			target.CreateShiftTradeMyTeamSimpleViewModel(shiftTradeDate).Should().Be.EqualTo(string.Empty);
		}

		[Test]
		public void ShouldRetrieveShiftTradeScheduleViewModel()
		{
			var mapper = MockRepository.GenerateMock<IShiftTradeScheduleViewModelMapper>();
			var target = new RequestsViewModelFactory(null, null, null, null, null, null, null, null, null, mapper, null, null);
			var viewModel = new ShiftTradeScheduleViewModel();
			var teamId = Guid.NewGuid();
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = DateOnly.Today,
				TeamIdList = new List<Guid> {teamId}
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
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var personRequestProvider = MockRepository.GenerateMock<IPersonRequestProvider>();
			var shiftTrade = MockRepository.GenerateStub<IShiftTradeRequest>();
			var personRequest = MockRepository.GenerateStub<IPersonRequest>(); personRequest.Request = shiftTrade;
			var shiftTradeSwapDetailsViewModel = new ShiftTradeSwapDetailsViewModel
				                                     {
														From = new ShiftTradeEditPersonScheduleViewModel(),
														To = new ShiftTradeEditPersonScheduleViewModel()
				                                     };
		    var detail = MockRepository.GenerateMock<IShiftTradeSwapDetail>();
		    var details = new ReadOnlyCollection<IShiftTradeSwapDetail>(new List<IShiftTradeSwapDetail>{detail});
			var loggedOnUser = new FakeLoggedOnUser(new Person());

			var target = new RequestsViewModelFactory(personRequestProvider, mapper, null, null, null, null, requestCheckSum,
			                                          null, loggedOnUser, null, null, null);

			personRequestProvider.Expect(p => p.RetrieveRequest(personRequestId)).Return(personRequest);
		    shiftTrade.Stub(x => x.ShiftTradeSwapDetails).Return(details);
			requestCheckSum.Expect(s => s.Check(shiftTrade));
            mapper.Expect(m => m.Map<IShiftTradeSwapDetail, ShiftTradeSwapDetailsViewModel>(Arg<IShiftTradeSwapDetail>.Is.Equal(detail)))
			      .Return(shiftTradeSwapDetailsViewModel);

			var result = target.CreateShiftTradeRequestSwapDetails(personRequestId);
			Assert.That(result.First(),Is.SameAs(shiftTradeSwapDetailsViewModel));
		}

		[Test]
		public void ShouldSetMinutesSinceTimelineStartOnShiftTradeSwapDetailsBasedOnTheTimelinesStartDateTime()
		{
			var timelineStartTime = new DateTime(2001, 10, 10, 1, 0, 0, DateTimeKind.Utc);
			var startTimeFromSchedule = timelineStartTime.Add(TimeSpan.FromMinutes(20));
			var startTimeToSchedule = timelineStartTime.Add(TimeSpan.FromMinutes(180));

			var requestCheckSum = MockRepository.GenerateStub<IShiftTradeRequestStatusChecker>();
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var personRequestProvider = MockRepository.GenerateMock<IPersonRequestProvider>();
			var shiftTrade = MockRepository.GenerateStub<IShiftTradeRequest>();
			var personRequest = MockRepository.GenerateStub<IPersonRequest>(); personRequest.Request = shiftTrade;

			var shiftTradeSwapDetailsViewModel = new ShiftTradeSwapDetailsViewModel()
			{
				TimeLineStartDateTime = timelineStartTime,
				From = new ShiftTradeEditPersonScheduleViewModel {StartTimeUtc = startTimeFromSchedule},
				To = new ShiftTradeEditPersonScheduleViewModel {StartTimeUtc = startTimeToSchedule}
			};

            var detail = MockRepository.GenerateMock<IShiftTradeSwapDetail>();
            var details = new ReadOnlyCollection<IShiftTradeSwapDetail>(new List<IShiftTradeSwapDetail> { detail });
		    shiftTrade.Stub(x => x.ShiftTradeSwapDetails).Return(details);
			var loggedOnUser = new FakeLoggedOnUser(new Person());
			var target = new RequestsViewModelFactory(personRequestProvider, mapper, null, null, null, null, requestCheckSum,
			                                          null, loggedOnUser, null, null, null);

			personRequestProvider.Expect(p => p.RetrieveRequest(new Guid())).IgnoreArguments().Return(personRequest);

			mapper.Expect(
                m => m.Map<IShiftTradeSwapDetail, ShiftTradeSwapDetailsViewModel>(Arg<IShiftTradeSwapDetail>.Is.Equal(detail)))
			      .Return(shiftTradeSwapDetailsViewModel);

			var result = target.CreateShiftTradeRequestSwapDetails(new Guid());
			Assert.That(result.First().From.MinutesSinceTimeLineStart, Is.EqualTo(20));
			Assert.That(result.First().To.MinutesSinceTimeLineStart, Is.EqualTo(180));
		}

		[Test]
		public void ShouldRetrieveIsMyScheduleForLoggedOnUser()
		{
			var personRequestId = new Guid();
			var requestCheckSum = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var personRequestProvider = MockRepository.GenerateMock<IPersonRequestProvider>();
			var personRequest = MockRepository.GenerateStub<IPersonRequest>();

			var personFrom = new Person();
			var personTo = new Person();
			var loggedOnUser = new FakeLoggedOnUser(personTo);
			var detail = new ShiftTradeSwapDetail(personFrom, personTo, DateOnly.Today, DateOnly.Today);
			var shiftTradeSwapDetails = new List<IShiftTradeSwapDetail> { detail };
			var shiftTrade = new ShiftTradeRequest(shiftTradeSwapDetails);
			personRequest.Request = shiftTrade;

			var target = new RequestsViewModelFactory(personRequestProvider, mapper, null, null, null, null, requestCheckSum,
													  null, loggedOnUser, null, null, null);

			var shiftTradeSwapDetailsViewModel = new ShiftTradeSwapDetailsViewModel
			{
				From = new ShiftTradeEditPersonScheduleViewModel(),
				To = new ShiftTradeEditPersonScheduleViewModel()
			};

			personRequestProvider.Expect(p => p.RetrieveRequest(personRequestId)).Return(personRequest);
			requestCheckSum.Expect(s => s.Check(shiftTrade));
			mapper.Expect(m => m.Map<IShiftTradeSwapDetail, ShiftTradeSwapDetailsViewModel>(Arg<IShiftTradeSwapDetail>.Is.Equal(detail)))
				 .Return(shiftTradeSwapDetailsViewModel);

			var result = target.CreateShiftTradeRequestSwapDetails(personRequestId);
			result.First().To.IsMySchedule.Should().Be.True();
		}
	}
}