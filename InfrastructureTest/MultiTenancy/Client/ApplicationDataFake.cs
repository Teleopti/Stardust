using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class ApplicationDataFake :IApplicationData
	{
		private IDataSource _dataSource;

		public void Dispose()
		{}

		public IDataSource Tenant(string tenantName)
		{
			return _dataSource;
		}

		public IMessageBrokerComposite Messaging { get; private set; }
		public IDictionary<string, string> AppSettings { get; private set; }
		public ILoadPasswordPolicyService LoadPasswordPolicyService { get; private set; }

		public void MakeSureDataSourceExists(string tenantName, IDictionary<string, string> applicationNhibConfiguration,
			string analyticsConnectionString)
		{}

		public void DoOnAllTenants_AvoidUsingThis(Action<IDataSource> actionOnTenant)
		{
			throw new NotImplementedException();
		}

		public void SetDataSource(IDataSource dataSource)
		{
			_dataSource = dataSource;
		}
	}
}