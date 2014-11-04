using System;
using System.Web.Mvc;
using Autofac.Extras.DynamicProxy2;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	[Intercept(typeof(AspectInterceptor))]
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
