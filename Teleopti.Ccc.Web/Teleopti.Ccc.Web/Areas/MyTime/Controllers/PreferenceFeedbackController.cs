using System.Threading.Tasks;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Core.Aop.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[PreferencePermission]
	[Aspects]
	public class PreferenceFeedbackController : AsyncController
	{
		private readonly IPreferenceViewModelFactory _viewModelFactory;
		private readonly IPreferencePeriodFeedbackViewModelFactory _preferencePeriodFeedbackViewModelFactory;

		public PreferenceFeedbackController(IPreferenceViewModelFactory viewModelFactory, IPreferencePeriodFeedbackViewModelFactory preferencePeriodFeedbackViewModelFactory)
		{
			_viewModelFactory = viewModelFactory;
			_preferencePeriodFeedbackViewModelFactory = preferencePeriodFeedbackViewModelFactory;
		}


		[HttpGet]
		[AsyncTask]
		public void FeedbackAsync(DateOnly date) { }

		[UnitOfWork]
		public virtual void FeedbackTask(DateOnly date)
		{
			//Thread.Sleep(new Random().Next(100, 2000));
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
			AsyncManager.Parameters["model"] = _preferencePeriodFeedbackViewModelFactory.CreatePeriodFeedbackViewModel(date);
		}

		public JsonResult PeriodFeedbackCompleted(PreferencePeriodFeedbackViewModel model, Task task)
		{
			task.Wait();
			return Json(model, JsonRequestBehavior.AllowGet);
		}


	}

}