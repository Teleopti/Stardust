using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Analytics
{
	public interface ICurrentAnalyticsUnitOfWork
	{
		IUnitOfWork Current();
	}
}