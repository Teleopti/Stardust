using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class RequestsControllerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnRequestsPartialView()
		{
			var target = new RequestsController(MockRepository.GenerateMock<IRequestsViewModelFactory>(), null, null);

			var result = target.Index();

			result.ViewName.Should().Be.EqualTo("RequestsPartial");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnViewModelForIndex()
		{
			var viewModelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();
			var target = new RequestsController(viewModelFactory, null, null);

			viewModelFactory.Stub(x => x.CreatePageViewModel()).Return(new RequestsViewModel());

			var result = target.Index();
			var model = result.Model as RequestsViewModel;

			model.Should().Not.Be.Null();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnViewModelForPaging()
		{
			var viewModelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();

			var target = new RequestsController(viewModelFactory, null, null);
			var model = new RequestViewModel[]{};
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

			var target = new RequestsController(null, textRequestPersister, null);

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

			var target = new RequestsController(null, null, absenceRequestPersister);

			absenceRequestPersister.Stub(x => x.Persist(form)).Return(resultData);

			var result = target.AbsenceRequest(form);
			var data = result.Data as RequestViewModel;

			data.Should().Be.SameInstanceAs(resultData);
		}

		[Test]
		public void ShouldReturnErrorMessageOnInvalidModel()
		{
			var target = new StubbingControllerBuilder().CreateController<RequestsController>(null, null, null);
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
			using (var target = new RequestsController(null, textRequestPersister, null))
			{
				var id = Guid.NewGuid();

				target.TextRequestDelete(id);

				textRequestPersister.AssertWasCalled(x => x.Delete(id));
			}
		}

		[Test]
		public void ShouldGetRequestById()
		{
			var modelFactory = MockRepository.GenerateStub<IRequestsViewModelFactory>();
			using (var target = new RequestsController(modelFactory, null, null))
			{
				var id = Guid.NewGuid();
				var viewModel = new RequestViewModel { Dates = "a", Id = "b", Status = "c", Subject = "d", Text = "e", Type = "f", UpdatedOn = "g" };

				modelFactory.Stub(f => f.CreateRequestViewModel(id)).Return(viewModel);

				var result = target.TextRequest(id);
				var data = (RequestViewModel)result.Data;

				assertRequestEqual(data, viewModel);	
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
