using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
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

		//rename
		public AuthenticationQueryResult TryLogon(string userName, string password, string userAgent)
		{
			var data = new Dictionary<string, string>
			{
				{ "userName", userName },
				{ "password", password }
			};
			var result = _postHttpRequest.Send<AuthenticationQueryResult>(_pathToTenantServer + "Authenticate/ApplicationLogon", userAgent, data);

			_nhibConfigEncryption.DecryptConfig(result.DataSourceConfiguration);
			return result;
		}

		public AuthenticationQueryResult TryIdentityLogon(string identity, string userAgent)
		{
			var data = new Dictionary<string, string>
			{
				{ "identity", identity }
			};
			var result = _postHttpRequest.Send<AuthenticationQueryResult>(_pathToTenantServer + "Authenticate/IdentityLogon", userAgent, data);

			result.DataSourceConfiguration = _nhibConfigEncryption.DecryptConfig(result.DataSourceConfiguration);
			return result;
		}
	}
}