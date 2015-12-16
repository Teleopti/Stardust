using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[RequestPermission]
	public class RequestsShiftTradeScheduleFilterController : Controller
    {
	    private readonly IRequestsShiftTradeScheduleFilterViewModelFactory _viewModelFactory;

	    public RequestsShiftTradeScheduleFilterController(IRequestsShiftTradeScheduleFilterViewModelFactory viewModelFactory)
	    {
		    _viewModelFactory = viewModelFactory;
	    }

		 [UnitOfWork]
		 [HttpGet]
		 public virtual JsonResult Get()
		 {
			 return Json(_viewModelFactory.ViewModel(), JsonRequestBehavior.AllowGet);
		 }
    }
}
