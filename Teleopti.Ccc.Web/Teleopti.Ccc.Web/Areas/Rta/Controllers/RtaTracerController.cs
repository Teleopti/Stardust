using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.All)]
	public class RtaTracerController : ApiController
	{
		[UnitOfWork, HttpGet, Route("api/RtaTracer/Traces")]
		public virtual IHttpActionResult Traces()
		{
			return Ok(new RtaTracerViewModel
			{
				Tracers = new[]
				{
					new RtaTracerTracerViewModel
					{
						Process = "box1:487",
						DataReceivedAt = "",
						ActivityCheckAt = "10:05:01",
						Tracing = "usercode34, Ashley Andeen"
					},
					new RtaTracerTracerViewModel
					{
						Process = "box1:1476",
						DataReceivedAt = "10:05:56",
						ActivityCheckAt = "10:06:01",
						Tracing = "usercode34, Ashley Andeen"
					}
				},
				UserCodeTraces = new[]
				{
					new RtaTracerUserCodeTracesViewModel
					{
						Header = "usercode34, Ashley Andeen",
						Traces = new[]
						{
							new Trace
							{
								StateCode = "AUX34",
								Lines = new[]
								{
									"Received",
									"Processing",
									"Processed",
									"PersonStateChangeEvent"
								}
							},
							new Trace
							{
								StateCode = "AUX34",
								Lines = new[]
								{
									"Received",
									"Processing",
									"No change"
								}
							},
							new Trace
							{
								StateCode = "AUX50",
								Lines = new[]
								{
									"Received",
									"Processing",
									"Processed",
									"PersonStateChangeEvent"
								}
							}
						}
					}
				}
			});
		}

		[UnitOfWork, HttpGet, Route("api/RtaTracer/Trace")]
		public virtual IHttpActionResult Trace()
		{
			return Ok();
		}

		[UnitOfWork, HttpGet, Route("api/RtaTracer/Stop")]
		public virtual IHttpActionResult Stop()
		{
			return Ok();
		}

		[UnitOfWork, HttpGet, Route("api/RtaTracer/Clear")]
		public virtual IHttpActionResult Clear()
		{
			return Ok();
		}
	}

	public class RtaTracerViewModel
	{
		public IEnumerable<RtaTracerTracerViewModel> Tracers;
		public IEnumerable<RtaTracerUserCodeTracesViewModel> UserCodeTraces;
	}

	public class RtaTracerUserCodeTracesViewModel
	{
		public string Header;
		public IEnumerable<Trace> Traces;
	}

	public class RtaTracerTracerViewModel
	{
		public string Process;
		public string DataReceivedAt;
		public string ActivityCheckAt;
		public string Tracing;
	}

	public class Trace
	{
		public string StateCode;
		public IEnumerable<string> Lines;
	}
}