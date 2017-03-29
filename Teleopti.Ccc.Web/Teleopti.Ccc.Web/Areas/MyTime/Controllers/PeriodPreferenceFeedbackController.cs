using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[PreferencePermission]
	public class PeriodPreferenceFeedbackController : Controller
	{
		private readonly IPreferenceViewModelFactory _viewModelFactory;

		public PeriodPreferenceFeedbackController(IPreferenceViewModelFactory viewModelFactory)
		{
			_viewModelFactory = viewModelFactory;
		}

		[UnitOfWork]
		public virtual JsonResult PeriodFeedback(DateOnly startDate, DateOnly endDate)
		{
			var preferenceFeedbacks = _viewModelFactory.CreateDayFeedbackViewModel(new DateOnlyPeriod(startDate, endDate));
			return Json(preferenceFeedbacks, JsonRequestBehavior.AllowGet);
		}
	}
}