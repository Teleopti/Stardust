using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeIntervalLengthFetcher : IIntervalLengthFetcher
	{
		public int IntervalLength { get; private set; }

		public void Has(int intervalLength)
		{
			IntervalLength = intervalLength;
		}
	}
}