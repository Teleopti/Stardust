using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Controllers
{
	[TestFixture]
	public class StudentAvailabilityControllerTest
	{
		[Test]
		public void ShouldReturnStudentAvailabilityPartialView()
		{
			var viewModelFactory = MockRepository.GenerateMock<IStudentAvailabilityViewModelFactory>();
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var target = new StudentAvailabilityController(viewModelFactory, virtualSchedulePeriodProvider, null);

			virtualSchedulePeriodProvider.Stub(x => x.HasSchedulePeriod()).Return(true);
			viewModelFactory.Stub(x => x.CreateViewModel(DateOnly.Today)).Return(new StudentAvailabilityViewModel());

			var result = target.Index(DateOnly.Today) as ViewResult;
			var model = result.Model as StudentAvailabilityViewModel;

			result.ViewName.Should().Be.EqualTo("StudentAvailabilityPartial");
			model.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldUseDefaultDateWhenNotSpecified()
		{
			var viewModelFactory = MockRepository.GenerateMock<IStudentAvailabilityViewModelFactory>();
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var target = new StudentAvailabilityController(viewModelFactory, virtualSchedulePeriodProvider, null);
			var defaultDate = DateOnly.Today.AddDays(23);

			virtualSchedulePeriodProvider.Stub(x => x.HasSchedulePeriod()).Return(true);
			virtualSchedulePeriodProvider.Stub(x => x.CalculateStudentAvailabilityDefaultDate()).Return(defaultDate);

			target.Index(null);

			viewModelFactory.AssertWasCalled(x => x.CreateViewModel(defaultDate));
		}

		[Test]
		public void ShouldReturnNoSchedulePeriodPartialWhenNoSchedulePeriod()
		{
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var target = new StudentAvailabilityController(null, virtualSchedulePeriodProvider, null);

			virtualSchedulePeriodProvider.Stub(x => x.HasSchedulePeriod()).Return(false);

			var result = target.Index(null) as ViewResult;

			result.ViewName.Should().Be.EqualTo("NoSchedulePeriodPartial");
		}

		[Test]
		public void ShouldReturnStudentAvailabilityForDate()
		{
			var viewModelFactory = MockRepository.GenerateMock<IStudentAvailabilityViewModelFactory>();

			var target = new StudentAvailabilityController(viewModelFactory, null, null);
			var model = new StudentAvailabilityDayViewModel();

			viewModelFactory.Stub(x => x.CreateDayViewModel(DateOnly.Today)).Return(model);

			var result = target.StudentAvailability(DateOnly.Today) as JsonResult;

			result.Data.Should().Be.SameInstanceAs(model);
		}

		[Test]
		public void ShouldPersistStudentAvailabilityForm()
		{ 
			var studentAvailabilityPersister = MockRepository.GenerateMock<IStudentAvailabilityPersister>();
			var form = new StudentAvailabilityDayForm();
			var resultData = new StudentAvailabilityDayFormResult();

			var target = new StudentAvailabilityController(null, null, studentAvailabilityPersister);

			studentAvailabilityPersister.Stub(x => x.Persist(form)).Return(resultData);

			var result = target.StudentAvailability(form) as JsonResult;
			var data = result.Data as StudentAvailabilityDayFormResult;

			data.Should().Be.SameInstanceAs(resultData);
		}

		[Test]
		public void ShouldReturnErrorMessageOnInvalidModel()
		{
			var target = new StudentAvailabilityController(null, null, null);
			const string message = "Test model validation error";

			target.ModelState.AddModelError("Test", message);

			var result = target.StudentAvailability(new StudentAvailabilityDayForm()) as JsonResult;
			var data = result.Data as ModelStateResult;

			Assert.That(data.Errors.Single(), Is.EqualTo(message));
		}

		[Test]
		public void ShouldDeleteStudentAvailability()
		{
			var studentAvailabilityPersister = MockRepository.GenerateMock<IStudentAvailabilityPersister>();
			var target = new StudentAvailabilityController(null, null, studentAvailabilityPersister);
			var resultData = new StudentAvailabilityDayFormResult();

			studentAvailabilityPersister.Stub(x => x.Delete(DateOnly.Today)).Return(resultData);

			var result = target.StudentAvailabilityDelete(DateOnly.Today) as JsonResult;
			var data = result.Data as StudentAvailabilityDayFormResult;

			data.Should().Be.SameInstanceAs(resultData);
		}
	}
}