using System;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Aspects
{
	public class ConnectionStringDataSourceScope : IRtaDataSourceScope
	{
		private readonly IDataSourceScope _dataSource;
		private readonly Lazy<IDataSource> _rtaConfigurationDataSource;

		[ThreadStatic]
		private static IDisposable _scope;

		public ConnectionStringDataSourceScope(IDataSourceScope dataSource, IConfigReader configReader, Func<IDataSourceForTenant> dataSourceForTenant)
		{
			_dataSource = dataSource;
			_rtaConfigurationDataSource = new Lazy<IDataSource>(() => dataSourceFromRtaConfiguration(configReader, dataSourceForTenant));
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			_scope = _dataSource.OnThisThreadUse(_rtaConfigurationDataSource.Value);
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			_scope.Dispose();
		}

		private static IDataSource dataSourceFromRtaConfiguration(IConfigReader configReader, Func<IDataSourceForTenant> dataSourceForTenant)
		{
			if (configReader == null)
				return null;
			if (dataSourceForTenant == null)
				return null;

			var configString = new SqlConnectionStringBuilder(configReader.ConnectionString("RtaApplication"));
			IDataSource dataSource = null;
			dataSourceForTenant().DoOnAllTenants_AvoidUsingThis(tenant =>
			{
				var c = new SqlConnectionStringBuilder(tenant.Application.ConnectionString);
				if (c.DataSource == configString.DataSource &&
					c.InitialCatalog == configString.InitialCatalog)
				{
					dataSource = tenant;
				}
			});

			return dataSource;
		}
	}
}