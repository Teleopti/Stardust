using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public class ExternalUserStateWebModelForTest : ExternalUserStateWebModel
	{
		public ExternalUserStateWebModelForTest()
		{
			AuthenticationKey = Domain.ApplicationLayer.Rta.Service.Rta.DefaultAuthenticationKey;
			PlatformTypeId = Guid.Empty.ToString();
			SourceId = "sourceId";
			IsLoggedOn = true;
		}
	}
}