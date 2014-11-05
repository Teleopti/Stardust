using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class AdherenceController : Controller
	{
		private readonly ICalculateAdherence _calculateAdherence;

		public AdherenceController(ICalculateAdherence calculateAdherence)
		{
			_calculateAdherence = calculateAdherence;
		}

		[ReadModelUnitOfWork, HttpGet]
		public virtual JsonResult ForToday(Guid personId)
		{
			var ret = _calculateAdherence.ForToday(personId);
			return Json(ret.IsValid ? ret : new object(), JsonRequestBehavior.AllowGet);
		}

    }
	
}
