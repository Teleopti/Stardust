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
		private readonly ICalculateAdherenceDetails _calculateAdherenceDetails;

		public AdherenceController(ICalculateAdherence calculateAdherence, ICalculateAdherenceDetails calculateAdherenceDetails)
		{
			_calculateAdherence = calculateAdherence;
			_calculateAdherenceDetails = calculateAdherenceDetails;
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet]
		public virtual JsonResult ForToday(Guid personId)
		{
			var model = _calculateAdherence.ForToday(personId);
			return Json(model ?? new object(), JsonRequestBehavior.AllowGet);
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet]
		public virtual JsonResult ForDetails(Guid personId)
		{
			var model = _calculateAdherenceDetails.ForDetails(personId);
			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}
}
