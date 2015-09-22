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
		private IEnumerable<IDataSource> _registeredDataSources = new IDataSource[] { };

		public virtual IEnumerable<IDataSource> RegisteredDataSources
		{
			get { return _registeredDataSources; }
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

		public void MakeSureDataSourceExists(
			string tenantName, 
			string applicationConnectionString, 
			string analyticsConnectionString,
			IDictionary<string, string> applicationNhibConfiguration)
		{
			throw new NotImplementedException();
		}

		public void DoOnAllTenants_AvoidUsingThis(Action<IDataSource> actionOnTenant)
		{
			RegisteredDataSources.ForEach(actionOnTenant);
		}

		public void RemoveDataSource(string tenantName)
		{
			throw new NotImplementedException();
		}

		public IApplicationData Current()
		{
			return this;
		}
	}

	public class FakeApplicationDataWithTestDatasource : FakeApplicationData
	{
		private readonly Func<IDataSourcesFactory> _dataSourcesFactory;

		public FakeApplicationDataWithTestDatasource(Func<IDataSourcesFactory> dataSourcesFactory)
		{
			_dataSourcesFactory = dataSourcesFactory;
		}

		public override IEnumerable<IDataSource> RegisteredDataSources
		{
			get
			{
				if (!base.RegisteredDataSources.Any())
					base.RegisteredDataSources = new[] { _dataSourcesFactory.Invoke().Create("App", ConnectionStringHelper.ConnectionStringUsedInTests, null) };
				return base.RegisteredDataSources;
			}
		}
		
	}
}