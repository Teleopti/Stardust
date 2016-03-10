using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class StateController : ApiController
	{
		private readonly Domain.ApplicationLayer.Rta.Service.Rta _rta;

		public StateController(Domain.ApplicationLayer.Rta.Service.Rta rta)
		{
			_rta = rta;
		}

		[HttpPost, Route("Rta/State/Change")]
		public IHttpActionResult Change([FromBody]ExternalUserStateWebModel input)
		{
			DateTime batchId;
			DateTime.TryParse(input.BatchId, out batchId);

			try
			{
				StackifyLib.Metrics.Count("RTA", "SaveState");
				_rta.SaveState(
					new ExternalUserStateInputModel
					{
						AuthenticationKey = input.AuthenticationKey,
						PlatformTypeId = input.PlatformTypeId,
						SourceId = input.SourceId,
						UserCode = input.UserCode,
						StateCode = input.StateCode,
						StateDescription = input.StateDescription,
						IsLoggedOn = input.IsLoggedOn,
						BatchId = batchId,
						IsSnapshot = input.IsSnapshot,
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
