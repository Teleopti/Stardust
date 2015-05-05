using System;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class AuthenticationQuerier : IAuthenticationQuerier
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;
		private readonly INhibConfigDecryption _nhibConfigDecryption;
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IJsonSerializer _jsonSerializer;
		private readonly Func<IApplicationData> _applicationData;
		private readonly IVerifyTerminalDate _verifyTerminalDate;

		public AuthenticationQuerier(ITenantServerConfiguration tenantServerConfiguration, 
																INhibConfigDecryption nhibConfigDecryption, 
																IPostHttpRequest postHttpRequest,
																IJsonSerializer jsonSerializer,
																Func<IApplicationData> applicationData,
																IVerifyTerminalDate verifyTerminalDate)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
			_nhibConfigDecryption = nhibConfigDecryption;
			_postHttpRequest = postHttpRequest;
			_jsonSerializer = jsonSerializer;
			_applicationData = applicationData;
			_verifyTerminalDate = verifyTerminalDate;
		}

		public AuthenticationQueryResult TryLogon(ApplicationLogonClientModel applicationLogonClientModel, string userAgent)
		{
			return doAuthenticationCall(_tenantServerConfiguration.Path + "Authenticate/ApplicationLogon", applicationLogonClientModel, userAgent);
		}

		public AuthenticationQueryResult TryLogon(IdentityLogonClientModel identityLogonClientModel, string userAgent)
		{
			return doAuthenticationCall(_tenantServerConfiguration.Path + "Authenticate/IdentityLogon", identityLogonClientModel, userAgent);
		}

		private AuthenticationQueryResult doAuthenticationCall(string path, object clientModel, string userAgent)
		{
			var json = _jsonSerializer.SerializeObject(clientModel);
			var result = _postHttpRequest.Send<AuthenticationQueryResult>(path, json, userAgent);
			if (result.Success)
			{
				_nhibConfigDecryption.DecryptConfig(result.DataSourceConfiguration);
				_applicationData().MakeSureDataSourceExists(result.Tenant, result.DataSourceConfiguration.ApplicationNHibernateConfig, result.DataSourceConfiguration.AnalyticsConnectionString);
				if (_verifyTerminalDate.IsTerminated(result.Tenant, result.PersonId))
				{
					return new AuthenticationQueryResult {Success = false, FailReason = Resources.LogOnFailedInvalidUserNameOrPassword};
				}
			}
			return result;
		}
	}
}