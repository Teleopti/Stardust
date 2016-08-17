using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeLatestStatisticsIntervalIdLoader : ILatestStatisticsIntervalIdLoader
	{
		private int? _intervalId;

		public void Has(int? intervalId)
		{
			_intervalId = intervalId;
		}

		public int? Load()
		{
			return _intervalId;
		}
	}
}