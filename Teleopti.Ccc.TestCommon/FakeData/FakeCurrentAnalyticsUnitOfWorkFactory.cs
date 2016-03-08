using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeCurrentAnalyticsUnitOfWorkFactory : ICurrentAnalyticsUnitOfWorkFactory
	{
		public IAnalyticsUnitOfWorkFactory Current()
		{
			return new FakeAnalyticsUnitOfWorkFactory();
		}
	}
}