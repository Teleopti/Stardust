using System;
using Teleopti.Ccc.Rta.WebService;
using Teleopti.Ccc.Web.Areas.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core
{
	public class ExternalUserStateForTest : ExternalUserState
	{
		public string AuthenticationKey = TeleoptiRtaService.DefaultAuthenticationKey;
		public string PlatformTypeId = Guid.Empty.ToString();
		public string SourceId = "sourceId";

		public ExternalUserStateForTest()
		{
			UserCode = "8808";
			StateCode = "AUX2";
			IsLoggedOn = true;
		}
	}
}