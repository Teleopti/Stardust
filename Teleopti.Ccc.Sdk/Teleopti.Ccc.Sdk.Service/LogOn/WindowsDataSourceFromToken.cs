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
                StateHolderReader.Instance.StateReader.ApplicationScopeData.DataSource(customUserNameSecurityToken.DataSource);

            _dataSourceContainer = new DataSourceContainer(dataSource, new RepositoryFactory(),
                                                           AuthenticationTypeOption.Windows);
        }

        public IDataSourceContainer DataSourceContainer { get { return _dataSourceContainer; } }
    }
}