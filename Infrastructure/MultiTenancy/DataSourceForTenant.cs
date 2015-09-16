using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class DataSourceForTenant : IDataSourceForTenant
	{
		private readonly IDataSourcesFactory _dataSourcesFactory;
		private readonly ISetLicenseActivator _setLicenseActivator;
		private readonly IFindTenantByNameWithEnsuredTransaction _findTenantByNameWithEnsuredTransaction;
		private readonly IDictionary<string, IDataSource> _registeredDataSources;

		public DataSourceForTenant(IDataSourcesFactory dataSourcesFactory, 
														ISetLicenseActivator setLicenseActivator, 
														IFindTenantByNameWithEnsuredTransaction findTenantByNameWithEnsuredTransaction)
		{
			_dataSourcesFactory = dataSourcesFactory;
			_setLicenseActivator = setLicenseActivator;
			_findTenantByNameWithEnsuredTransaction = findTenantByNameWithEnsuredTransaction;
			_registeredDataSources = new Dictionary<string, IDataSource>();
		}

		public IDataSource Tenant(string tenantName)
		{
			var foundTenant = findTenant(tenantName);
			if (foundTenant != null)
				return foundTenant;
			var notYetParsedTenant = _findTenantByNameWithEnsuredTransaction.Find(tenantName);
			if (notYetParsedTenant != null)
			{
				MakeSureDataSourceExists(notYetParsedTenant.Name,
					notYetParsedTenant.DataSourceConfiguration.ApplicationConnectionString,
					notYetParsedTenant.DataSourceConfiguration.AnalyticsConnectionString,
					notYetParsedTenant.DataSourceConfiguration.ApplicationNHibernateConfig);
			}
			return findTenant(tenantName);
		}

		private IDataSource findTenant(string tenantName)
		{
			IDataSource found;
			return _registeredDataSources.TryGetValue(tenantName, out found) ? found : null;
		}

		private readonly object dataSourceListLocker = new object();
		public void MakeSureDataSourceExists(string tenantName, string applicationConnectionString, string analyticsConnectionString, IDictionary<string, string> applicationNhibConfiguration)
		{
			if (findTenant(tenantName) != null)
				return;
			lock (dataSourceListLocker)
			{
				if (findTenant(tenantName) != null)
					return;

				var newDataSource = _dataSourcesFactory.Create(tenantName, applicationConnectionString, analyticsConnectionString, applicationNhibConfiguration);
				_setLicenseActivator.Execute(newDataSource);
				_registeredDataSources[tenantName] = newDataSource;
			}
		}

		public void RemoveDataSource(string tenantName)
		{
			lock (dataSourceListLocker)
			{
				_registeredDataSources.Remove(tenantName);
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