using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class AdherenceController : ApiController
	{
		private readonly IAdherencePercentageViewModelBuilder _adherencePercentageViewModelBuilder;
		private readonly IAdherenceDetailsViewModelBuilder _adherenceDetailsViewModelBuilder;

		public AdherenceController(IAdherencePercentageViewModelBuilder adherencePercentageViewModelBuilder, IAdherenceDetailsViewModelBuilder adherenceDetailsViewModelBuilder)
		{
			_adherencePercentageViewModelBuilder = adherencePercentageViewModelBuilder;
			_adherenceDetailsViewModelBuilder = adherenceDetailsViewModelBuilder;
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/Adherence/ForToday")]
		public virtual IHttpActionResult ForToday(Guid personId)
		{
			var model = _adherencePercentageViewModelBuilder.Build(personId);
			return Ok(model);
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/Adherence/ForDetails")]
		public virtual IHttpActionResult ForDetails(Guid personId)
		{
			var model = _adherenceDetailsViewModelBuilder.Build(personId);
			return Ok(model);
		}
	}
}
