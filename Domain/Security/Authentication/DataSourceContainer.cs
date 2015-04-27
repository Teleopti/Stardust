using System;
using log4net;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public class DataSourceContainer : IDataSourceContainer
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(DataSourceContainer));

		public DataSourceContainer(IDataSource dataSource, IRepositoryFactory repositoryFactory, AuthenticationTypeOption authenticationTypeOption)
		{

			DataSource = dataSource;
			RepositoryFactory = repositoryFactory;
			AuthenticationTypeOption = authenticationTypeOption;
		}

		public IDataSource DataSource { get; private set; }

		public AuthenticationTypeOption AuthenticationTypeOption { get; private set; }

		public IRepositoryFactory RepositoryFactory { get; private set; }

		public IPerson User { get; private set; }

		public IAvailableBusinessUnitsProvider AvailableBusinessUnitProvider
		{
			get { return new AvailableBusinessUnitsProvider(this); }
		}

		public string DataSourceName
		{
			get
			{
				return DataSource != null ? DataSource.DataSourceName : "";
			}
		}

		public void SetUser(IPerson person)
		{
			User = person;
		}

		public string LogOnName { get; set; }

		public override string ToString()
		{
			return DataSource != null ? DataSource.DataSourceName : "";
		}
	}
}