using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsIntervalRepository : IAnalyticsIntervalRepository
	{
		private readonly int _intervalsPerDay;
		private readonly int _maxIntervalId;

		public FakeAnalyticsIntervalRepository() : this(96, 95)
		{
		}

		public FakeAnalyticsIntervalRepository(int intervalsPerDay, int maxIntervalId)
		{
			_intervalsPerDay = intervalsPerDay;
			_maxIntervalId = maxIntervalId;
		}

		public int IntervalsPerDay()
		{
			return _intervalsPerDay;
		}

		public int MaxIntervalId()
		{
			return _maxIntervalId;
		}

	}
}