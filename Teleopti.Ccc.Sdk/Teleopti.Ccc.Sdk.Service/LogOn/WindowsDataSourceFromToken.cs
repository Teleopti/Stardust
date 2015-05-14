using System.Linq;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
{
    public class WindowsDataSourceFromToken
    {
        private DataSourceContainer _dataSourceContainer;

        public bool DataSourceNotFound()
        {
            return _dataSourceContainer == null;
        }

        public void SetDataSource(ITokenWithBusinessUnitAndDataSource customUserNameSecurityToken)
        {
            var dataSource =
                StateHolderReader.Instance.StateReader.ApplicationScopeData.Tenant(customUserNameSecurityToken.DataSource);

            _dataSourceContainer = new DataSourceContainer(dataSource, AuthenticationTypeOption.Windows);
        }

        public IDataSourceContainer DataSourceContainer { get { return _dataSourceContainer; } }
    }
}