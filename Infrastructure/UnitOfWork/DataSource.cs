using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	/// <summary>
	/// Holds a ref to IUnitOfWorkFactory towards Raptor and Matrix
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2008-04-17
	/// </remarks>
	public sealed class DataSource : IDataSource 
	{
		public DataSource(IUnitOfWorkFactory application, IUnitOfWorkFactory statistic, IAuthenticationSettings authenticationSettings)
		{
			InParameter.NotNull("application", application);
			Application = application;
			Statistic = statistic;
			AuthenticationSettings = authenticationSettings;
		}
		
		public string OriginalFileName { get; set; }

		public AuthenticationTypeOption AuthenticationTypeOption { get; set; }

		public IUnitOfWorkFactory Statistic { get; private set; }

		public IUnitOfWorkFactory Application { get; private set; }

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
			if(Statistic!=null)
				Statistic.Dispose();
			Application.Dispose();
		}
	}
}
