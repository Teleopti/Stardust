using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class StateController : ApiController
	{
		private readonly Domain.ApplicationLayer.Rta.Service.Rta _rta;
		private readonly INow _now;
		private readonly IToggleManager _toggles;
		private readonly IRtaTracer _rtaTracer;

		public StateController(Domain.ApplicationLayer.Rta.Service.Rta rta, INow now, IToggleManager toggles, IRtaTracer rtaTracer)
		{
			_rta = rta;
			_now = now;
			_toggles = toggles;
			_rtaTracer = rtaTracer;
		}

		[HttpPost, Route("Rta/State/Change")]
		public IHttpActionResult Change([FromBody] ExternalUserStateWebModel input)
		{
			return handleRtaExceptions(new BatchInputModel
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
			return handleRtaExceptions(new BatchInputModel
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

		private IHttpActionResult handleRtaExceptions(BatchInputModel input)
		{
			try
			{
				if (_toggles.IsEnabled(Toggles.RTA_AsyncOptimization_43924))
					_rta.Enqueue(input);
				else
					_rta.Process(input);
			}
			catch (InvalidAuthenticationKeyException e)
			{
				return BadRequest(e.Message);
			}
			catch (LegacyAuthenticationKeyException e)
			{
				return BadRequest(e.Message);
			}
			catch (InvalidSourceException e)
			{
				return BadRequest(e.Message);
			}
			catch (InvalidUserCodeException e)
			{
				return BadRequest(e.Message);
			}

			return Ok();
		}
	}
}