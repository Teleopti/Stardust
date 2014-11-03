using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class AdherenceController : Controller
	{
		private readonly ICalculateAdherence _calculateAdherence;

		public AdherenceController(ICalculateAdherence calculateAdherence)
		{
			_calculateAdherence = calculateAdherence;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult ForToday(Guid personId)
		{
			var ret = _calculateAdherence.ForToday(personId);
			return Json(ret.IsValid ? ret : new object(), JsonRequestBehavior.AllowGet);
		}

    }
	
}
