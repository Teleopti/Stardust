﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
{
    public class ApplicationDataSourceFromToken
    {
	    public bool DataSourceNotFound()
        {
            return DataSource == null;
        }

        public void SetDataSource(ITokenWithBusinessUnitAndDataSource customUserNameSecurityToken)
        {
	        DataSource = DataSourceForTenantServiceLocator.DataSourceForTenant.Tenant(customUserNameSecurityToken.DataSource);
        }

        public IDataSource DataSource { get; private set; }
    }
}