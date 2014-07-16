using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
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
			var target = new RequestsController(MockRepository.GenerateMock<IRequestsViewModelFactory>(), null, null, null, null, new FakePermissionProvider());

			var result = target.Index();

			result.ViewName.Should().Be.EqualTo("RequestsPartial");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnViewModelForIndex()
		{
			var viewModelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();
			var target = new RequestsController(viewModelFactory, null, null, null, null, new FakePermissionProvider());

			viewModelFactory.Stub(x => x.CreatePageViewModel()).Return(new RequestsViewModel());

			var result = target.Index();
			var model = result.Model as RequestsViewModel;

			model.Should().Not.Be.Null();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnViewModelForPaging()
		{
			var viewModelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();

			var target = new RequestsController(viewModelFactory, null, null, null, null, new FakePermissionProvider());
			var model = new RequestViewModel[] { };
			var paging = new Paging();

			viewModelFactory.Stub(x => x.CreatePagingViewModel(paging)).Return(model);

			var result = target.Requests(paging);

			result.Data.Should().Be.SameInstanceAs(model);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldPersistTextRequestForm()
		{
			var textRequestPersister = MockRepository.GenerateMock<ITextRequestPersister>();
			var form = new TextRequestForm();
			var resultData = new RequestViewModel();

			var target = new RequestsController(null, textRequestPersister, null, null, null, new FakePermissionProvider());

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

			using (var target = new RequestsController(null, null, null, shiftTradePersister, null, new FakePermissionProvider()))
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

			var target = new RequestsController(null, null, absenceRequestPersister, null, null, new FakePermissionProvider());

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

			var target = new RequestsController(null, null, absenceRequestPersister, null, null, new FakePermissionProvider());
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

			var target = new RequestsController(null, null, absenceRequestPersister, null, null, new FakePermissionProvider());
			var context = new FakeHttpContext("/");
			context.SetResponse(response);
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);
			target.ModelState.AddModelError("Error", "Error");

			absenceRequestPersister.Stub(x => x.Persist(form)).Throw(new InvalidOperationException());

			var result = target.AbsenceRequest(form);
			var data = result.Data as RequestViewModel;

			data.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnErrorMessageOnInvalidModelFromTextRequest()
		{
			var target = new StubbingControllerBuilder().CreateController<RequestsController>(null, null, null,null, null, null);
			const string message = "Test model validation error";
			target.ModelState.AddModelError("Test", message);

			var result = target.TextRequest(new TextRequestForm());
			var data = result.Data as ModelStateResult;

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			data.Errors.Single().Should().Be(message);

		}

		[Test]
		public void ShouldReturnErrorMessageOnInvalidModelFromShiftTradeRequest()
		{
			var target = new StubbingControllerBuilder().CreateController<RequestsController>(null, null, null, null, null, null);
			const string message = "Test model validation error";
			target.ModelState.AddModelError("Test", message);

			var result = target.ShiftTradeRequest(new ShiftTradeRequestForm());
			var data = result.Data as ModelStateResult;

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			data.Errors.Single().Should().Be(message);

		}

		[Test]
		public void ShouldDeleteTextRequest()
		{
			var textRequestPersister = MockRepository.GenerateMock<ITextRequestPersister>();
			using (var target = new RequestsController(null, textRequestPersister, null, null, null, new FakePermissionProvider()))
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
			using (var target = new RequestsController(modelFactory, null, null, null, null, new FakePermissionProvider()))
			{
				var id = Guid.NewGuid();
				var viewModel = new RequestViewModel { Dates = "a", Id = "b", Status = "c", Subject = "d", Text = "e", Type = "f", UpdatedOn = "g" };

				modelFactory.Stub(f => f.CreateRequestViewModel(id)).Return(viewModel);

				var result = target.RequestDetail(id);
				var data = (RequestViewModel)result.Data;

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
				OpenPeriodRelativeStart = 2,
				OpenPeriodRelativeEnd = 30,
				NowYear = 2013,
				NowMonth = 12,
				NowDay = 9,
			};

			modelFactory.Stub(x => x.CreateShiftTradePeriodViewModel()).Return(model);

			var target = new RequestsController(modelFactory, null, null, null, null, new FakePermissionProvider());
			var result = target.ShiftTradeRequestPeriod();
			var data = (ShiftTradeRequestsPeriodViewModel) result.Data;

			data.HasWorkflowControlSet.Should().Be.EqualTo(model.HasWorkflowControlSet);
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

			var target = new RequestsController(modelFactory, null, null, null, null, new FakePermissionProvider());
			var result = target.ShiftTradeRequestMyTeam(givenDate);

			var data = (string)result.Data;
			data.Should().Be.EqualTo(myTeamId);
		}

		[Test]
		public void ShouldGetScheduleModel()
		{
			var modelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();
			var model = new ShiftTradeScheduleViewModel();
			var pageIndex = 1;

			modelFactory.Stub(x => x.CreateShiftTradeScheduleViewModel(Arg<ShiftTradeScheduleViewModelData>.Is.Anything))
			            .Return(model);

			var target = new RequestsController(modelFactory, null, null, null, null, new FakePermissionProvider());

			var result = target.ShiftTradeRequestSchedule(DateOnly.Today, Guid.NewGuid().ToString(), new Paging(), pageIndex);
			result.Data.Should().Be.SameInstanceAs(model);
		}
		
		[Test]
		public void ShouldApproveShiftTradeRequest()
		{
			var id = Guid.NewGuid();
			var resultData = new RequestViewModel();
			var shiftTradePersister = MockRepository.GenerateStrictMock<IRespondToShiftTrade>();
			shiftTradePersister.Expect(a => a.OkByMe(id, "")).Return(resultData);

			using (var target = new RequestsController(null, null, null, null, shiftTradePersister, new FakePermissionProvider()))
			{
				var result = target.ApproveShiftTrade(new ShiftTradeRequestReplyForm(){ID = id, Message = ""});
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

			using (var target = new RequestsController(null, null, null, null, shiftTradePersister, new FakePermissionProvider()))
			{
				var result = target.DenyShiftTrade(new ShiftTradeRequestReplyForm(){ID = id, Message = ""});
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
														 From =  new ShiftTradeEditPersonScheduleViewModel
															 {
																         ScheduleLayers = new List<ShiftTradeEditScheduleLayerViewModel>(),
																			DayOffText = "DO",
																			HasUnderlyingDayOff = false,
																			MinutesSinceTimeLineStart = 60,
																			Name="xxx"
															         },
														 To =  new ShiftTradeEditPersonScheduleViewModel
															 {
																		 ScheduleLayers = new List<ShiftTradeEditScheduleLayerViewModel>(),
																		 DayOffText = "DO",
																		 HasUnderlyingDayOff = false,
																		 MinutesSinceTimeLineStart = 60,
																		 Name = "yyy"
															       },
														
				                            };

			requestViewModelFactory.Expect(r => r.CreateShiftTradeRequestSwapDetails(id)).Return(new List<ShiftTradeSwapDetailsViewModel> { shiftTradeSwapDetails });

			using (var target = new RequestsController(requestViewModelFactory, null, null, null, null, new FakePermissionProvider()))
			{
				var result = (IList<ShiftTradeSwapDetailsViewModel>) target.ShiftTradeRequestSwapDetails(id).Data;
				Assert.That(result.First().From.Name, Is.EqualTo("xxx"));				
				Assert.That(result.First().To.Name,Is.EqualTo("yyy"));
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

			using (var target = new RequestsController(null, null, null, null, shiftTradePersister, new FakePermissionProvider()))
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
			using (var target = new RequestsController(null, null, null, null, null, new FakePermissionProvider()))
			{
				var result = target.PersonalAccountPermission();
				result.Data.Should().Be.EqualTo(true);
			} 
		}

		private static void assertRequestEqual(RequestViewModel target, RequestViewModel expected)
		{
			target.Dates.Should().Be.EqualTo(expected.Dates);
			target.Id.Should().Be.EqualTo(expected.Id);
			target.Status.Should().Be.EqualTo(expected.Status);
			target.Subject.Should().Be.EqualTo(expected.Subject);
			target.Text.Should().Be.EqualTo(expected.Text);
			target.Type.Should().Be.EqualTo(expected.Type);
			target.UpdatedOn.Should().Be.EqualTo(expected.UpdatedOn);
		}
	}
}
