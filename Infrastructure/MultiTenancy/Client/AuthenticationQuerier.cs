using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class AuthenticationQuerier : IAuthenticationQuerier
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;
		private readonly INhibConfigDecryption _nhibConfigDecryption;
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IJsonSerializer _jsonSerializer;


		public AuthenticationQuerier(ITenantServerConfiguration tenantServerConfiguration, 
																INhibConfigDecryption nhibConfigDecryption, 
																IPostHttpRequest postHttpRequest,
																IJsonSerializer jsonSerializer)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
			_nhibConfigDecryption = nhibConfigDecryption;
			_postHttpRequest = postHttpRequest;
			_jsonSerializer = jsonSerializer;
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
			_nhibConfigDecryption.DecryptConfig(result.DataSourceConfiguration);
			return result;
		}
	}
}