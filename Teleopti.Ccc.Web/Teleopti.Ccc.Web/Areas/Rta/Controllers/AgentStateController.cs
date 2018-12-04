using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;
using Teleopti.Wfm.Adherence.Monitor;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)]
	public class AgentStateController : ApiController
	{
		private readonly AgentStatesViewModelBuilder _builder;

		public AgentStateController(AgentStatesViewModelBuilder builder)
		{
			_builder = builder;
		}

		[UnitOfWork, HttpPost, Route("api/AgentStates/Poll")]
		public virtual IHttpActionResult Poll(AgentStateFilter filter)
		{
			return Ok(_builder.Build(filter));
		}
	}
}