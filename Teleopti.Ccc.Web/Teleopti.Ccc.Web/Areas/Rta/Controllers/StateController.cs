using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class StateController : ApiController
	{
		private readonly Wfm.Adherence.States.Rta _rta;
		private readonly INow _now;
		private readonly IRtaTracer _tracer;

		public StateController(Wfm.Adherence.States.Rta rta, INow now, IRtaTracer tracer)
		{
			_rta = rta;
			_now = now;
			_tracer = tracer;
		}

		[HttpPost, Route("Rta/State/Change")]
		public IHttpActionResult Change([FromBody] ExternalUserStateWebModel input)
		{
			_tracer.ProcessReceived("Rta/State/Change", 1);
			return process(new BatchInputModel
			{
				AuthenticationKey = input.AuthenticationKey,
				SourceId = input.SourceId,
				States = new[]
				{
					new BatchStateInputModel
					{
						StateCode = input.StateCode,
						UserCode = input.UserCode
					}
				}
			});
		}

		[HttpPost, Route("Rta/State/Batch")]
		public IHttpActionResult Batch([FromBody] ExternalUserBatchWebModel input)
		{
			_tracer.ProcessReceived("Rta/State/Batch", input?.States?.Count());
			return process(new BatchInputModel
			{
				AuthenticationKey = input.AuthenticationKey,
				SourceId = input.SourceId,
				SnapshotId = input.IsSnapshot ? _now.UtcDateTime() : null as DateTime?,
				CloseSnapshot = input.IsSnapshot,
				States = input.States.Select(i => new BatchStateInputModel
					{
						UserCode = i.UserCode,
						StateCode = i.StateCode
					})
					.ToArray()
			});
		}

		private IHttpActionResult process(BatchInputModel input)
		{
			var exceptionHandler = new InvalidInputMessage();
			_rta.Enqueue(input, exceptionHandler);
			if (exceptionHandler.Message != null)
				return BadRequest(exceptionHandler.Message);
			return Ok();
		}
	}
}