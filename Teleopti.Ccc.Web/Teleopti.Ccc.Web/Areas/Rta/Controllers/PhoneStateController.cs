using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)]
	public class PhoneStateController : ApiController
	{
		private readonly PhoneStateViewModelBuilder _build;

		public PhoneStateController(PhoneStateViewModelBuilder build)
		{
			_build = build;
		}

		[UnitOfWork, HttpGet, Route("api/PhoneState/InfoFor")]
		public virtual IHttpActionResult InfoFor([FromUri] Query query)
		{
			return Ok(_build.Build(query.Ids));
		}

		public class Query
		{
			public Guid[] Ids { get; set; }
		}
	}


}