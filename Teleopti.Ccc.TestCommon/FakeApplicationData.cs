using System.Collections.Generic;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeApplicationData : ICurrentApplicationData, IApplicationData
	{
		private readonly FakeDataSourceForTenant _dataSourceForTenant;

		public FakeApplicationData(FakeDataSourceForTenant dataSourceForTenant)
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
					_dataSourceForTenant.Has(x);
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
	
}