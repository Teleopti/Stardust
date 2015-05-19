using System;
using System.IO;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class AuthenticationFromFileQuerier : IAuthenticationQuerier
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;
		private readonly Func<IApplicationData> _applicationData;
		private readonly ILoadUserUnauthorized _loadUserUnauthorized;

		public AuthenticationFromFileQuerier(ITenantServerConfiguration tenantServerConfiguration, 
																				Func<IApplicationData> applicationData,
																				ILoadUserUnauthorized loadUserUnauthorized)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
			_applicationData = applicationData;
			_loadUserUnauthorized = loadUserUnauthorized;
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

			var applicationdata = _applicationData();
			var result = JsonConvert.DeserializeObject<AuthenticationInternalQuerierResult>(json);
			applicationdata.MakeSureDataSourceExists(result.Tenant, result.DataSourceConfiguration.ApplicationNHibernateConfig, result.DataSourceConfiguration.AnalyticsConnectionString);
			var datasource = applicationdata.Tenant(result.Tenant);
			var ret = new AuthenticationQuerierResult
			{
				Success = true,
				Person = _loadUserUnauthorized.LoadFullPersonInSeperateTransaction(datasource.Application, result.PersonId),
				DataSource = datasource
			};
			return ret;
		}
	}
}