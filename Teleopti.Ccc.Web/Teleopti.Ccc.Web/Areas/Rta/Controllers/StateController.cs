using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class StateController : ApiController
	{
		private readonly Domain.ApplicationLayer.Rta.Service.Rta _rta;
		private readonly ILog _logger;

		public StateController(Domain.ApplicationLayer.Rta.Service.Rta rta, ILog logger)
		{
			_rta = rta;
			_logger = logger;
		}

		[HttpPost, Route("Rta/State/Change")]
		public IHttpActionResult Change([FromBody]ExternalUserStateWebModel input)
		{
			DateTime batchId;
			DateTime.TryParse(input.BatchId, out batchId);

			try
			{
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

		[HttpPost, Route("Rta/State/Batch")]
		public IHttpActionResult Batch([FromBody]IEnumerable<ExternalUserStateWebModel> input)
		{

			try
			{
				var batch = input.Select(i =>
				{
					DateTime batchId;
					DateTime.TryParse(i.BatchId, out batchId);
					return new ExternalUserStateInputModel
					{
						AuthenticationKey = i.AuthenticationKey,
						PlatformTypeId = i.PlatformTypeId,
						SourceId = i.SourceId,
						UserCode = i.UserCode,
						StateCode = i.StateCode,
						StateDescription = i.StateDescription,
						IsLoggedOn = i.IsLoggedOn,
						BatchId = batchId,
						IsSnapshot = i.IsSnapshot,
					};
				});
				_rta.SaveStateBatch(batch);
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
