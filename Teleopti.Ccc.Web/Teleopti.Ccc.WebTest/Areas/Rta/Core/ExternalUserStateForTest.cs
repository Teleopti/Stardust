using System;
using Teleopti.Ccc.Web.Areas.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core
{
	public class ExternalUserStateForTest : ExternalUserStateInputModel
	{
		public ExternalUserStateForTest()
		{
			AuthenticationKey = TeleoptiRtaService.DefaultAuthenticationKey;
			PlatformTypeId = Guid.Empty.ToString();
			SourceId = "sourceId";
			UserCode = "8808";
			StateCode = "AUX2";
			IsLoggedOn = true;
		}
	}
}