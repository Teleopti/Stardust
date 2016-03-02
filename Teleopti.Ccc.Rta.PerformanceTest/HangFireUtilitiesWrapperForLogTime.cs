using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Infrastructure.Hangfire;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	public class HangFireUtilitiesWrapperForLogTime
	{
		private readonly HangfireUtilties _hangfire;

		public HangFireUtilitiesWrapperForLogTime(HangfireUtilties hangfire)
		{
			_hangfire = hangfire;
		}

		[LogTime]
		public virtual void WaitForQueue()
		{
			_hangfire.WaitForQueue();
		}
	}
}