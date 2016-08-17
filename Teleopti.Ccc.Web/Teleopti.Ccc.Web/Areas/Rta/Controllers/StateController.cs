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

		public StateController(Domain.ApplicationLayer.Rta.Service.Rta rta)
		{
			_rta = rta;
		}

		[HttpPost, Route("Rta/State/Change")]
		public IHttpActionResult Change([FromBody]ExternalUserStateWebModel input)
		{
			DateTime snapshotId;
			DateTime.TryParse(input.SnapshotId, out snapshotId);

			try
			{
				_rta.SaveState(
					new StateInputModel
					{
						AuthenticationKey = input.AuthenticationKey,
						PlatformTypeId = input.PlatformTypeId,
						SourceId = input.SourceId,
						UserCode = input.UserCode,
						StateCode = input.StateCode,
						StateDescription = input.StateDescription,
						SnapshotId = snapshotId,
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
				var states = input.Select(i => new BatchStateInputModel
				{
					UserCode = i.UserCode,
					StateCode = i.StateCode,
					StateDescription = i.StateDescription,
				}).ToArray();

				var root = input.First();
				DateTime snapshotId;
				DateTime.TryParse(root.SnapshotId, out snapshotId);

				_rta.SaveStateBatch(new BatchInputModel
				{
					AuthenticationKey = root.AuthenticationKey,
					PlatformTypeId = root.PlatformTypeId,
					SourceId = root.SourceId,
					SnapshotId = snapshotId,
					States = states
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
