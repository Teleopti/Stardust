using System;
using System.Collections.Generic;
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
		public void ShouldRetrieveAbsenceTypesforViewModel()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var absence = new Absence { Description = new Description("Vacation") };
			absence.SetId(Guid.NewGuid());
			var absences = new List<IAbsence> { absence };

			var absenceTypesProvider = MockRepository.GenerateMock<IAbsenceTypesProvider>();
			absenceTypesProvider.Stub(x => x.GetRequestableAbsences()).Return(absences);

			var target = new RequestsViewModelFactory(null, null, absenceTypesProvider, permissionProvider, null, null);

			var result = target.CreatePageViewModel();

			result.AbsenceTypes.FirstOrDefault().Name.Should().Be.EqualTo(absence.Description.Name);
			result.AbsenceTypes.FirstOrDefault().Id.Should().Be.EqualTo(absence.Id);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRetrieveTextRequestPermissionForViewModel()
		{
			var absenceTypesProvider = MockRepository.GenerateMock<IAbsenceTypesProvider>();
			absenceTypesProvider.Stub(x => x.GetRequestableAbsences()).Return(new List<IAbsence>());
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests)).
				Return(true);

			var target = new RequestsViewModelFactory(null, null, absenceTypesProvider, permissionProvider, null, null);
			var result = target.CreatePageViewModel();

			result.RequestPermission.TextRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldRetrieveAbsenceRequestPermissionForViewModel()
		{
			var absenceTypesProvider = MockRepository.GenerateMock<IAbsenceTypesProvider>();
			absenceTypesProvider.Stub(x => x.GetRequestableAbsences()).Return(new List<IAbsence>());
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb)).
				Return(true);

			var target = new RequestsViewModelFactory(null, null, absenceTypesProvider, permissionProvider, null, null);
			var result = target.CreatePageViewModel();

			result.RequestPermission.AbsenceRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldRetrievePersonRequestsForPagingViewModel()
		{
			var personRequestProvider = MockRepository.GenerateMock<IPersonRequestProvider>();
			var target = new RequestsViewModelFactory(personRequestProvider, MockRepository.GenerateMock<IMappingEngine>(), null, null, null, null);
			var paging = new Paging();
			personRequestProvider.Stub(x => x.RetrieveRequests(paging)).Return(new IPersonRequest[] { });

			target.CreatePagingViewModel(paging);

			//Henrik 20130118 When shifttrades is implemented, use x.RetrieveRequests(paging) instead
			personRequestProvider.AssertWasCalled(x => x.RetrieveTextAndAbsenceRequests(paging));
		}

		[Test]
		public void ShouldMapToViewModelForPagingViewModel()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new RequestsViewModelFactory(MockRepository.GenerateMock<IPersonRequestProvider>(), mapper, null, null, null, null);
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
			var target = new RequestsViewModelFactory(provider, mapper, null, null, null, null);
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
			var target = new RequestsViewModelFactory(null, null, null, null, provider, mapper);
			var shiftTradePeriodViewModel = new ShiftTradeRequestsPeriodViewModel();
			var workflowControlSet = new WorkflowControlSet();

			provider.Stub(p => p.RetrieveUserWorkflowControlSet()).Return(workflowControlSet);
			mapper.Stub(x => x.Map(workflowControlSet)).Return(shiftTradePeriodViewModel);

			var result = target.CreateShiftTradePeriodViewModel();

			result.Should().Be.SameInstanceAs(shiftTradePeriodViewModel);
		}

		[Test]
		public void ShouldRetrieveShiftTradeScheduleViewModel()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new RequestsViewModelFactory(null, mapper, null, null, null, null);
			var viewModel = new ShiftTradeScheduleViewModel();

			mapper.Stub(x => x.Map<DateOnly, ShiftTradeScheduleViewModel>(Arg<DateOnly>.Is.Anything)).Return(viewModel);

			var result = target.CreateShiftTradeScheduleViewModel(DateTime.Now);
			result.Should().Be.SameInstanceAs(viewModel);
		}
	}
}
