using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public interface IDataSourceContainer
	{
		IDataSource DataSource { get; }
		AuthenticationTypeOption AuthenticationTypeOption { get; }
		IRepositoryFactory RepositoryFactory { get; }
		IPerson User { get; }
		void SetUser(IPerson person);
		string LogOnName { get; set; }
		IAvailableBusinessUnitsProvider AvailableBusinessUnitProvider { get; }
		string DataSourceName { get; }
	}
}