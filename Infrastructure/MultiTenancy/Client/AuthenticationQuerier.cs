using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class AuthenticationQuerier : IAuthenticationQuerier
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;
		private readonly INhibConfigEncryption _nhibConfigEncryption;
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IJsonSerializer _jsonSerializer;


		public AuthenticationQuerier(ITenantServerConfiguration tenantServerConfiguration, 
																INhibConfigEncryption nhibConfigEncryption, 
																IPostHttpRequest postHttpRequest,
																IJsonSerializer jsonSerializer)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
			_nhibConfigEncryption = nhibConfigEncryption;
			_postHttpRequest = postHttpRequest;
			_jsonSerializer = jsonSerializer;
		}

		public AuthenticationQueryResult TryApplicationLogon(ApplicationLogonClientModel applicationLogonClientModel, string userAgent)
		{
			var json = _jsonSerializer.SerializeObject(applicationLogonClientModel);
			var result = _postHttpRequest.Send<AuthenticationQueryResult>(_tenantServerConfiguration.Path + "Authenticate/ApplicationLogon", json, userAgent);

			_nhibConfigEncryption.DecryptConfig(result.DataSourceConfiguration);
			
			return result;
		}

		public AuthenticationQueryResult TryIdentityLogon(IdentityLogonClientModel identityLogonClientModel, string userAgent)
		{
			var json = _jsonSerializer.SerializeObject(identityLogonClientModel);
			var result = _postHttpRequest.Send<AuthenticationQueryResult>(_tenantServerConfiguration.Path + "Authenticate/IdentityLogon", json, userAgent);

			_nhibConfigEncryption.DecryptConfig(result.DataSourceConfiguration);
			
			return result;
		}
	}
}