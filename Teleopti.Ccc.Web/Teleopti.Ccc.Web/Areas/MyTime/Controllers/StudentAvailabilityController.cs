﻿using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.StudentAvailability)]
	public class StudentAvailabilityController : Controller
	{
		private readonly IStudentAvailabilityViewModelFactory _viewModelFactory;
		private readonly IVirtualSchedulePeriodProvider _virtualSchedulePeriodProvider;
		private readonly IStudentAvailabilityPersister _studentAvailabilityPersister;

		public StudentAvailabilityController(IStudentAvailabilityViewModelFactory viewModelFactory,
														IVirtualSchedulePeriodProvider virtualSchedulePeriodProvider,
														IStudentAvailabilityPersister studentAvailabilityPersister)
		{
			_viewModelFactory = viewModelFactory;
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_studentAvailabilityPersister = studentAvailabilityPersister;
		}

		[EnsureInPortal]
		[UnitOfWorkAction]
		public ActionResult Index(DateOnly? dateParam)
		{
			if (_virtualSchedulePeriodProvider.MissingSchedulePeriod())
				return View("NoSchedulePeriodPartial");
			var date = dateParam.HasValue ? dateParam.Value : _virtualSchedulePeriodProvider.CalculateStudentAvailabilityDefaultDate();
			var studentAvailabilityViewModel = _viewModelFactory.CreateViewModel(date);
			return View("StudentAvailabilityPartial", studentAvailabilityViewModel);
		}

		[UnitOfWorkAction]
		[HttpGet]
		public ActionResult StudentAvailability(DateOnly date)
		{
			return Json(_viewModelFactory.CreateDayViewModel(date), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpPostOrPut]
		public ActionResult StudentAvailability(StudentAvailabilityDayForm form)
		{
			return ModelState.IsValid ? 
								Json(_studentAvailabilityPersister.Persist(form)) : 
								ModelState.ToJson();
		}

		/// <summary>
		/// Deletes a student availability
		/// </summary>
		/// <param name="date">Date to delete</param>
		/// <returns>Student availability or http error code (404 for not found)</returns>
		[UnitOfWorkAction]
		[HttpDelete]
		[ActionName("StudentAvailability")]
		public ActionResult StudentAvailabilityDelete(DateOnly date)
		{
			return Json(_studentAvailabilityPersister.Delete(date));
		}
	}
}