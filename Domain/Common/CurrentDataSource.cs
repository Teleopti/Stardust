using System;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class CurrentDataSource : ICurrentDataSource, IDataSourceScope
	{
		private readonly ICurrentIdentity _currentIdentity;
		private readonly Lazy<IDataSource> _rtaConfigurationDataSource;
		[ThreadStatic]
		private static IDataSource _threadDataSource;
 
		public static ICurrentDataSource Make()
		{
			return new CurrentDataSource(new CurrentIdentity(new CurrentTeleoptiPrincipal()), null, null);
		}

		public CurrentDataSource(ICurrentIdentity currentIdentity, IConfigReader configReader, Func<IDataSourceForTenant> dataSourceForTenant)
		{
			_currentIdentity = currentIdentity;
			_rtaConfigurationDataSource = new Lazy<IDataSource>(() => dataSourceFromRtaConfiguration(configReader, dataSourceForTenant));
		}

		public IDataSource Current()
		{
			if (_threadDataSource != null)
				return _threadDataSource;
			var identity = _currentIdentity.Current();
			if (identity != null)
				return identity.DataSource;
			return _rtaConfigurationDataSource.Value;
		}
		
		public string CurrentName()
		{
			return Current().DataSourceName;
		}

		public IDisposable OnThisThreadUse(IDataSource dataSource)
		{
			_threadDataSource = dataSource;
			return new GenericDisposable(() =>
			{
				_threadDataSource = null;
			});
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