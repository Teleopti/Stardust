using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using NHibernate.Cfg.ConfigurationSchema;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	/// <summary>
	/// Data shared by hole application
	/// </summary>
	public class ApplicationData : IApplicationData
	{
		private readonly IList<IDataSource> _registeredDataSourceCollection;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly ILoadPasswordPolicyService _loadPasswordPolicyService;
		private readonly IDataSourcesFactory _dataSourcesFactory;
		private bool disposed;

		public ApplicationData(IDictionary<string, string> appSettings, IEnumerable<IDataSource> registeredDataSources, IMessageBrokerComposite messageBroker, ILoadPasswordPolicyService loadPasswordPolicyService)
		{
			InParameter.NotNull("appSettings", appSettings);
			InParameter.NotNull("registeredDataSources", registeredDataSources);
			AppSettings = appSettings;
			if (!registeredDataSources.Any())
				throw new DataSourceException("Can not find any registered data source");
			checkNoDuplicateDataSourceExists(registeredDataSources);
			_registeredDataSourceCollection = new List<IDataSource>(registeredDataSources);
			_messageBroker = messageBroker;
			_loadPasswordPolicyService = loadPasswordPolicyService;
		}

		public ApplicationData(IDictionary<string, string> appSettings, IDataSource registeredDataSources,
									  IMessageBrokerComposite messageBroker)
		{
			InParameter.NotNull("appSettings", appSettings);
			InParameter.NotNull("registeredDataSources", registeredDataSources);
			AppSettings = appSettings;
			IList<IDataSource> sources = new List<IDataSource> { registeredDataSources };
			_registeredDataSourceCollection = new ReadOnlyCollection<IDataSource>(sources);
			_messageBroker = messageBroker;
		}

		//factory so we can add datasources later
		public ApplicationData(IDictionary<string, string> appSettings, IMessageBrokerComposite messageBroker,
			ILoadPasswordPolicyService loadPasswordPolicyService, IDataSourcesFactory dataSourcesFactory)
		{
			InParameter.NotNull("appSettings", appSettings);
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

		private static void checkNoDuplicateDataSourceExists(IEnumerable<IDataSource> registeredDataSources)
		{
			InParameter.NotNull("registeredDataSources", registeredDataSources);
			IList<string> uniqueNames = new List<string>();
			foreach (IDataSource dataSource in registeredDataSources)
			{
				if(dataSource.Application==null)
					continue;
				if (uniqueNames.Contains(dataSource.Application.Name))
					throw new DataSourceException(
						 string.Format(CultureInfo.CurrentCulture, "The data sources '{0}' is registered multiple times.",
											dataSource.Application.Name));
				uniqueNames.Add(dataSource.Application.Name);
			}
		}

		public IEnumerable<IDataSource> RegisteredDataSourceCollection
		{
			get { return _registeredDataSourceCollection; }
		}

		public IMessageBrokerComposite Messaging
		{
			get { return _messageBroker; }
		}

		public IDictionary<string, string> AppSettings { get; private set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					ReleaseManagedResources();
				}
				ReleaseUnmanagedResources();
				disposed = true;
			}
		}

		protected virtual void ReleaseUnmanagedResources()
		{
		}

		protected virtual void ReleaseManagedResources()
		{
			foreach (IDataSource dataSources in RegisteredDataSourceCollection)
			{
				dataSources.Dispose();
			}
			if (Messaging != null)
			{
				Messaging.Dispose();
			}
		}

		public void DisposeAllDataSourcesExcept(IDataSource dataSource)
		{
			for (var i = _registeredDataSourceCollection.Count - 1; i >= 0; i--)
			{
				var ds = _registeredDataSourceCollection[i];
				if (ds != dataSource)
				{
					_registeredDataSourceCollection.Remove(ds);
					ds.Dispose();
				}
			}
		}

		public IDataSource CreateAndAddDataSource(string dataSourceName, IDictionary<string, string> applicationNhibConfiguration, string analyticsConnectionString)
		{
			//just hack for now! Will be fixed soon!!
			var dataSource = existingDataSource(dataSourceName);
			if (dataSource != null)
				return dataSource;

			var newDataSource =  _dataSourcesFactory.Create(applicationNhibConfiguration, analyticsConnectionString);
			_registeredDataSourceCollection.Add(newDataSource);
			return newDataSource;
			//
		}

		private IDataSource existingDataSource(string datasourceName)
		{
			return _registeredDataSourceCollection.FirstOrDefault(dataSource => dataSource.DataSourceName.Equals(datasourceName));
		}
	}
}