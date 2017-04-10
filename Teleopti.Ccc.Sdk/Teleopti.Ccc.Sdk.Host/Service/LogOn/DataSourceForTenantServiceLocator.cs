using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.LogOn
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