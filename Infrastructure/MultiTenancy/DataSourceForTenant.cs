using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class DataSourceForTenant : IDataSourceForTenant
	{
		private readonly IDataSourcesFactory _dataSourcesFactory;
		private readonly IList<IDataSource> _registeredDataSourceCollection;

		public DataSourceForTenant(IDataSourcesFactory dataSourcesFactory)
		{
			_dataSourcesFactory = dataSourcesFactory;
			_registeredDataSourceCollection = new List<IDataSource>();
		}

		public IDataSource Tenant(string tenantName)
		{
			return _registeredDataSourceCollection.SingleOrDefault(x => x.DataSourceName.Equals(tenantName));
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

				applicationNhibConfiguration[NHibernate.Cfg.Environment.SessionFactoryName] = tenantName;
				applicationNhibConfiguration[NHibernate.Cfg.Environment.ConnectionString] = applicationConnectionString;
				var newDataSource = _dataSourcesFactory.Create(applicationNhibConfiguration, analyticsConnectionString);
				_registeredDataSourceCollection.Add(newDataSource);
			}
		}


		public void MakeSureDataSourceExists_UseOnlyFromTests(IDataSource datasource)
		{
			_registeredDataSourceCollection.Add(datasource);
		}

		public void DoOnAllTenants_AvoidUsingThis(Action<IDataSource> actionOnTenant)
		{
			foreach (var dataSource in _registeredDataSourceCollection)
			{
				actionOnTenant(dataSource);
			}
		}

		public void Dispose()
		{
			foreach (var dataSource in _registeredDataSourceCollection)
			{
				dataSource.Dispose();
			}
		}
	}
}