using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Web.Areas.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public class ExternalUserStateWebModelForTest : ExternalUserStateWebModel
	{
		public ExternalUserStateWebModelForTest()
		{
			AuthenticationKey = TeleoptiRtaService.DefaultAuthenticationKey;
			PlatformTypeId = Guid.Empty.ToString();
			SourceId = "sourceId";
			IsLoggedOn = true;
		}
	}
}