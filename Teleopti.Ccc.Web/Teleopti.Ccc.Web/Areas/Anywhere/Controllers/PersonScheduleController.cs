using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class PersonScheduleController : Controller
	{
		private readonly IPersonScheduleViewModelFactory _viewModelFactory;

		public PersonScheduleController(IPersonScheduleViewModelFactory viewModelFactory)
		{
			_viewModelFactory = viewModelFactory;
		}

		[UnitOfWork, HttpGet]
		public virtual JsonResult Get(Guid personId, DateTime date)
		{
			var data = _viewModelFactory.CreateViewModel(personId, date);
			return Json(data, JsonRequestBehavior.AllowGet);
		}
	}

}