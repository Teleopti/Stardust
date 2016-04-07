using System;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
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