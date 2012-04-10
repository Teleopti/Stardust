using System;
using System.Threading;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.StandardPreferences)]
	public class PreferenceController : Controller
	{
		private readonly IPreferenceViewModelFactory _viewModelFactory;
		private readonly IVirtualSchedulePeriodProvider _virtualSchedulePeriodProvider;
		private readonly IPreferencePersister _preferencePersister;

		public PreferenceController(IPreferenceViewModelFactory viewModelFactory, IVirtualSchedulePeriodProvider virtualSchedulePeriodProvider, IPreferencePersister preferencePersister)
		{
			_viewModelFactory = viewModelFactory;
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_preferencePersister = preferencePersister;
		}

		[EnsureInPortal]
		[UnitOfWorkAction]
		public ViewResult Index(DateOnly? date)
		{
			if (!_virtualSchedulePeriodProvider.HasSchedulePeriod())
				return View("NoSchedulePeriodPartial");
			if (!date.HasValue)
				date = _virtualSchedulePeriodProvider.CalculatePreferenceDefaultDate();

			return View("PreferencePartial", _viewModelFactory.CreateViewModel(date.Value));
		}

		[UnitOfWorkAction]
		[HttpPostOrPut]
		public JsonResult Preference(PreferenceDayInput input)
		{
			return Json(_preferencePersister.Persist(input));
		}

		[UnitOfWorkAction]
		[HttpDelete]
		[ActionName("Preference")]
		public JsonResult PreferenceDelete(DateOnly date)
		{
			return Json(_preferencePersister.Delete(date));
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult Feedback(DateOnly date)
		{
			return Json(_viewModelFactory.CreateDayFeedbackViewModel(date), JsonRequestBehavior.AllowGet);
		}

	}
}