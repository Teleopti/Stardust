using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeApplicationData : ICurrentApplicationData, IApplicationData, IDataSourceForTenant
	{
		private readonly Func<IDataSourcesFactory> _dataSourcesFactory;
		private IEnumerable<IDataSource> _registeredDataSources = new IDataSource[] { };

		public FakeApplicationData(Func<IDataSourcesFactory> dataSourcesFactory)
		{
			_dataSourcesFactory = dataSourcesFactory;
		}

		public IEnumerable<IDataSource> RegisteredDataSources
		{
			get
			{
				if (!_registeredDataSources.Any())
					_registeredDataSources = new[] {_dataSourcesFactory.Invoke().Create("App", ConnectionStringHelper.ConnectionStringUsedInTests, null)};
				return _registeredDataSources;
			}
			set { _registeredDataSources = value.ToArray(); }
		}

		public void Dispose()
		{
		}

		public IDataSource Tenant(string tenantName)
		{
			return RegisteredDataSources.SingleOrDefault(x => x.DataSourceName.Equals(tenantName));
		}

		public IMessageBrokerComposite Messaging { get; private set; }
		public IDictionary<string, string> AppSettings { get; private set; }
		public ILoadPasswordPolicyService LoadPasswordPolicyService { get; private set; }

		public IDataSourceForTenant DataSourceForTenant
		{
			get { return this; }
		}

		public void MakeSureDataSourceExists(string tenantName, string applicationConnectionString, string analyticsConnectionString,
			IDictionary<string, string> applicationNhibConfiguration)
		{
			throw new NotImplementedException();
		}

		public void DoOnAllTenants_AvoidUsingThis(Action<IDataSource> actionOnTenant)
		{
			RegisteredDataSources.ForEach(actionOnTenant);
		}

		public IApplicationData Current()
		{
			return this;
		}
	}
}