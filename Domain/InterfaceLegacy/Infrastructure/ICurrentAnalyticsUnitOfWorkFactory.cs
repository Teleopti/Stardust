namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface ICurrentAnalyticsUnitOfWorkFactory
	{
		IAnalyticsUnitOfWorkFactory Current();
	}
}