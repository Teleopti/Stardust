using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentApplicationData : ICurrentApplicationData, IApplicationData
	{
		private readonly Func<IDataSourcesFactory> _dataSourcesFactory;

		public FakeCurrentApplicationData(Func<IDataSourcesFactory> dataSourcesFactory)
		{
			_dataSourcesFactory = dataSourcesFactory;
		}

		public IEnumerable<IDataSource> RegisteredDataSources { get; set; }

		public void Dispose()
		{
		}

		public IDataSource Tenant(string tenantName)
		{
			throw new NotImplementedException();
		}

		public IMessageBrokerComposite Messaging { get; private set; }
		public IDictionary<string, string> AppSettings { get; private set; }
		public ILoadPasswordPolicyService LoadPasswordPolicyService { get; private set; }

		public void MakeSureDataSourceExists(string tenantName, string applicationConnectionString, string analyticsConnectionString,
			IDictionary<string, string> applicationNhibConfiguration)
		{
			throw new NotImplementedException();
		}

		public void DoOnAllTenants_AvoidUsingThis(Action<IDataSource> actionOnTenant)
		{
			if (RegisteredDataSources == null)
				RegisteredDataSources = new[] {_dataSourcesFactory.Invoke().Create("App", ConnectionStringHelper.ConnectionStringUsedInTests, null)};
			RegisteredDataSources.ForEach(actionOnTenant);
		}

		public IApplicationData Current()
		{
			return this;
		}
	}
}