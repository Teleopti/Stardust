﻿using System.Linq;
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
            var findApplicationUser = AutofacHostFactory.Container.Resolve<IFindApplicationUser>();
            var dataSource =
                StateHolderReader.Instance.StateReader.ApplicationScopeData.RegisteredDataSourceCollection.
                    FirstOrDefault<IDataSource>(d => d.Application.Name == customUserNameSecurityToken.DataSource);

            _dataSourceContainer = new DataSourceContainer(dataSource, new RepositoryFactory(), findApplicationUser,
                                                           AuthenticationTypeOption.Application);
        }

        public IDataSourceContainer DataSourceContainer { get { return _dataSourceContainer; } }
    }
}