namespace Teleopti.Interfaces.Infrastructure
{
	public interface ICurrentAnalyticsUnitOfWorkFactory
	{
		IUnitOfWorkFactory Current();
	}
}