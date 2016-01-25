using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

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

			return Ok();
		}
	}
}
