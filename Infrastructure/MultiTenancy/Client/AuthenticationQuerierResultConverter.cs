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
		private readonly INhibConfigDecryption _nhibDecryption;
		private readonly Func<IApplicationData> _applicationData;
		private readonly ILoadUserUnauthorized _loadUser;

		public AuthenticationQuerierResultConverter(INhibConfigDecryption nhibDecryption, Func<IApplicationData> applicationData, ILoadUserUnauthorized loadUser)
		{
			_nhibDecryption = nhibDecryption;
			_applicationData = applicationData;
			_loadUser = loadUser;
		}

		public AuthenticationQuerierResult Convert(AuthenticationInternalQuerierResult tenantServerResult)
		{
			var ret = new AuthenticationQuerierResult{Success = tenantServerResult.Success};
			if (tenantServerResult.Success)
			{
				var applicationData = _applicationData();
				var decryptedConfig = _nhibDecryption.DecryptConfig(tenantServerResult.DataSourceConfiguration);
				applicationData.MakeSureDataSourceExists(tenantServerResult.Tenant, decryptedConfig.ApplicationNHibernateConfig, decryptedConfig.AnalyticsConnectionString);
				var dataSource = applicationData.Tenant(tenantServerResult.Tenant);
				var person = _loadUser.LoadFullPersonInSeperateTransaction(dataSource.Application, tenantServerResult.PersonId);
				if (person.IsTerminated())
					return new AuthenticationQuerierResult
					{
						Success = false,
						FailReason = Resources.LogOnFailedInvalidUserNameOrPassword
					};
				ret.Person = person;
				ret.DataSource = dataSource;
			
			}
			else
			{
				ret.FailReason = tenantServerResult.FailReason;
			}
			return ret;
		}
	}

	public interface IAuthenticationQuerierResultConverter
	{
		AuthenticationQuerierResult Convert(AuthenticationInternalQuerierResult tenantServerResult);
	}
}