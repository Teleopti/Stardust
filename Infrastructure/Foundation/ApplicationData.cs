using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class ApplicationData : IApplicationData
	{
		private readonly IList<IDataSource> _registeredDataSourceCollection;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly ILoadPasswordPolicyService _loadPasswordPolicyService;
		private readonly IDataSourcesFactory _dataSourcesFactory;

		public ApplicationData(IDictionary<string, string> appSettings,
			IEnumerable<IDataSource> registeredDataSources,
			IMessageBrokerComposite messageBroker,
			ILoadPasswordPolicyService loadPasswordPolicyService,
			IDataSourcesFactory dataSourcesFactory)
		{
			AppSettings = appSettings;
			_registeredDataSourceCollection = registeredDataSources.ToList();
			_messageBroker = messageBroker;
			_loadPasswordPolicyService = loadPasswordPolicyService;
			_dataSourcesFactory = dataSourcesFactory;
			checkNoDuplicateDataSourceExists(_registeredDataSourceCollection);
		}

		public ILoadPasswordPolicyService LoadPasswordPolicyService
		{
			get { return _loadPasswordPolicyService; }
		}

		private static void checkNoDuplicateDataSourceExists(IEnumerable<IDataSource> registeredDataSources)
		{
			InParameter.NotNull("registeredDataSources", registeredDataSources);
			IList<string> uniqueNames = new List<string>();
			foreach (IDataSource dataSource in registeredDataSources)
			{
				if(dataSource.Application==null)
					continue;
				if (uniqueNames.Contains(dataSource.DataSourceName))
					throw new DataSourceException(
						 string.Format(CultureInfo.CurrentCulture, "The data sources '{0}' is registered multiple times.",
											dataSource.DataSourceName));
				uniqueNames.Add(dataSource.DataSourceName);
			}
		}

		public IDataSource DataSource(string tenant)
		{
			return _registeredDataSourceCollection.SingleOrDefault(x => x.DataSourceName.Equals(tenant));
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

		//TODO: tenant remove this!
		public IDataSource CreateAndAddDataSource(string dataSourceName, IDictionary<string, string> applicationNhibConfiguration, string analyticsConnectionString)
		{
			var dataSource = existingDataSource(dataSourceName);
			if (dataSource != null)
				return dataSource;

			applicationNhibConfiguration[NHibernate.Cfg.Environment.SessionFactoryName] = dataSourceName;

			var newDataSource =  _dataSourcesFactory.Create(applicationNhibConfiguration, analyticsConnectionString);
			_registeredDataSourceCollection.Add(newDataSource);
			return newDataSource;
		}

		private IDataSource existingDataSource(string datasourceName)
		{
			return _registeredDataSourceCollection.FirstOrDefault(dataSource => dataSource.DataSourceName.Equals(datasourceName));
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