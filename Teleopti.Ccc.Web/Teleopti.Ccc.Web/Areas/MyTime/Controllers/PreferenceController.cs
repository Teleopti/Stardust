using System.Web.Mvc;
using AutofacContrib.DynamicProxy2;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Core.Aop.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.StandardPreferences)]
	[Aspects]
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
			if (!_virtualSchedulePeriodProvider.HasSchedulePeriod())
				return View("NoSchedulePeriodPartial");
			if (!date.HasValue)
				date = _virtualSchedulePeriodProvider.CalculatePreferenceDefaultDate();

			return View("PreferencePartial", _viewModelFactory.CreateViewModel(date.Value));
		}

		[UnitOfWork]
		[HttpPostOrPut]
		public virtual JsonResult Preference(PreferenceDayInput input)
		{
			return Json(_preferencePersister.Persist(input));
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