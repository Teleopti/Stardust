using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public sealed class DataSource : IDataSource
	{
		public DataSource(IUnitOfWorkFactory application, IUnitOfWorkFactory statistic, IAuthenticationSettings authenticationSettings)
			: this(application, statistic, null, authenticationSettings)
		{
		}

		public DataSource(IUnitOfWorkFactory application, IUnitOfWorkFactory statistic, IReadModelUnitOfWorkFactory readModel, IAuthenticationSettings authenticationSettings)
		{
			InParameter.NotNull("application", application);
			Application = application;
			Statistic = statistic;
			ReadModel = readModel;
			AuthenticationSettings = authenticationSettings;
		}

		public string OriginalFileName { get; set; }

		public AuthenticationTypeOption AuthenticationTypeOption { get; set; }

		public IUnitOfWorkFactory Statistic { get; private set; }

		public IUnitOfWorkFactory Application { get; private set; }

		public IReadModelUnitOfWorkFactory ReadModel { get; private set; }

		public IAuthenticationSettings AuthenticationSettings { get; private set; }

		public string DataSourceName
		{
			get { return Application.Name; }
		}

		public void ResetStatistic()
		{
			Statistic = null;
		}

		public void Dispose()
		{
			if (Statistic != null)
				Statistic.Dispose();
			Application.Dispose();
		}
	}
}
