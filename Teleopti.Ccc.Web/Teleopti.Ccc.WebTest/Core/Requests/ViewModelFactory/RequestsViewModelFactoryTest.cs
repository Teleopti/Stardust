using System;
using System.Collections.Generic;
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
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
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
			var absenceTypesProvider = MockRepository.GenerateMock<IAbsenceTypesProvider>();
			absenceTypesProvider.Stub(x => x.GetRequestableAbsences()).Return(new List<IAbsence>());

			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			IPerson person = new Person();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var target = new RequestsViewModelFactory(null, null, absenceTypesProvider,
			                                          MockRepository.GenerateMock<IPermissionProvider>(), null, null, null, null,
			                                          loggedOnUser, null, null);

			var result = target.CreatePageViewModel();
			var expectedFormat = person.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;
			result.DatePickerFormat.Should().Be.EqualTo(expectedFormat);
		}

		[Test]
		public void ShouldRetrieveAbsenceTypesforViewModel()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			IPerson person = new Person();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var absence = new Absence { Description = new Description("Vacation") };
			absence.SetId(Guid.NewGuid());
			var absences = new List<IAbsence> { absence };

			var absenceTypesProvider = MockRepository.GenerateMock<IAbsenceTypesProvider>();
			absenceTypesProvider.Stub(x => x.GetRequestableAbsences()).Return(absences);

			var target = new RequestsViewModelFactory(null, null, absenceTypesProvider, permissionProvider, null, null, null,
			                                          null, loggedOnUser, null, null);

			var result = target.CreatePageViewModel();

			result.AbsenceTypes.FirstOrDefault().Name.Should().Be.EqualTo(absence.Description.Name);
			result.AbsenceTypes.FirstOrDefault().Id.Should().Be.EqualTo(absence.Id);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRetrieveTextRequestPermissionForViewModel()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			IPerson person = new Person();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var absenceTypesProvider = MockRepository.GenerateMock<IAbsenceTypesProvider>();
			absenceTypesProvider.Stub(x => x.GetRequestableAbsences()).Return(new List<IAbsence>());
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests)).
				Return(true);

			var target = new RequestsViewModelFactory(null, null, absenceTypesProvider, permissionProvider, null, null, null,
			                                          null, loggedOnUser, null, null);
			var result = target.CreatePageViewModel();

			result.RequestPermission.TextRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldRetrieveAbsenceRequestPermissionForViewModel()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			IPerson person = new Person();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);


			var absenceTypesProvider = MockRepository.GenerateMock<IAbsenceTypesProvider>();
			absenceTypesProvider.Stub(x => x.GetRequestableAbsences()).Return(new List<IAbsence>());
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb)).
				Return(true);

			var target = new RequestsViewModelFactory(null, null, absenceTypesProvider, permissionProvider, null, null, null,
			                                          null, loggedOnUser, null, null);
			var result = target.CreatePageViewModel();

			result.RequestPermission.AbsenceRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldRetrievePersonRequestsForPagingViewModel()
		{
			var personRequestProvider = MockRepository.GenerateMock<IPersonRequestProvider>();
			var target = new RequestsViewModelFactory(personRequestProvider, MockRepository.GenerateMock<IMappingEngine>(), null,
			                                          null, null, null, null, null, null, null, null);
			var paging = new Paging();
			personRequestProvider.Stub(x => x.RetrieveRequests(paging)).Return(new IPersonRequest[] { });

			target.CreatePagingViewModel(paging);

			personRequestProvider.AssertWasCalled(x => x.RetrieveRequests(paging));
		}

		[Test]
		public void ShouldMapToViewModelForPagingViewModel()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new RequestsViewModelFactory(MockRepository.GenerateMock<IPersonRequestProvider>(), mapper, null, null,
			                                          null, null, null, null, null, null, null);
			var requests = new RequestViewModel[] {};

			mapper.Stub(x => x.Map<IEnumerable<IPersonRequest>, IEnumerable<RequestViewModel>>(null)).Return(requests);

			var result = target.CreatePagingViewModel(null);

			result.Should().Be.SameInstanceAs(requests);
		}

		[Test]
		public void ShouldMapToViewModelForId()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var provider = MockRepository.GenerateMock<IPersonRequestProvider>();
			var target = new RequestsViewModelFactory(provider, mapper, null, null, null, null, null, null, null, null, null);
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
			var target = new RequestsViewModelFactory(null, null, null, null, provider, mapper, null, now, null, null, null);
			var shiftTradePeriodViewModel = new ShiftTradeRequestsPeriodViewModel();
			var workflowControlSet = new WorkflowControlSet();

			provider.Stub(p => p.RetrieveUserWorkflowControlSet()).Return(workflowControlSet);
			mapper.Stub(x => x.Map(workflowControlSet, now)).Return(shiftTradePeriodViewModel);

			var result = target.CreateShiftTradePeriodViewModel();

			result.Should().Be.SameInstanceAs(shiftTradePeriodViewModel);
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
													  MockRepository.GenerateMock<INow>(), null, null, null);

			target.CreateShiftTradeMyTeamSimpleViewModel(shiftTradeDate).Should().Be.EqualTo(myTeamId.ToString());
		}

		[Test]
		public void ShouldRetrieveEmptyStringWhenNotBelongingToATeam()
		{
			var provider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			var shiftTradeDate = DateOnly.Today;

			provider.Stub(x => x.RetrieveMyTeamId(shiftTradeDate)).Return(null);

			var target = new RequestsViewModelFactory(null, null, null, null, provider,
													  MockRepository.GenerateMock<IShiftTradePeriodViewModelMapper>(), null,
													  MockRepository.GenerateMock<INow>(), null, null, null);

			target.CreateShiftTradeMyTeamSimpleViewModel(shiftTradeDate).Should().Be.EqualTo(string.Empty);
		}

		[Test]
		public void ShouldRetrieveShiftTradeScheduleViewModel()
		{
			var mapper = MockRepository.GenerateMock<IShiftTradeScheduleViewModelMapper>();
			var target = new RequestsViewModelFactory(null, null, null, null, null, null, null, null, null, mapper, null);
			var viewModel = new ShiftTradeScheduleViewModel();
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = DateOnly.Today, TeamId = Guid.NewGuid() };

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

			var target = new RequestsViewModelFactory(personRequestProvider, mapper, null, null, null, null, requestCheckSum,
			                                          null, null, null, null);

			personRequestProvider.Expect(p => p.RetrieveRequest(personRequestId)).Return(personRequest);
			requestCheckSum.Expect(s => s.Check(shiftTrade));		
			mapper.Expect(m => m.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(Arg<IShiftTradeRequest>.Is.Equal(shiftTrade)))
			      .Return(shiftTradeSwapDetailsViewModel);

			var result = target.CreateShiftTradeRequestSwapDetails(personRequestId);
			Assert.That(result,Is.SameAs(shiftTradeSwapDetailsViewModel));

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

			var target = new RequestsViewModelFactory(personRequestProvider, mapper, null, null, null, null, requestCheckSum,
			                                          null, null, null, null);

			personRequestProvider.Expect(p => p.RetrieveRequest(new Guid())).IgnoreArguments().Return(personRequest);

			mapper.Expect(
				m => m.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(Arg<IShiftTradeRequest>.Is.Equal(shiftTrade)))
			      .Return(shiftTradeSwapDetailsViewModel);

			var result = target.CreateShiftTradeRequestSwapDetails(new Guid());
			Assert.That(result.From.MinutesSinceTimeLineStart, Is.EqualTo(20));
			Assert.That(result.To.MinutesSinceTimeLineStart, Is.EqualTo(180));

		}
	}
}
