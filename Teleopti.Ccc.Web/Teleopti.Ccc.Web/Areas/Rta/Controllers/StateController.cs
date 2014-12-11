using System;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class StateController : Controller
	{
		private readonly IRta _rta;

		public StateController(IRta rta)
		{
			_rta = rta;
		}

		[HttpPost]
		public JsonResult Change(ExternalUserStateWebModel input)
		{
			DateTime batchId;
			DateTime.TryParse(input.BatchId, out batchId);

			var result = _rta.SaveState(
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

			// apparently 1 = input accepted, 0 = something was missing, anything else == error
			if (result == 1 || result == 0)
				return Json(result);
			throw new HttpException("Result from TeleoptiRtaService was " + result);
		}
	}
}
