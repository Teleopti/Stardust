using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class ApplicationData : IApplicationData, IDataSourceForTenant
	{
		private readonly IList<IDataSource> _registeredDataSourceCollection;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly ILoadPasswordPolicyService _loadPasswordPolicyService;
		private readonly IDataSourcesFactory _dataSourcesFactory;

		public ApplicationData(IDictionary<string, string> appSettings,
			IMessageBrokerComposite messageBroker,
			ILoadPasswordPolicyService loadPasswordPolicyService,
			IDataSourcesFactory dataSourcesFactory)
		{
			AppSettings = appSettings;
			_registeredDataSourceCollection = new List<IDataSource>();
			_messageBroker = messageBroker;
			_loadPasswordPolicyService = loadPasswordPolicyService;
			_dataSourcesFactory = dataSourcesFactory;
		}

		public ILoadPasswordPolicyService LoadPasswordPolicyService
		{
			get { return _loadPasswordPolicyService; }
		}

		public IDataSourceForTenant DataSourceForTenant
		{
			get { return this; }
		}

		public IDataSource Tenant(string tenantName)
		{
			return _registeredDataSourceCollection.SingleOrDefault(x => x.DataSourceName.Equals(tenantName));
		}

		public IMessageBrokerComposite Messaging
		{
			get { return _messageBroker; }
		}

		public IDictionary<string, string> AppSettings { get; private set; }

		public void Dispose()
		{
			foreach (var dataSources in _registeredDataSourceCollection)
			{
				dataSources.Dispose();
			}
			if (Messaging != null)
			{
				Messaging.Dispose();
			}
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
	}
}