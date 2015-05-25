using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public class DataSourceContainer : IDataSourceContainer
	{
		public DataSourceContainer(IDataSource dataSource, IPerson user)
		{
			DataSource = dataSource;
			User = user;
		}

		public IDataSource DataSource { get; private set; }

		public IPerson User { get; private set; }

		public IAvailableBusinessUnitsProvider AvailableBusinessUnitProvider
		{
			get { return new AvailableBusinessUnitsProvider(User, DataSource); }
		}

		public override string ToString()
		{
			return DataSource != null ? DataSource.DataSourceName : "";
		}
	}
}