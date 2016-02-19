using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public sealed class DataSource : IDataSource
	{
		public DataSource(IUnitOfWorkFactory application, IUnitOfWorkFactory statistic, IReadModelUnitOfWorkFactory readModel)
		{
			InParameter.NotNull("application", application);
			Application = application;
			Analytics = statistic;
			ReadModel = readModel;
		}

		public IUnitOfWorkFactory Analytics { get; private set; }

		public IUnitOfWorkFactory Application { get; private set; }

		public IReadModelUnitOfWorkFactory ReadModel { get; private set; }

		public string DataSourceName
		{
			get { return Application.Name; }
		}

		public void ResetStatistic()
		{
			Analytics = null;
		}

		public void Dispose()
		{
			if (Analytics != null)
				Analytics.Dispose();
			Application.Dispose();
		}
	}
}
