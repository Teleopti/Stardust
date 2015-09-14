using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class DataSourceForTenant : IDataSourceForTenant
	{
		private readonly IDataSourcesFactory _dataSourcesFactory;
		private readonly ISetLicenseActivator _setLicenseActivator;
		private readonly IDictionary<string, IDataSource> _registeredDataSources;

		public DataSourceForTenant(IDataSourcesFactory dataSourcesFactory, ISetLicenseActivator setLicenseActivator)
		{
			_dataSourcesFactory = dataSourcesFactory;
			_setLicenseActivator = setLicenseActivator;
			_registeredDataSources = new Dictionary<string, IDataSource>();
		}

		public IDataSource Tenant(string tenantName)
		{
			IDataSource found;
			if (_registeredDataSources.TryGetValue(tenantName, out found))
				return found;
			return null;
		}

		private readonly object addDataSourceLocker = new object();
		public void MakeSureDataSourceExists(string tenantName, string applicationConnectionString, string analyticsConnectionString, IDictionary<string, string> applicationNhibConfiguration)
		{
			if (Tenant(tenantName) != null)
				return;
			lock (addDataSourceLocker)
			{
				if (Tenant(tenantName) != null)
					return;

				var newDataSource = _dataSourcesFactory.Create(tenantName, applicationConnectionString, analyticsConnectionString, applicationNhibConfiguration);
				_setLicenseActivator.Execute(newDataSource);
				_registeredDataSources[tenantName] = newDataSource;
			}
		}


		public void MakeSureDataSourceExists_UseOnlyFromTests(IDataSource datasource)
		{
			_registeredDataSources[datasource.DataSourceName] = datasource;
		}

		public void DoOnAllTenants_AvoidUsingThis(Action<IDataSource> actionOnTenant)
		{
			foreach (var dataSource in _registeredDataSources.Values)
			{
				actionOnTenant(dataSource);
			}
		}

		public void Dispose()
		{
			foreach (var dataSource in _registeredDataSources.Values)
			{
				dataSource.Dispose();
			}
			_registeredDataSources.Clear();
		}
	}
}