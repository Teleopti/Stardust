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
		public JsonResult Change(ExternalUserStateWebModel state)
		{
			var result = _rta.SaveState(
				new ExternalUserStateInputModel
				{
					AuthenticationKey = state.AuthenticationKey,
					PlatformTypeId = state.PlatformTypeId,
					SourceId = state.SourceId,
					UserCode = state.UserCode,
					StateCode = state.StateCode,
					StateDescription = state.StateDescription,
					IsLoggedOn = state.IsLoggedOn,
					SecondsInState = state.SecondsInState,
					Timestamp = DateTime.Parse(state.Timestamp),
					BatchId = DateTime.Parse(state.BatchId ?? state.Timestamp),
					IsSnapshot = state.IsSnapshot,
				});

			// apparently 1 = state accepted, 0 = something was missing, anything else == error
			if (result == 1 || result == 0)
				return Json(result);
			throw new HttpException("Result from TeleoptiRtaService was " + result);
		}
	}
}
