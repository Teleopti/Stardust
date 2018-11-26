using System.Threading.Tasks;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	//[StudentAvailabilityPermission]
	public class StudentAvailabilityFeedbackController : AsyncController
	{
		private readonly IStudentAvailabilityViewModelFactory _viewModelFactory;
		private readonly IStudentAvailabilityPeriodFeedbackViewModelFactory _studentAvailabilityPeriodFeedbackViewModelFactory;

		public StudentAvailabilityFeedbackController(IStudentAvailabilityViewModelFactory viewModelFactory, IStudentAvailabilityPeriodFeedbackViewModelFactory studentAvailabilityPeriodFeedbackViewModelFactory)
		{
			_viewModelFactory = viewModelFactory;
			_studentAvailabilityPeriodFeedbackViewModelFactory = studentAvailabilityPeriodFeedbackViewModelFactory;
		}

		[HttpGet]
		[AsyncTask]
		public void FeedbackAsync(DateOnly date) { }

		[UnitOfWork]
		public virtual void FeedbackTask(DateOnly date)
		{
			AsyncManager.Parameters["model"] = _viewModelFactory.CreateDayFeedbackViewModel(date);
		}

		public JsonResult FeedbackCompleted(StudentAvailabilityDayFeedbackViewModel model, Task task)
		{
			task.GetAwaiter().GetResult();
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[AsyncTask]
		public void PeriodFeedbackAsync(DateOnly date) { }

		[UnitOfWork]
		public virtual void PeriodFeedbackTask(DateOnly date)
		{
			AsyncManager.Parameters["model"] = _studentAvailabilityPeriodFeedbackViewModelFactory.CreatePeriodFeedbackViewModel(date);
		}

		public JsonResult PeriodFeedbackCompleted(StudentAvailabilityPeriodFeedbackViewModel model, Task task)
		{
			task.GetAwaiter().GetResult();
			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}
}