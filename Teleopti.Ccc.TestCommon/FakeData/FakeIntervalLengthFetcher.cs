using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeIntervalLengthFetcher : IIntervalLengthFetcher
	{
		private int intervalLength;

		public int GetIntervalLength()
		{
			return intervalLength;
		}

		private void SetIntervalLength(int value)
		{
			intervalLength = value;
		}

		public void Has(int intervalLength)
		{
			SetIntervalLength(intervalLength);
		}
	}
}