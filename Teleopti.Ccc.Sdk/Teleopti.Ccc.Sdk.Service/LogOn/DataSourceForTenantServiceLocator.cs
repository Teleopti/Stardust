using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
{
	public static class DataSourceForTenantServiceLocator
	{
		public static IDataSourceForTenant DataSourceForTenant { get; private set; }

		public static void Set(IDataSourceForTenant dataSourceForTenant)
		{
			DataSourceForTenant = dataSourceForTenant;
		}
	}
}