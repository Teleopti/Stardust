using System;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.StandardPreferences)]
	public class PreferenceFeedbackController : AsyncController
	{
		private readonly IPreferenceViewModelFactory _viewModelFactory;

		public PreferenceFeedbackController(IPreferenceViewModelFactory viewModelFactory)
		{
			_viewModelFactory = viewModelFactory;
		}

		[HttpGet]
		[AsyncTask]
		public void FeedbackAsync(DateOnly date) { }

		public void FeedbackTask(DateOnly date)
		{
			AsyncManager.Parameters["model"] = _viewModelFactory.CreateDayFeedbackViewModel(date);
		}

		public JsonResult FeedbackCompleted(PreferenceDayFeedbackViewModel model, Exception exception)
		{
			if (exception != null)
				throw exception;
			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}
}