using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeApplicationInsights : IApplicationInsights
	{
		public void Init()
		{
		}

		public void TrackEvent(string description)
		{
		}
	}
}
