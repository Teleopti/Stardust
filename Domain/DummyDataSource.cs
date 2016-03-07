using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain
{
	public class DummyDataSource : IDataSource
	{
		public DummyDataSource(string name)
		{
			DataSourceName = name;
		}

		public string DataSourceName { get; set; }

		public IUnitOfWorkFactory Application { get; set; }
		public IAnalyticsUnitOfWorkFactory Analytics { get; set; }
		public IReadModelUnitOfWorkFactory ReadModel { get; set; }

		public void RemoveAnalytics()
		{
		}

		public void Dispose()
		{
		}

	}
}