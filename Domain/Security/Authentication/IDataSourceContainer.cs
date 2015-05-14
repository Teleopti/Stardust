using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public interface IDataSourceContainer
	{
		IDataSource DataSource { get; }
		AuthenticationTypeOption AuthenticationTypeOption { get; }
		IPerson User { get; }
		void SetUser(IPerson person);
		IAvailableBusinessUnitsProvider AvailableBusinessUnitProvider { get; }
		string DataSourceName { get; }
	}
}