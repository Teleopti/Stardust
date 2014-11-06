using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Web.Core.Aop.Aspects;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class AdherenceController : Controller
	{
		private readonly ICalculateAdherence _calculateAdherence;

		public AdherenceController(ICalculateAdherence calculateAdherence)
		{
			_calculateAdherence = calculateAdherence;
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet]
		public virtual JsonResult ForToday(Guid personId)
		{
			var model = _calculateAdherence.ForToday(personId);
			return Json(model ?? new object(), JsonRequestBehavior.AllowGet);
		}

    }
	
}
