using System;
using System.IO;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class AuthenticationFromFileQuerier : IAuthenticationQuerier
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;
		private readonly Func<IApplicationData> _applicationData;

		public AuthenticationFromFileQuerier(ITenantServerConfiguration tenantServerConfiguration, Func<IApplicationData> applicationData)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
			_applicationData = applicationData;
		}

		public AuthenticationQuerierResult TryLogon(ApplicationLogonClientModel applicationLogonClientModel, string userAgent)
		{
			return readFile();
		}

		public AuthenticationQuerierResult TryLogon(IdentityLogonClientModel identityLogonClientModel, string userAgent)
		{
			return readFile();
		}

		private AuthenticationQuerierResult readFile()
		{
			var path = _tenantServerConfiguration.FullPath(string.Empty);
			if (!File.Exists(path))
				return new AuthenticationQuerierResult { FailReason = string.Format("No file with name {0}", path), Success = false };

			var json = File.ReadAllText(path);

			var result = JsonConvert.DeserializeObject<AuthenticationQuerierResult>(json);
			_applicationData().MakeSureDataSourceExists(result.DataSource.DataSourceName, result.ApplicationNHibernateConfig, result.AnalyticsConnectionString);
			return result;
		}
	}
}