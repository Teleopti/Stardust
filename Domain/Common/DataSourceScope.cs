using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class DataSourceScope : IDataSourceScope
	{
		private readonly IDataSourceForTenant _dataSourceForTenant;

		public DataSourceScope(IDataSourceForTenant dataSourceForTenant)
		{
			_dataSourceForTenant = dataSourceForTenant;
		}

		public IDisposable OnThisThreadUse(IDataSource dataSource)
		{
			DataSourceState.ThreadDataSource = dataSource;
			return new GenericDisposable(() =>
			{
				DataSourceState.ThreadDataSource = null;
			});
		}

		public IDisposable OnThisThreadUse(string tenant)
		{
			return OnThisThreadUse(_dataSourceForTenant.Tenant(tenant));
		}
	}
}