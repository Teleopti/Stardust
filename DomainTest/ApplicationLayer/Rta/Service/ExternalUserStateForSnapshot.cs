using System;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	public class ExternalUserStateForSnapshot : ExternalUserStateForTest
	{
		public ExternalUserStateForSnapshot(DateTime time)
		{
			BatchId = time;
			IsSnapshot = true;
		}
	}
}