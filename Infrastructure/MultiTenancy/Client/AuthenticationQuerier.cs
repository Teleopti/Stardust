using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class AuthenticationQuerier : IAuthenticationQuerier
	{
		private readonly string _pathToTenantServer;
		private readonly INhibConfigEncryption _nhibConfigEncryption;
		private readonly IPostHttpRequest _postHttpRequest;


		public AuthenticationQuerier(string pathToTenantServer, 
																INhibConfigEncryption nhibConfigEncryption, 
																IPostHttpRequest postHttpRequest)
		{
			_pathToTenantServer = pathToTenantServer;
			_nhibConfigEncryption = nhibConfigEncryption;
			_postHttpRequest = postHttpRequest;
		}

		public AuthenticationQueryResult TryApplicationLogon(string userName, string password, string userAgent)
		{
			var data = new Dictionary<string, string>
			{
				{ "userName", userName },
				{ "password", password }
			};
			var result = _postHttpRequest.Send<AuthenticationQueryResult>(_pathToTenantServer + "Authenticate/ApplicationLogon", userAgent, data);

			_nhibConfigEncryption.DecryptConfig(result.DataSourceConfiguration);
			//hardcode for now
			result.PasswordPolicy =
				"<!--Default config data-->\r\n<PasswordPolicy MaxNumberOfAttempts=\"3\" InvalidAttemptWindow=\"0\" PasswordValidForDayCount=\"2147483647\" PasswordExpireWarningDayCount=\"0\" />";
			
			return result;
		}

		public AuthenticationQueryResult TryIdentityLogon(string identity, string userAgent)
		{
			var data = new Dictionary<string, string>
			{
				{ "identity", identity }
			};
			var result = _postHttpRequest.Send<AuthenticationQueryResult>(_pathToTenantServer + "Authenticate/IdentityLogon", userAgent, data);

			_nhibConfigEncryption.DecryptConfig(result.DataSourceConfiguration);
			//hardcode for now
			result.PasswordPolicy =
				"<!--Default config data-->\r\n<PasswordPolicy MaxNumberOfAttempts=\"3\" InvalidAttemptWindow=\"0\" PasswordValidForDayCount=\"2147483647\" PasswordExpireWarningDayCount=\"0\" />";
			
			return result;
		}
	}
}