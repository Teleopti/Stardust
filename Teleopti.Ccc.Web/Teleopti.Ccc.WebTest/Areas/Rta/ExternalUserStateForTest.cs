using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public class ExternalUserStateForTest : ExternalUserStateInputModel
	{
		public ExternalUserStateForTest()
		{
			AuthenticationKey = Web.Areas.Rta.Rta.DefaultAuthenticationKey;
			PlatformTypeId = Guid.Empty.ToString();
			SourceId = "sourceId";
			UserCode = "8808";
			StateCode = "AUX2";
			IsLoggedOn = true;
		}
	}
}