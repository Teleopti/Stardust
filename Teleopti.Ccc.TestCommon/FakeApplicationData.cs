using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeApplicationData : ICurrentApplicationData, IApplicationData
	{
		private readonly DataSourceForTenant _dataSourceForTenant;

		public FakeApplicationData(DataSourceForTenant dataSourceForTenant)
		{
			_dataSourceForTenant = dataSourceForTenant;
		}

		public virtual IEnumerable<IDataSource> RegisteredDataSources
		{
			get
			{
				var values = new List<IDataSource>();
				_dataSourceForTenant.DoOnAllTenants_AvoidUsingThis(d =>
				{
					values.Add(d);
				});
				return values;
			}
			set
			{
				value.ForEach(x =>
				{
					_dataSourceForTenant.MakeSureDataSourceExists_UseOnlyFromTests(x);
				});
			}
		}

		public void Dispose()
		{
		}

		public IMessageBrokerComposite Messaging { get; private set; }
		public IDictionary<string, string> AppSettings { get; private set; }
		public ILoadPasswordPolicyService LoadPasswordPolicyService { get; private set; }
		
		public IApplicationData Current()
		{
			return this;
		}
	}

	public class FakeApplicationDataWithTestDatasource : FakeApplicationData
	{
		private readonly Func<IDataSourcesFactory> _dataSourcesFactory;

		public FakeApplicationDataWithTestDatasource(Func<IDataSourcesFactory> dataSourcesFactory, DataSourceForTenant dataSourceForTenant) : base(dataSourceForTenant)
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