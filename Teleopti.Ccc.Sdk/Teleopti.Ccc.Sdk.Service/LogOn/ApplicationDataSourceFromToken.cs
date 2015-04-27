using Autofac.Integration.Wcf;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Interfaces.Domain;
using Autofac;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
{
    public class ApplicationDataSourceFromToken
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

            _dataSourceContainer = new DataSourceContainer(dataSource, new RepositoryFactory(), AuthenticationTypeOption.Application);
        }

        public IDataSourceContainer DataSourceContainer { get { return _dataSourceContainer; } }
    }
}