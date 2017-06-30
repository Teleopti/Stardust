using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.CommandProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class RequestsControllerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnRequestsPartialView()
		{
			var target = new RequestsController(MockRepository.GenerateMock<IRequestsViewModelFactory>(), null, null, null, null,
				new FakePermissionProvider(), null, null, null, null, null, null);

			var result = target.Index();

			result.ViewName.Should().Be.EqualTo("RequestsPartial");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnViewModelForIndex()
		{
			var viewModelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();
			var target = new RequestsController(viewModelFactory, null, null, null, null, new FakePermissionProvider(), null,
				null, null, null, null, null);

			viewModelFactory.Stub(x => x.CreatePageViewModel()).Return(new RequestsViewModel());

			var result = target.Index();
			var model = result.Model as RequestsViewModel;

			model.Should().Not.Be.Null();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnViewModelForPaging()
		{
			var viewModelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();

			var target = new RequestsController(viewModelFactory, null, null, null, null,
				new FakePermissionProvider(), null, null, null, null, null, null);
			var model = new RequestViewModel[] {};
			var paging = new Paging();
			var filter = new RequestListFilter() {HideOldRequest = false, IsSortByUpdateDate = true};

			viewModelFactory.Stub(x => x.CreatePagingViewModel(paging, filter)).Return(model);

			var result = target.Requests(paging, filter);

			result.Data.Should().Be.SameInstanceAs(model);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnViewModelAfterSpecificDateOnly()
		{
			var viewModelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();

			var target = new RequestsController(viewModelFactory, null, null, null, null,
				new FakePermissionProvider(), null, null, null, null, null, null);
			var model = new RequestViewModel[] {};
			var paging = new Paging();
			var filter = new RequestListFilter() {HideOldRequest = false, IsSortByUpdateDate = true};

			viewModelFactory.Stub(x => x.CreatePagingViewModel(paging, filter)).Return(model);

			var result = target.Requests(paging, filter);

			result.Data.Should().Be.SameInstanceAs(model);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldPersistTextRequestForm()
		{
			var textRequestPersister = MockRepository.GenerateMock<ITextRequestPersister>();
			var form = new TextRequestForm();
			var resultData = new RequestViewModel();

			var target = new RequestsController(null, textRequestPersister, null, null, null, new FakePermissionProvider(), null,
				null, null, null, null, null);

			textRequestPersister.Stub(x => x.Persist(form)).Return(resultData);

			var result = target.TextRequest(form);
			var data = result.Data as RequestViewModel;

			data.Should().Be.SameInstanceAs(resultData);
		}

		[Test]
		public void ShouldPersistShiftTradeRequest()
		{
			var shiftTradePersister = MockRepository.GenerateMock<IShiftTradeRequestPersister>();
			var form = new ShiftTradeRequestForm();
			var resultData = new RequestViewModel();

			shiftTradePersister.Stub(x => x.Persist(form)).Return(resultData);

			using (var target = new RequestsController(null, null, null, shiftTradePersister, null, new FakePermissionProvider(), null,
					null, null, null, null, null))
			{
				var result = target.ShiftTradeRequest(form);
				var data = result.Data as RequestViewModel;
				data.Should().Be.SameInstanceAs(resultData);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldPersistAbsenceRequestForm()
		{
			var absenceRequestPersister = MockRepository.GenerateMock<IAbsenceRequestPersister>();
			var form = new AbsenceRequestForm();
			var resultData = new RequestViewModel();

			var target = new RequestsController(null, null, absenceRequestPersister, null, null, new FakePermissionProvider(),
				null, null, null, null, null, null);

			absenceRequestPersister.Stub(x => x.Persist(form)).Return(resultData);

			var result = target.AbsenceRequest(form);
			var data = result.Data as RequestViewModel;

			data.Should().Be.SameInstanceAs(resultData);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnErrorFromAbsenceRequestFormWhenPersistError()
		{
			var absenceRequestPersister = MockRepository.GenerateMock<IAbsenceRequestPersister>();
			var response = MockRepository.GenerateStub<FakeHttpResponse>();
			var form = new AbsenceRequestForm();

			var target = new RequestsController(null, null, absenceRequestPersister, null, null, new FakePermissionProvider(),
				null, null, null, null, null, null);
			var context = new FakeHttpContext("/");
			context.SetResponse(response);
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);

			absenceRequestPersister.Stub(x => x.Persist(form)).Throw(new InvalidOperationException());

			var result = target.AbsenceRequest(form);
			var data = result.Data as RequestViewModel;

			data.Should().Be.Null();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnErrorFromAbsenceRequestFormWhenModelError()
		{
			var absenceRequestPersister = MockRepository.GenerateMock<IAbsenceRequestPersister>();
			var response = MockRepository.GenerateStub<FakeHttpResponse>();
			var form = new AbsenceRequestForm();

			var target = new RequestsController(null, null, absenceRequestPersister, null, null, new FakePermissionProvider(),
				null, null, null, null, null, null);
			var context = new FakeHttpContext("/");
			context.SetResponse(response);
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);
			target.ModelState.AddModelError("Error", @"Error");

			absenceRequestPersister.Stub(x => x.Persist(form)).Throw(new InvalidOperationException());

			var result = target.AbsenceRequest(form);
			var data = result.Data as RequestViewModel;

			data.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnErrorMessageOnInvalidModelFromTextRequest()
		{
			var target = new StubbingControllerBuilder().CreateController<RequestsController>(null, null, null, null, null, null,
				null, null, null, null, null, null);
			const string message = "Test model validation error";
			target.ModelState.AddModelError("Test", message);

			var result = target.TextRequest(new TextRequestForm());
			var data = (ModelStateResult) result.Data;

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			data.Errors.Single().Should().Be(message);
		}

		[Test]
		public void ShouldReturnErrorMessageOnInvalidModelFromShiftTradeRequest()
		{
			var target = new StubbingControllerBuilder().CreateController<RequestsController>(null, null, null, null, null, null,
				null, null, null, null, null, null);
			const string message = "Test model validation error";
			target.ModelState.AddModelError("Test", message);

			var result = target.ShiftTradeRequest(new ShiftTradeRequestForm());
			var data = (ModelStateResult) result.Data;

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			data.Errors.Single().Should().Be(message);
		}

		[Test]
		public void ShouldDeleteTextRequest()
		{
			var textRequestPersister = MockRepository.GenerateMock<ITextRequestPersister>();
			using (var target = new RequestsController(null, textRequestPersister, null, null, null, new FakePermissionProvider(), null,
					null, null, null, null, null))
			{
				var id = Guid.NewGuid();

				target.RequestDelete(id);

				textRequestPersister.AssertWasCalled(x => x.Delete(id));
			}
		}

		[Test]
		public void ShouldGetRequestById()
		{
			var modelFactory = MockRepository.GenerateStub<IRequestsViewModelFactory>();
			using (var target = new RequestsController(modelFactory, null, null, null, null, new FakePermissionProvider(), null, null,
					null, null, null, null))
			{
				var id = Guid.NewGuid();
				var viewModel = new RequestViewModel
				{
					Id = "b",
					Status = "c",
					Subject = "d",
					Text = "e",
					Type = "f",
					UpdatedOnDateTime = "g"
				};

				modelFactory.Stub(f => f.CreateRequestViewModel(id)).Return(viewModel);

				var result = target.RequestDetail(id);
				var data = (RequestViewModel) result.Data;

				assertRequestEqual(data, viewModel);
			}
		}

		[Test]
		public void ShouldGetShiftTradePeriodInformation()
		{
			var modelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();
			var model = new ShiftTradeRequestsPeriodViewModel
			{
				HasWorkflowControlSet = true,
				MiscSetting = new ShiftTradeRequestMiscSetting() {AnonymousTrading = true},
				OpenPeriodRelativeStart = 2,
				OpenPeriodRelativeEnd = 30,
				NowYear = 2013,
				NowMonth = 12,
				NowDay = 9,
			};

			modelFactory.Stub(x => x.CreateShiftTradePeriodViewModel()).Return(model);

			var target = new RequestsController(modelFactory, null, null, null, null, new FakePermissionProvider(), null, null,
				null, null, null, null);
			var result = target.ShiftTradeRequestPeriod();
			var data = (ShiftTradeRequestsPeriodViewModel) result.Data;

			data.HasWorkflowControlSet.Should().Be.EqualTo(model.HasWorkflowControlSet);
			data.MiscSetting.AnonymousTrading.Should().Be.EqualTo(model.MiscSetting.AnonymousTrading);
			data.OpenPeriodRelativeStart.Should().Be.EqualTo(model.OpenPeriodRelativeStart);
			data.OpenPeriodRelativeEnd.Should().Be.EqualTo(model.OpenPeriodRelativeEnd);
			data.NowYear = model.NowYear;
			data.NowMonth = model.NowMonth;
			data.NowDay = model.NowDay;
		}

		[Test]
		public void ShouldGetIdOfTeamIBelongTo()
		{
			var givenDate = DateOnly.Today;
			var modelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();
			var myTeamId = Guid.NewGuid().ToString();

			modelFactory.Stub(x => x.CreateShiftTradeMyTeamSimpleViewModel(givenDate)).Return(myTeamId);

			var target = new RequestsController(modelFactory, null, null, null, null, new FakePermissionProvider(), null, null,
				null, null, null, null);
			var result = target.ShiftTradeRequestMyTeam(givenDate);

			var data = (string) result.Data;
			data.Should().Be.EqualTo(myTeamId);
		}

		[Test]
		public void ShouldGetIdOfSiteIBelongTo()
		{
			var givenDate = DateOnly.Today;
			var modelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();
			var mySiteId = Guid.NewGuid().ToString();

			modelFactory.Stub(x => x.CreateShiftTradeMySiteIdViewModel(givenDate)).Return(mySiteId);

			var target = new RequestsController(modelFactory, null, null, null, null, new FakePermissionProvider(), null, null,
				null, null, null, null);
			var result = target.ShiftTradeRequestMySite(givenDate);

			var data = (string) result.Data;
			data.Should().Be.EqualTo(mySiteId);
		}

		[Test]
		public void ShouldApproveShiftTradeRequest()
		{
			var id = Guid.NewGuid();
			var resultData = new RequestViewModel();
			var shiftTradePersister = MockRepository.GenerateStrictMock<IRespondToShiftTrade>();
			shiftTradePersister.Expect(a => a.OkByMe(id, "")).Return(resultData);

			using (var target = new RequestsController(null, null, null, null, shiftTradePersister, new FakePermissionProvider(), null,
					null, null, null, null, null))
			{
				var result = target.ApproveShiftTrade(new ShiftTradeRequestReplyForm {ID = id, Message = ""});
				var data = result.Data as RequestViewModel;
				data.Should().Be.SameInstanceAs(resultData);
			}

			shiftTradePersister.VerifyAllExpectations();
		}

		[Test]
		public void ShouldDenyShiftTradeRequest()
		{
			var id = Guid.NewGuid();
			var resultData = new RequestViewModel();
			var shiftTradePersister = MockRepository.GenerateStrictMock<IRespondToShiftTrade>();
			shiftTradePersister.Expect(a => a.Deny(id, "")).Return(resultData);

			using (var target = new RequestsController(null, null, null, null, shiftTradePersister, new FakePermissionProvider(), null,
					null, null, null, null, null))
			{
				var result = target.DenyShiftTrade(new ShiftTradeRequestReplyForm() {ID = id, Message = ""});
				var data = result.Data as RequestViewModel;
				data.Should().Be.SameInstanceAs(resultData);
			}

			shiftTradePersister.VerifyAllExpectations();
		}

		[Test]
		public void ShouldGetShiftTradeDetails()
		{
			var id = Guid.NewGuid();
			var requestViewModelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();
			var shiftTradeSwapDetails = new ShiftTradeSwapDetailsViewModel
			{
				From = new ShiftTradeEditPersonScheduleViewModel
				{
					ScheduleLayers = new List<ShiftTradeEditScheduleLayerViewModel>(),
					DayOffText = "DO",
					HasUnderlyingDayOff = false,
					MinutesSinceTimeLineStart = 60,
					Name = "xxx"
				},
				To = new ShiftTradeEditPersonScheduleViewModel
				{
					ScheduleLayers = new List<ShiftTradeEditScheduleLayerViewModel>(),
					DayOffText = "DO",
					HasUnderlyingDayOff = false,
					MinutesSinceTimeLineStart = 60,
					Name = "yyy"
				}
			};

			requestViewModelFactory.Expect(r => r.CreateShiftTradeRequestSwapDetails(id))
				.Return(new List<ShiftTradeSwapDetailsViewModel> {shiftTradeSwapDetails});

			using (var target = new RequestsController(requestViewModelFactory, null, null, null, null, new FakePermissionProvider(),
					null, null, null, null, null, null))
			{
				var result = (IList<ShiftTradeSwapDetailsViewModel>) target.ShiftTradeRequestSwapDetails(id).Data;
				Assert.That(result.First().From.Name, Is.EqualTo("xxx"));
				Assert.That(result.First().To.Name, Is.EqualTo("yyy"));
			}

			requestViewModelFactory.VerifyAllExpectations();
		}

		[Test]
		public void ReSendShiftTrade_WhenStatusIsReffered_ShouldSetTheShiftTradeStatusToOkByMe()
		{
			var id = Guid.NewGuid();
			var resultData = new RequestViewModel();
			var shiftTradePersister = MockRepository.GenerateStrictMock<IRespondToShiftTrade>();
			shiftTradePersister.Expect(a => a.ResendReferred(id)).Return(resultData);

			using (var target = new RequestsController(null, null, null, null, shiftTradePersister, new FakePermissionProvider(), null,
					null, null, null, null, null))
			{
				var result = target.ResendShiftTrade(id);
				var data = result.Data as RequestViewModel;
				data.Should().Be.SameInstanceAs(resultData);
			}

			shiftTradePersister.VerifyAllExpectations();
		}

		[Test]
		public void ShouldReturnPersonalAccountPermission()
		{
			using (var target = new RequestsController(null, null, null, null, null, new FakePermissionProvider(), null, null, null,
					null, null, null))
			{
				var result = target.PersonalAccountPermission();
				result.Data.Should().Be.EqualTo(true);
			}
		}

		[Test]
		public void ShouldGetAnonymousTrading()
		{
			var modelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();
			var model = new ShiftTradeRequestsPeriodViewModel
			{
				MiscSetting = new ShiftTradeRequestMiscSetting {AnonymousTrading = true}
			};

			modelFactory.Stub(x => x.CreateShiftTradePeriodViewModel(Guid.Empty)).IgnoreArguments().Return(model);

			var target = new RequestsController(modelFactory, null, null, null, null, new FakePermissionProvider(), null, null,
				null, null, null, null);

			var result = target.GetShiftTradeRequestMiscSetting(Guid.Empty);
			var data = (ShiftTradeRequestMiscSetting) result.Data;
			data.AnonymousTrading.Should().Be.True();
		}

		private static void assertRequestEqual(RequestViewModel target, RequestViewModel expected)
		{
			target.Id.Should().Be.EqualTo(expected.Id);
			target.Status.Should().Be.EqualTo(expected.Status);
			target.Subject.Should().Be.EqualTo(expected.Subject);
			target.Text.Should().Be.EqualTo(expected.Text);
			target.Type.Should().Be.EqualTo(expected.Type);
			target.UpdatedOnDateTime.Should().Be.EqualTo(expected.UpdatedOnDateTime);
			target.DateTimeFrom.Should().Be.EqualTo(expected.DateTimeFrom);
			target.DateTimeTo.Should().Be.EqualTo(expected.DateTimeTo);
		}

		[Test]
		public void ShouldAllowCancelAbsenceRequest()
		{
			var person = PersonFactory.CreatePerson("Bill", "Bloggins").WithId();
			var data = doCancelAbsenceRequestMyTimeSpecificValidation(person, new DateTimePeriod(2016, 03, 02, 2016, 03, 03));
			data.ErrorMessages.Should().Be.Empty();
		}

		[Test]
		public void ShouldValidatePermissionForCancelAbsenceRequest()
		{
			var person = PersonFactory.CreatePerson("Bill", "Bloggins").WithId();
			var data = doCancelAbsenceRequestMyTimeSpecificValidation(person, new DateTimePeriod(2016, 03, 02, 2016, 03, 03),
				false);
			data.ErrorMessages.Should().Contain(Resources.InsufficientPermission);
		}

		[Test]
		public void ShouldValidateCancellationThresholdForCancelAbsenceRequest()
		{
			setupStateHolderProxy();

			var person = PersonFactory.CreatePerson("Bill", "Bloggins").WithId();
			var workflowControlSet = new WorkflowControlSet("WorkflowControlSet")
			{
				AbsenceRequestCancellationThreshold = 10
			};
			person.WorkflowControlSet = workflowControlSet;

			var today = DateTime.Today.ToUniversalTime();
			var data = doCancelAbsenceRequestMyTimeSpecificValidation(person, new DateTimePeriod(today, today.AddDays(1)));

			data.ErrorMessages.Should()
				.Contain(string.Format(Resources.AbsenceRequestCancellationThresholdExceeded,
					workflowControlSet.AbsenceRequestCancellationThreshold));
		}

		[Test]
		public void ShouldHandleNullTeamIdListInFilter()
		{
			shouldHandleTeamIdsCorrectly(null);
		}

		[Test]
		public void ShouldHandleInvalidGuidInTeamIdListInFilter()
		{
			var teamIds = $"abc, {Guid.NewGuid()},{Guid.NewGuid()} , {Guid.NewGuid()} ,123";
			shouldHandleTeamIdsCorrectly(teamIds);
		}

		private static void shouldHandleTeamIdsCorrectly(string teamIds)
		{
			var timeFilterHelper = MockRepository.GenerateMock<ITimeFilterHelper>();
			timeFilterHelper.Stub(x => x.GetFilter(new DateOnly(), "", "", false, false))
				.IgnoreArguments().Return(null);

			var viewModelFactory = MockRepository.GenerateMock<IRequestsShiftTradeScheduleViewModelFactory>();
			viewModelFactory.Stub(x => x.CreateViewModel(new ShiftTradeScheduleViewModelData()))
				.IgnoreArguments().Return(new ShiftTradeScheduleViewModel());

			var toggleManager = new TrueToggleManager();

			using (var target = new RequestsController(null, null, null, null, null, null,
				timeFilterHelper, toggleManager, viewModelFactory, null, null, null))
			{
				var filter = new ScheduleFilter
				{
					TeamIds = teamIds
				};

				var result = target.ShiftTradeRequestSchedule(new DateOnly(DateTime.Now), filter,
					new Paging {Skip = 0, Take = 1, TotalCount = 10});

				result.Should().Not.Be.Null();
			}
		}

		private static RequestCommandHandlingResult doCancelAbsenceRequestMyTimeSpecificValidation(IPerson person, DateTimePeriod period, bool hasPermission = true)
		{
			var fakePersonRequestRepository = new FakePersonRequestRepository();
			var modelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();
			var resultData = new RequestViewModel();
			modelFactory.Stub(x => x.CreateRequestViewModel(Guid.Empty)).IgnoreArguments().Return(resultData);

			var absenceRequest = new AbsenceRequest(AbsenceFactory.CreateAbsence("Holiday").WithId(), period);
			var personRequest = new PersonRequest(person, absenceRequest).WithId();

			fakePersonRequestRepository.Add(personRequest);

			var permissionProvider = new FakePermissionProvider();
			if (hasPermission)
			{
				permissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MyTimeCancelRequest, new DateOnly(personRequest.RequestedDate), person);
			}

			var cancelAbsenceRequestCommandHandler = MockRepository.GenerateMock<IHandleCommand<CancelAbsenceRequestCommand>>();
			cancelAbsenceRequestCommandHandler.Stub(x => x.Handle(null)).IgnoreArguments();

			var requestsController = new RequestsController(modelFactory, null, null, null, null, permissionProvider, null, null,
				null, null,
				new CancelAbsenceRequestCommandProvider(cancelAbsenceRequestCommandHandler, fakePersonRequestRepository,
					permissionProvider), null);

			var result = requestsController.CancelRequest(personRequest.Id.GetValueOrDefault());
			var data = (RequestCommandHandlingResult)result.Data;
			return data;
		}

		private static void setupStateHolderProxy()
		{
			var stateMock = new FakeState();
			var dataSource = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);
			var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
			StateHolderProxyHelper.CreateSessionData(loggedOnPerson, dataSource, BusinessUnitFactory.BusinessUnitUsedInTest);
			StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);
		}
	}
}