using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class DataSourceScope : IDataSourceScope
	{
		private readonly IDataSourceForTenant _dataSourceForTenant;
		private readonly DataSourceState _state;

		public DataSourceScope(IDataSourceForTenant dataSourceForTenant, DataSourceState state)
		{
			_dataSourceForTenant = dataSourceForTenant;
			_state = state;
		}

		public IDisposable OnThisThreadUse(IDataSource dataSource)
		{
			_state.SetOnThread(dataSource);
			return new GenericDisposable(() => _state.SetOnThread(null));
		}

		public IDisposable OnThisThreadUse(string tenant)
		{
			if (tenant == null)
				return OnThisThreadUse(null as IDataSource);
			return OnThisThreadUse(_dataSourceForTenant.Tenant(tenant));
		}
	}
}