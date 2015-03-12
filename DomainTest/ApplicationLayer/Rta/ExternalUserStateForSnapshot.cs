using System;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
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