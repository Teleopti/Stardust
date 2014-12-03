using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class IntervalLengthFetcher : IIntervalLengthFetcher
	{
		//have a repository or do it self
		public int IntervalLength { get { return 15; }}
	}
}