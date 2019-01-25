using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Authentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class AuthenticationFromFileTenantClient : IAuthenticationTenantClient
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;
		private readonly Func<IDataSourceForTenant> _dataSourceForTenant;
		private readonly ILoadUserUnauthorized _loadUserUnauthorized;

		public AuthenticationFromFileTenantClient(ITenantServerConfiguration tenantServerConfiguration,
																				Func<IDataSourceForTenant> dataSourceForTenant,
																				ILoadUserUnauthorized loadUserUnauthorized)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
			_dataSourceForTenant = dataSourceForTenant;
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

		public AuthenticationQuerierResult TryLogon(IdLogonClientModel idLogonClientModel, string userAgent)
		{
			throw new NotImplementedException();
		}

		private AuthenticationQuerierResult readFile()
		{
			var path = _tenantServerConfiguration.FullPath(string.Empty);
			if (!File.Exists(path))
				return new AuthenticationQuerierResult { FailReason = string.Format("No file with name {0}", path), Success = false };

			var json = File.ReadAllText(path);

			var dataSourceForTenant = _dataSourceForTenant();
			var result = JsonConvert.DeserializeObject<AuthenticationInternalQuerierResult>(json);
			dataSourceForTenant.MakeSureDataSourceCreated(result.Tenant, result.DataSourceConfiguration.ApplicationConnectionString, result.DataSourceConfiguration.AnalyticsConnectionString, new Dictionary<string, string>());
			var datasource = dataSourceForTenant.Tenant(result.Tenant);
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