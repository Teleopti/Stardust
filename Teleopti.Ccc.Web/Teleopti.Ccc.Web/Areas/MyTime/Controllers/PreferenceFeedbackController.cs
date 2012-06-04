using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
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
	public class PreferenceFeedbackController : AsyncController
	{
		private readonly IPreferenceViewModelFactory _viewModelFactory;
		private readonly IPreferencePeriodViewModelFactory _preferencePeriodViewModelFactory;

		public PreferenceFeedbackController(IPreferenceViewModelFactory viewModelFactory, IPreferencePeriodViewModelFactory preferencePeriodViewModelFactory)
		{
			_viewModelFactory = viewModelFactory;
			_preferencePeriodViewModelFactory = preferencePeriodViewModelFactory;
		}


		[HttpGet]
		[AsyncTask]
		public void FeedbackAsync(DateOnly date) { }

		[UnitOfWork]
		public virtual void FeedbackTask(DateOnly date)
		{
			AsyncManager.Parameters["model"] = _viewModelFactory.CreateDayFeedbackViewModel(date);
		}

		public JsonResult FeedbackCompleted(PreferenceDayFeedbackViewModel model, Task task)
		{
			task.Wait();
			return Json(model, JsonRequestBehavior.AllowGet);
		}




		[HttpGet]
		[AsyncTask]
		public void PeriodFeedbackAsync(DateOnly date) { }

		[UnitOfWork]
		public virtual void PeriodFeedbackTask(DateOnly date)
		{
			AsyncManager.Parameters["model"] = _preferencePeriodViewModelFactory.CreatePeriodFeedbackViewModel(date);
		}

		public JsonResult PeriodFeedbackCompleted(PreferencePeriodFeedbackViewModel model, Task task)
		{
			task.Wait();
			return Json(model, JsonRequestBehavior.AllowGet);
		}


	}

}