using System.Web.Http;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.All)]
	public class RtaTracerController : ApiController
	{
		private readonly RtaTracerViewModelBuilder _viewModel;
		private readonly IRtaTracer _tracer;

		public RtaTracerController(RtaTracerViewModelBuilder viewModel, IRtaTracer tracer)
		{
			_viewModel = viewModel;
			_tracer = tracer;
		}

		[HttpGet, Route("api/RtaTracer/Traces")]
		public virtual IHttpActionResult Traces()
		{
			return Ok(_viewModel.Build());
		}

		[HttpGet, Route("api/RtaTracer/Trace")]
		public virtual IHttpActionResult Trace(string userCode)
		{
			_tracer.Trace(userCode);
			return Ok();
		}

		[HttpGet, Route("api/RtaTracer/Stop")]
		public virtual IHttpActionResult Stop()
		{
			_tracer.Stop();
			return Ok();
		}

		[HttpGet, Route("api/RtaTracer/Clear")]
		public virtual IHttpActionResult Clear()
		{
			_tracer.Clear();
			return Ok();
		}
	}
}