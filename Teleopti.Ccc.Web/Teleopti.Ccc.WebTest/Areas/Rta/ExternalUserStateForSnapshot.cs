using System;

namespace Teleopti.Ccc.WebTest.Areas.Rta
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