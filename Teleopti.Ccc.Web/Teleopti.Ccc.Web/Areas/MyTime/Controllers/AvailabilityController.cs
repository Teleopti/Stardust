using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;


namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.StudentAvailability)]
	public class AvailabilityController : Controller
	{
		private readonly IStudentAvailabilityViewModelFactory _viewModelFactory;
		private readonly IVirtualSchedulePeriodProvider _virtualSchedulePeriodProvider;
		private readonly IStudentAvailabilityPersister _studentAvailabilityPersister;

		public AvailabilityController(IStudentAvailabilityViewModelFactory viewModelFactory,
														IVirtualSchedulePeriodProvider virtualSchedulePeriodProvider,
														IStudentAvailabilityPersister studentAvailabilityPersister)
		{
			_viewModelFactory = viewModelFactory;
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_studentAvailabilityPersister = studentAvailabilityPersister;
		}

		[HttpGet]
		[EnsureInPortal]
		[UnitOfWork]
		public virtual ActionResult Index(DateOnly? dateParam)
		{
			var date = dateParam.HasValue ? dateParam.Value : _virtualSchedulePeriodProvider.CalculateStudentAvailabilityDefaultDate();

            if (_virtualSchedulePeriodProvider.MissingPersonPeriod(date))
                return View("NoPersonPeriodPartial");
			if (_virtualSchedulePeriodProvider.MissingSchedulePeriod())
				return View("NoSchedulePeriodPartial");

			var studentAvailabilityViewModel = _viewModelFactory.CreateViewModel(date);
			return View("StudentAvailabilityPartial", studentAvailabilityViewModel);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult StudentAvailabilitiesAndSchedules(DateOnly @from, DateOnly to)
		{
			return Json(_viewModelFactory.CreateStudentAvailabilityAndSchedulesViewModels(from, to), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult StudentAvailability(DateOnly date)
		{
			return Json(_viewModelFactory.CreateDayViewModel(date), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpPostOrPut]
		public virtual JsonResult StudentAvailability(StudentAvailabilityDayInput input)
		{
			return ModelState.IsValid ? 
								Json(_studentAvailabilityPersister.Persist(input)) : 
								ModelState.ToJson();
		}

		/// <summary>
		/// Deletes a student availability
		/// </summary>
		/// <param name="date">Date to delete</param>
		/// <returns>Student availability or http error code (404 for not found)</returns>
		[UnitOfWork]
		[HttpDelete]
		[ActionName("StudentAvailability")]
		public virtual JsonResult StudentAvailabilityDelete(DateOnly date)
		{
			return Json(_studentAvailabilityPersister.Delete(date));
		}
	}
}