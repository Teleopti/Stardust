using System;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class AuthenticationQuerierResultConverter : IAuthenticationQuerierResultConverter
	{
		private readonly IDataSourceConfigDecryption _dataSourceDecryption;
		private readonly Func<IDataSourceForTenant> _dataSourceForTenant;
		private readonly ILoadUserUnauthorized _loadUser;

		public AuthenticationQuerierResultConverter(IDataSourceConfigDecryption dataSourceDecryption, Func<IDataSourceForTenant> dataSourceForTenant, ILoadUserUnauthorized loadUser)
		{
			_dataSourceDecryption = dataSourceDecryption;
			_dataSourceForTenant = dataSourceForTenant;
			_loadUser = loadUser;
		}

		public AuthenticationQuerierResult Convert(AuthenticationInternalQuerierResult tenantServerResult)
		{
			if (!tenantServerResult.Success)
				return new AuthenticationQuerierResult
				{
					Success = false,
					FailReason = tenantServerResult.FailReason
				};
			
			var dataSourceForTenant = _dataSourceForTenant();
			var decryptedConfig = _dataSourceDecryption.DecryptConfig(tenantServerResult.DataSourceConfiguration);
			dataSourceForTenant.MakeSureDataSourceCreated(tenantServerResult.Tenant, decryptedConfig.ApplicationConnectionString, decryptedConfig.AnalyticsConnectionString, decryptedConfig.ApplicationNHibernateConfig);
			var dataSource = dataSourceForTenant.Tenant(tenantServerResult.Tenant);
			var person = _loadUser.LoadFullPersonInSeperateTransaction(dataSource.Application, tenantServerResult.PersonId);
			if (person.IsTerminated())
				return new AuthenticationQuerierResult
				{
					Success = false,
					FailReason = Resources.LogOnFailedInvalidUserNameOrPassword
				};

			return new AuthenticationQuerierResult
			{
				Success = true,
				DataSource = dataSource,
				Person = person,
				TenantPassword = tenantServerResult.TenantPassword
			};
		}
	}
}