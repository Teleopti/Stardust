namespace Teleopti.Interfaces.Infrastructure
{
	public interface ICurrentAnalyticsUnitOfWorkFactory
	{
		IAnalyticsUnitOfWorkFactory Current();
	}
}