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
		private readonly Func<IApplicationData> _applicationData;
		private readonly ILoadUserUnauthorized _loadUser;

		public AuthenticationQuerierResultConverter(IDataSourceConfigDecryption dataSourceDecryption, Func<IApplicationData> applicationData, ILoadUserUnauthorized loadUser)
		{
			_dataSourceDecryption = dataSourceDecryption;
			_applicationData = applicationData;
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
			
			var applicationData = _applicationData();
			var decryptedConfig = _dataSourceDecryption.DecryptConfig(tenantServerResult.DataSourceConfiguration);
			applicationData.MakeSureDataSourceExists(tenantServerResult.Tenant, decryptedConfig.ApplicationConnectionString, decryptedConfig.AnalyticsConnectionString, decryptedConfig.ApplicationNHibernateConfig);
			var dataSource = applicationData.Tenant(tenantServerResult.Tenant);
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