using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	public class ExternalUserStateForTest : ExternalUserStateInputModel
	{
		public ExternalUserStateForTest()
		{
			AuthenticationKey = Domain.ApplicationLayer.Rta.Service.ConfiguredKeyAuthenticator.LegacyAuthenticationKey;
			PlatformTypeId = Guid.Empty.ToString();
			SourceId = "sourceId";
			UserCode = "8808";
			StateCode = "AUX2";
			IsLoggedOn = true;
		}
	}
}