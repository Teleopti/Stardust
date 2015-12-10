using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class AdherenceController : Controller
	{
		private readonly IAdherencePercentageViewModelBuilder _adherencePercentageViewModelBuilder;
		private readonly IAdherenceDetailsViewModelBuilder _adherenceDetailsViewModelBuilder;

		public AdherenceController(IAdherencePercentageViewModelBuilder adherencePercentageViewModelBuilder, IAdherenceDetailsViewModelBuilder adherenceDetailsViewModelBuilder)
		{
			_adherencePercentageViewModelBuilder = adherencePercentageViewModelBuilder;
			_adherenceDetailsViewModelBuilder = adherenceDetailsViewModelBuilder;
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet]
		public virtual JsonResult ForToday(Guid personId)
		{
			var model = _adherencePercentageViewModelBuilder.Build(personId);
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet]
		public virtual JsonResult ForDetails(Guid personId)
		{
			var model = _adherenceDetailsViewModelBuilder.Build(personId);
			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}
}
