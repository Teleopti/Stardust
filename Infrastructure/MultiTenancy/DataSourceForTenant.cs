using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class FakeDataSourceForTenant : DataSourceForTenant
	{
		public FakeDataSourceForTenant(
			IDataSourcesFactory dataSourcesFactory,
			ISetLicenseActivator setLicenseActivator,
			IFindTenantByNameWithEnsuredTransaction findTenantByNameWithEnsuredTransaction
			) : base(
				dataSourcesFactory,
				setLicenseActivator,
				findTenantByNameWithEnsuredTransaction)
		{
		}

		public void Has(IDataSource datasource)
		{
			DataSources[datasource.DataSourceName] = datasource;
		}
	}

	public class DataSourceForTenant : IDataSourceForTenant
	{
		private readonly IDataSourcesFactory _dataSourcesFactory;
		private readonly ISetLicenseActivator _setLicenseActivator;
		private readonly IFindTenantByNameWithEnsuredTransaction _findTenantByNameWithEnsuredTransaction;
		protected readonly IDictionary<string, IDataSource> DataSources;

		public DataSourceForTenant(
			IDataSourcesFactory dataSourcesFactory,
			ISetLicenseActivator setLicenseActivator,
			IFindTenantByNameWithEnsuredTransaction findTenantByNameWithEnsuredTransaction)
		{
			_dataSourcesFactory = dataSourcesFactory;
			_setLicenseActivator = setLicenseActivator;
			_findTenantByNameWithEnsuredTransaction = findTenantByNameWithEnsuredTransaction;
			DataSources = new Dictionary<string, IDataSource>();
		}

		public IDataSource Tenant(string tenantName)
		{
			var foundTenant = findTenant(tenantName);
			if (foundTenant != null)
				return foundTenant;
			var notYetParsedTenant = _findTenantByNameWithEnsuredTransaction.Find(tenantName);
			if (notYetParsedTenant != null)
			{
				MakeSureDataSourceCreated(notYetParsedTenant.Name,
					notYetParsedTenant.DataSourceConfiguration.ApplicationConnectionString,
					notYetParsedTenant.DataSourceConfiguration.AnalyticsConnectionString,
					notYetParsedTenant.ApplicationConfig);
			}
			return findTenant(tenantName);
		}

		private IDataSource findTenant(string tenantName)
		{
			IDataSource found;
			return DataSources.TryGetValue(tenantName, out found) ? found : null;
		}

		private readonly object dataSourceListLocker = new object();
		public void MakeSureDataSourceCreated(string tenantName, string applicationConnectionString, string analyticsConnectionString, IDictionary<string, string> applicationNhibConfiguration)
		{
			if (findTenant(tenantName) != null)
				return;
			lock (dataSourceListLocker)
			{
				if (findTenant(tenantName) != null)
					return;

				var newDataSource = _dataSourcesFactory.Create(tenantName, applicationConnectionString, analyticsConnectionString, applicationNhibConfiguration);
				_setLicenseActivator.Execute(newDataSource);
				DataSources[tenantName] = newDataSource;
			}
		}

		public void RemoveDataSource(string tenantName)
		{
			lock (dataSourceListLocker)
			{
				IDataSource ds;
				if (DataSources.TryGetValue(tenantName, out ds))
				{
					DataSources.Remove(tenantName);
					ds.Dispose();
				}
			}
		}

		public void DoOnAllTenants_AvoidUsingThis(Action<IDataSource> actionOnTenant)
		{
			foreach (var dataSource in DataSources.Values)
			{
				actionOnTenant(dataSource);
			}
		}

		public void Dispose()
		{
			foreach (var dataSource in DataSources.Values)
			{
				dataSource.Dispose();
			}
			DataSources.Clear();
		}
	}
}