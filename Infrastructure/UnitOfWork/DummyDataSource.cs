using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class DummyDataSource : IDataSource
	{
		public DummyDataSource(string name)
		{
			DataSourceName = name;
		}

		public string DataSourceName { get; set; }

		public IUnitOfWorkFactory Application { get; set; }
		public IUnitOfWorkFactory Statistic { get; set; }
		public IReadModelUnitOfWorkFactory ReadModel { get; set; }

		public void ResetStatistic()
		{
		}

		public void Dispose()
		{
		}

	}
}