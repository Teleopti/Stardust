using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class CurrentAnalyticsUnitOfWorkFactory : ICurrentAnalyticsUnitOfWorkFactory
	{
		private readonly ICurrentDataSource _currentDataSource;

		public static ICurrentAnalyticsUnitOfWorkFactory Make()
		{
			return new CurrentAnalyticsUnitOfWorkFactory(CurrentDataSource.Make());
		}

		public CurrentAnalyticsUnitOfWorkFactory(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public IAnalyticsUnitOfWorkFactory Current()
		{
			var current = _currentDataSource.Current();
			return current == null ? null : current.Analytics;
		}
	}
}