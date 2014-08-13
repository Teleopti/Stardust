using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
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

		 [UnitOfWorkAction]
		 [HttpGet]
		 public JsonResult Get()
		 {
			 return Json(_viewModelFactory.ViewModel(), JsonRequestBehavior.AllowGet);
		 }

    }

	public interface IRequestsShiftTradeScheduleFilterViewModelFactory
	{
		RequestsShiftTradeScheduleFilterViewModel ViewModel();
	}

	public class RequestsShiftTradeScheduleFilterViewModelFactory : IRequestsShiftTradeScheduleFilterViewModelFactory
	{
		private readonly IDayOffTemplateRepository _dayOffTemplateRepository;

		public RequestsShiftTradeScheduleFilterViewModelFactory(IDayOffTemplateRepository dayOffTemplateRepository)
		{
			_dayOffTemplateRepository = dayOffTemplateRepository;
		}

		public RequestsShiftTradeScheduleFilterViewModel ViewModel()
		{
			var ret = new RequestsShiftTradeScheduleFilterViewModel
			{
				DayOffShortNames =
					from t in _dayOffTemplateRepository.FindAllDayOffsSortByDescription()
					select t.Description.ShortName
			};

			return ret;
		}
	}

	public class RequestsShiftTradeScheduleFilterViewModel
	{
		public IEnumerable<string> DayOffShortNames { get; set; }
	}
}
