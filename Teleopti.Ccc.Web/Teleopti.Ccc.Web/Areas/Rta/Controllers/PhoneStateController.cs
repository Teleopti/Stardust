using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;
using Teleopti.Wfm.Adherence.Monitor;

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

		[UnitOfWork, HttpGet, Route("api/PhoneStates")]
		public virtual IHttpActionResult InfoFor()
		{
			return Ok(_build.Build());
		}
	}
}