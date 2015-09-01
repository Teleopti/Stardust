using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
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

		public CurrentDataSource(ICurrentIdentity currentIdentity, IConfigReader configReader, ICurrentApplicationData applicationData)
		{
			_currentIdentity = currentIdentity;
			_rtaConfigurationDataSource = new Lazy<IDataSource>(() => ConfiguredKeyAuthenticator.DataSourceFromRtaConfiguration(configReader, applicationData));
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
	}
}