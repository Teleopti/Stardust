using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class StateController : ApiController
	{
		private readonly Domain.ApplicationLayer.Rta.Service.Rta _rta;
		private readonly INow _now;

		public StateController(Domain.ApplicationLayer.Rta.Service.Rta rta, INow now)
		{
			_rta = rta;
			_now = now;
		}

		[HttpPost, Route("Rta/State/Change")]
		public IHttpActionResult Change([FromBody]ExternalUserStateWebModel input)
		{
			try
			{
				_rta.Process(new BatchInputModel
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
			catch (InvalidPlatformException e)
			{
				return BadRequest(e.Message);
			}
			catch (InvalidUserCodeException e)
			{
				return BadRequest(e.Message);
			}

			return Ok();
		}

		[HttpPost, Route("Rta/State/Batch")]
		public IHttpActionResult Batch([FromBody]ExternalUserBatchWebModel input)
		{
			try
			{
				_rta.Process(new BatchInputModel
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
			catch (InvalidPlatformException e)
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
