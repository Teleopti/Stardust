using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
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
			var target = new RequestsController(MockRepository.GenerateMock<IRequestsViewModelFactory>(), null, null, null);

			var result = target.Index();

			result.ViewName.Should().Be.EqualTo("RequestsPartial");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnViewModelForIndex()
		{
			var viewModelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();
			var target = new RequestsController(viewModelFactory, null, null, null);

			viewModelFactory.Stub(x => x.CreatePageViewModel()).Return(new RequestsViewModel());

			var result = target.Index();
			var model = result.Model as RequestsViewModel;

			model.Should().Not.Be.Null();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnViewModelForPaging()
		{
			var viewModelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();

			var target = new RequestsController(viewModelFactory, null, null, null);
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

			var target = new RequestsController(null, textRequestPersister, null, null);

			textRequestPersister.Stub(x => x.Persist(form)).Return(resultData);

			var result = target.TextRequest(form);
			var data = result.Data as RequestViewModel;

			data.Should().Be.SameInstanceAs(resultData);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldPersistAbsenceRequestForm()
		{
			var absenceRequestPersister = MockRepository.GenerateMock<IAbsenceRequestPersister>();
			var form = new AbsenceRequestForm();
			var resultData = new RequestViewModel();

			var target = new RequestsController(null, null, absenceRequestPersister, null);

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

			var target = new RequestsController(null, null, absenceRequestPersister, null);
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

			var target = new RequestsController(null, null, absenceRequestPersister, null);
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
		public void ShouldReturnErrorMessageOnInvalidModel()
		{
			var target = new StubbingControllerBuilder().CreateController<RequestsController>(null, null, null,null);
			const string message = "Test model validation error";
			target.ModelState.AddModelError("Test", message);

			var result = target.TextRequest(new TextRequestForm());
			var data = result.Data as ModelStateResult;

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			data.Errors.Single().Should().Be(message);

		}

		[Test]
		public void ShouldDeleteTextRequest()
		{
			var textRequestPersister = MockRepository.GenerateMock<ITextRequestPersister>();
			using (var target = new RequestsController(null, textRequestPersister, null, null))
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
			using (var target = new RequestsController(modelFactory, null, null, null))
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
				OpenPeriodRelativeEnd = 30
			};

			modelFactory.Stub(x => x.CreateShiftTradePeriodViewModel()).Return(model);

			var target = new RequestsController(modelFactory, null, null);
			var result = target.ShiftTradeRequestPeriod();
			var data = (ShiftTradeRequestsPeriodViewModel) result.Data;

			data.HasWorkflowControlSet.Should().Be.EqualTo(model.HasWorkflowControlSet);
			data.OpenPeriodRelativeStart.Should().Be.EqualTo(model.OpenPeriodRelativeStart);
			data.OpenPeriodRelativeEnd.Should().Be.EqualTo(model.OpenPeriodRelativeEnd);
		}

		[Test]
		public void ShouldGetLayersForMySchedule()
		{
			var modelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();
			var layer = new ShiftTradeScheduleLayerViewModel
			            	{
			            		Payload = "phone",
			            		Color = "green",
			            		StartTimeText = "9:00",
			            		EndTimeText = "15:00",
			            		LengthInMinutes = 360,
			            		ElapsedMinutesSinceShiftStart = 30
			            	};
			var model = new ShiftTradeRequestsScheduleViewModel
							{
								MySchedule = new ShiftTradeScheduleViewModel { ScheduleLayers = new List<ShiftTradeScheduleLayerViewModel> { layer } }
							};

			modelFactory.Stub(x => x.CreateShiftTradeScheduleViewModel(Arg<DateTime>.Is.Anything)).Return(model);

			var target = new RequestsController(modelFactory, null, null);
			
			var result = target.ShiftTradeRequestSchedule(DateTime.Now);
			var scheduleViewModel = (ShiftTradeRequestsScheduleViewModel) result.Data;

			var createdLayer = scheduleViewModel.MySchedule.ScheduleLayers.FirstOrDefault();
			createdLayer.Payload.Should().Be.EqualTo(layer.Payload);
			createdLayer.Color.Should().Be.EqualTo(layer.Color);
			createdLayer.StartTimeText.Should().Be.EqualTo(layer.StartTimeText);
			createdLayer.EndTimeText.Should().Be.EqualTo(layer.EndTimeText);
			createdLayer.LengthInMinutes.Should().Be.EqualTo(layer.LengthInMinutes);
			createdLayer.ElapsedMinutesSinceShiftStart.Should().Be.EqualTo(layer.ElapsedMinutesSinceShiftStart);

		}
		
		[Test]
		public void ApproveShiftTradeRequest()
		{

			var id = Guid.NewGuid();
			var shiftTradePersister = MockRepository.GenerateStrictMock<IShiftTradeResponseService>();
			shiftTradePersister.Expect(a => a.OkByMe(id)).Repeat.Once();

			using (var target = new RequestsController(null, null, null, shiftTradePersister))
			{
				target.ApproveShiftTrade(id);
			}

			shiftTradePersister.VerifyAllExpectations();
		}

		[Test]
		public void RejectShiftTradeRequest()
		{
			var id = Guid.NewGuid();
			var shiftTradePersister = MockRepository.GenerateStrictMock<IShiftTradeResponseService>();
			shiftTradePersister.Expect(a => a.Reject(id)).Repeat.Once();

			using (var target = new RequestsController(null, null, null, shiftTradePersister))
			{
				target.RejectShiftTrade(id);
			}

			shiftTradePersister.VerifyAllExpectations();


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
