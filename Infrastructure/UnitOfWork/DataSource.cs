using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public sealed class DataSource : IDataSource
	{
		public DataSource(IUnitOfWorkFactory application, IAnalyticsUnitOfWorkFactory analytics, IReadModelUnitOfWorkFactory readModel)
		{
			InParameter.NotNull(nameof(application), application);
			Application = application;
			Analytics = analytics;
			ReadModel = readModel;
		}

		public IUnitOfWorkFactory Application { get; private set; }
		public IAnalyticsUnitOfWorkFactory Analytics { get; private set; }
		public IReadModelUnitOfWorkFactory ReadModel { get; private set; }

		public string DataSourceName => Application?.Name;

		public void RemoveAnalytics()
		{
			Analytics = null;
		}

		public void Dispose()
		{
			if (Analytics != null)
			{
				Analytics.Dispose();
				Analytics = null;
			}
			if (ReadModel != null)
			{
				ReadModel.Dispose();
				ReadModel = null;
			}
			Application.Dispose();
			Application = null;
		}
	}
}
