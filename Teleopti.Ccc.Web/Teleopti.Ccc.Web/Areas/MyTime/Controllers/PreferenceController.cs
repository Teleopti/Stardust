using System.Net;
using System.Web.Mvc;
using Autofac.Extras.DynamicProxy2;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Core.Aop.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[PreferencePermission]
	[Intercept(typeof(AspectInterceptor))]
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
		[UnitOfWork]
		public virtual ViewResult Index(DateOnly? date)
		{
			if (_virtualSchedulePeriodProvider.MissingPersonPeriod())
				return View("NoPersonPeriodPartial");
			if (_virtualSchedulePeriodProvider.MissingSchedulePeriod())
				return View("NoSchedulePeriodPartial");
			if (!date.HasValue)
				date = _virtualSchedulePeriodProvider.CalculatePreferenceDefaultDate();

			return View("PreferencePartial", _viewModelFactory.CreateViewModel(date.Value));
		}

		[HttpGet]
		[UnitOfWork]
		public virtual JsonResult PreferencesAndSchedules(DateOnly @from, DateOnly to)
		{
			return Json(_viewModelFactory.CreatePreferencesAndSchedulesViewModel(from, to), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		[ActionName("Preference")]
		public virtual JsonResult GetPreference(DateOnly date)
		{
			var model = _viewModelFactory.CreateDayViewModel(date);
			if (model==null)
			{
				Response.StatusCode = (int) HttpStatusCode.NoContent;
				return null;
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpPostOrPut]
		public virtual JsonResult Preference(PreferenceDayInput input)
		{
			if (!ModelState.IsValid)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return ModelState.ToJson();
			}
			return Json(_preferencePersister.Persist(input));
		}

		[UnitOfWork]
		[HttpPostOrPut]
		public virtual JsonResult MustHave(MustHaveInput input)
		{
			return Json(_preferencePersister.MustHave(input));
		}

		[UnitOfWork]
		[HttpDelete]
		[ActionName("Preference")]
		public virtual JsonResult PreferenceDelete(DateOnly date)
		{
			return Json(_preferencePersister.Delete(date));
		}

	}
}