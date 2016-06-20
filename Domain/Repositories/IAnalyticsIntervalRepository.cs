namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsIntervalRepository
	{
		int IntervalsPerDay();
		int MaxIntervalId();
	}
}