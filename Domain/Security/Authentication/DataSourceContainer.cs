using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public class DataSourceContainer : IDataSourceContainer
	{
		public DataSourceContainer(IDataSource dataSource, AuthenticationTypeOption authenticationTypeOption)
		{

			DataSource = dataSource;
			AuthenticationTypeOption = authenticationTypeOption;
		}

		public IDataSource DataSource { get; private set; }

		public AuthenticationTypeOption AuthenticationTypeOption { get; private set; }

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

		public override string ToString()
		{
			return DataSource != null ? DataSource.DataSourceName : "";
		}
	}
}