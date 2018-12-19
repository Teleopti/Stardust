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
			IInitializeLicenseServiceForTenant initializeLicenseServiceForTenant,
			IFindTenantByNameWithEnsuredTransaction findTenantByNameWithEnsuredTransaction
		) : base(
			dataSourcesFactory,
			initializeLicenseServiceForTenant,
			findTenantByNameWithEnsuredTransaction)
		{
		}

		public void Has(IDataSource datasource)
		{
			Add(datasource);
		}
	}

	public class DataSourceForTenant : IDataSourceForTenant
	{
		private readonly IDataSourcesFactory _dataSourcesFactory;
		private readonly IInitializeLicenseServiceForTenant _initializeLicenseServiceForTenant;
		private readonly IFindTenantByNameWithEnsuredTransaction _findTenantByNameWithEnsuredTransaction;
		private readonly IDictionary<string, tenantWithLicenseActivation> _dataSources;

		public DataSourceForTenant(
			IDataSourcesFactory dataSourcesFactory,
			IInitializeLicenseServiceForTenant initializeLicenseServiceForTenant,
			IFindTenantByNameWithEnsuredTransaction findTenantByNameWithEnsuredTransaction)
		{
			_dataSourcesFactory = dataSourcesFactory;
			_initializeLicenseServiceForTenant = initializeLicenseServiceForTenant;
			_findTenantByNameWithEnsuredTransaction = findTenantByNameWithEnsuredTransaction;
			_dataSources = new Dictionary<string, tenantWithLicenseActivation>();
		}

		private class tenantWithLicenseActivation
		{
			private readonly IDataSource _dataSource;
			private readonly IInitializeLicenseServiceForTenant _initializer;
			private bool _licenseServiceInitialized;

			public tenantWithLicenseActivation(IDataSource dataSource, IInitializeLicenseServiceForTenant initializer)
			{
				_dataSource = dataSource;
				_initializer = initializer;
				tryInitialize();
			}

			public IDataSource DataSource()
			{
				if (!_licenseServiceInitialized)
					tryInitialize();

				return _dataSource;
			}

			private void tryInitialize()
			{
				_licenseServiceInitialized = _initializer?.TryInitialize(_dataSource) ?? false;
			}
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
			return _dataSources.TryGetValue(tenantName, out var found) ? found.DataSource() : null;
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
//				_setLicenseActivator.Execute(newDataSource.Application);
				Add(newDataSource);
			}
		}

		protected void Add(IDataSource newDataSource)
		{
			_dataSources[newDataSource.DataSourceName] = new tenantWithLicenseActivation(newDataSource, _initializeLicenseServiceForTenant);
		}

		public void RemoveDataSource(string tenantName)
		{
			lock (dataSourceListLocker)
			{
				if (!_dataSources.TryGetValue(tenantName, out var ds))
					return;
				_dataSources.Remove(tenantName);
				ds.DataSource().Dispose();
			}
		}

		public void DoOnAllTenants_AvoidUsingThis(Action<IDataSource> actionOnTenant)
		{
			foreach (var dataSource in _dataSources.Values)
			{
				actionOnTenant(dataSource.DataSource());
			}
		}

		public void Dispose()
		{
			foreach (var dataSource in _dataSources.Values)
			{
				dataSource.DataSource().Dispose();
			}

			_dataSources.Clear();
		}
	}
}