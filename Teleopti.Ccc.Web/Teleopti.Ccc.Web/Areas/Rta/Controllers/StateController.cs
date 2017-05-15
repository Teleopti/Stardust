using System;
using System.Collections.Generic;
using System.Linq;
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
			try
			{
				_rta.SaveStateBatch(new BatchInputModel
				{
					AuthenticationKey = input.AuthenticationKey,
					SourceId = input.SourceId,
					SnapshotId = parseSnapshotId(input.SnapshotId),
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
				_rta.SaveStateBatch(new BatchInputModel
				{
					AuthenticationKey = input.AuthenticationKey,
					SourceId = input.SourceId,
					SnapshotId = parseSnapshotId(input.SnapshotId),
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

		[HttpPost, Route("Rta/State/CloseSnapshot")]
		public void CloseSnapshot([FromBody]ExternalCloseSnapshotWebModel input)
		{
			_rta.CloseSnapshot(new CloseSnapshotInputModel
			{
				AuthenticationKey = input.AuthenticationKey,
				SourceId = input.SourceId,
				SnapshotId = input.SnapshotId
			});
		}

		private static DateTime? parseSnapshotId(string snapshotId)
		{
			DateTime parsed;
			DateTime.TryParse(snapshotId, out parsed);
			return parsed == DateTime.MinValue ? (DateTime?)null : parsed;
		}

	}
}
