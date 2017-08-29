using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

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