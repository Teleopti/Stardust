using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentApplicationData:ICurrentApplicationData, IApplicationData
	{
		public void Dispose()
		{
		}

		public IEnumerable<IDataSource> RegisteredDataSourceCollection { get; set; }
		public IDataSource DataSource(string tenant)
		{
			throw new NotImplementedException();
		}

		public IMessageBrokerComposite Messaging { get; private set; }
		public IDictionary<string, string> AppSettings { get; private set; }
		public ILoadPasswordPolicyService LoadPasswordPolicyService { get; private set; }

		public void MakeSureDataSourceExists(string dataSourceName, IDictionary<string, string> applicationNhibConfiguration, string analyticsConnectionString)
		{
			throw new NotImplementedException();
		}

		public void DoOnAllTenants_AvoidUsingThis(Action<IDataSource> actionOnTenant)
		{
			foreach (var dataSource in RegisteredDataSourceCollection)
			{
				actionOnTenant(dataSource);
			}
		}

		public IApplicationData Current()
		{
			return this;
		}
	}
}