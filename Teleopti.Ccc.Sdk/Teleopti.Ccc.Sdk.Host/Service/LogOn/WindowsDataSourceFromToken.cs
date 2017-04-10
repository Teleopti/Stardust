using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.LogOn
{
    public class WindowsDataSourceFromToken
    {
	    public bool DataSourceNotFound()
        {
            return DataSource == null;
        }

        public void SetDataSource(ITokenWithBusinessUnitAndDataSource customUserNameSecurityToken)
        {
            DataSource =
								DataSourceForTenantServiceLocator.DataSourceForTenant.Tenant(customUserNameSecurityToken.DataSource);
        }

				public IDataSource DataSource { get; private set; }
    }
}