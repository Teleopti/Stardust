using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public static class FakeDatabaseAdherenceExtensions
	{
		public static FakeDatabase WithApprovedPeriod(this FakeDatabase database, string startTime, string endTime)
		{
			return database.WithApprovedPeriod(null, startTime, endTime);
		}

		public static FakeDatabase WithRemovedApprovedPeriod(this FakeDatabase database, string startTime, string endTime)
		{
			return database.WithRemovedApprovedPeriod(null, startTime, endTime);
		}

		public static FakeDatabase WithHistoricalStateChange(this FakeDatabase database, string time, Adherence adherence)
		{
			return database.WithHistoricalStateChange(null, time, adherence);
		}

		public static FakeDatabase WithHistoricalAdherenceDayStart(this FakeDatabase database, string time, Adherence adherence)
		{
			return database.WithHistoricalAdherenceDayStart(null, time, adherence);
		}
	}
}