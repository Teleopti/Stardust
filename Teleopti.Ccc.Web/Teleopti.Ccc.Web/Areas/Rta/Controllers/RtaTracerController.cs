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
					new Tracer
					{
						Process = "box1:487",
						DataReceivedAt = "",
						ActivityCheckAt = "10:05:01",
						Tracing = "usercode34, Ashley Andeen"
					},
					new Tracer
					{
						Process = "box1:1476",
						DataReceivedAt = "10:05:56",
						ActivityCheckAt = "10:06:01",
						Tracing = "usercode34, Ashley Andeen"
					}
				},
				TracedUsers = new[]
				{
					new TracedUser
					{
						User = "usercode34, Ashley Andeen",
						States = new[]
						{
							new TracedState
							{
								StateCode = "AUX34",
								Traces = new[]
								{
									"Received",
									"Processing",
									"Processed",
									"PersonStateChangeEvent"
								}
							},
							new TracedState
							{
								StateCode = "AUX34",
								Traces = new[]
								{
									"Received",
									"Processing",
									"No change"
								}
							},
							new TracedState
							{
								StateCode = "AUX50",
								Traces = new[]
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
		public IEnumerable<Tracer> Tracers;
		public IEnumerable<TracedUser> TracedUsers;
	}

	public class Tracer
	{
		public string Process;
		public string DataReceivedAt;
		public string ActivityCheckAt;
		public string Tracing;
	}
	
	public class TracedUser
	{
		public string User;
		public IEnumerable<TracedState> States;
	}
	
	public class TracedState
	{
		public string StateCode;
		public IEnumerable<string> Traces;
	}
}