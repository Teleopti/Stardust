using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public static class MutateNowExtensions
	{
		public static void Is(this IMutateNow now, string utc)
		{
			now.Is(utc.Utc());
		}
	}
}