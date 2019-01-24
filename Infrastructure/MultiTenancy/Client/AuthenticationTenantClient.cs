using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class AuthenticationTenantClient : IAuthenticationTenantClient
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IJsonSerializer _jsonSerializer;
		private readonly IAuthenticationQuerierResultConverter _resultConverter;

		public AuthenticationTenantClient(ITenantServerConfiguration tenantServerConfiguration, 
																IPostHttpRequest postHttpRequest,
																IJsonSerializer jsonSerializer,
																IAuthenticationQuerierResultConverter resultConverter)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
			_postHttpRequest = postHttpRequest;
			_jsonSerializer = jsonSerializer;
			_resultConverter = resultConverter;
		}

		public AuthenticationQuerierResult TryLogon(ApplicationLogonClientModel applicationLogonClientModel, string userAgent)
		{
			return doAuthenticationCall(_tenantServerConfiguration.FullPath("Authenticate/ApplicationLogon"), applicationLogonClientModel, userAgent);
		}

		public AuthenticationQuerierResult TryLogon(IdentityLogonClientModel identityLogonClientModel, string userAgent)
		{
			return doAuthenticationCall(_tenantServerConfiguration.FullPath("Authenticate/IdentityLogon"), identityLogonClientModel, userAgent);
		}

		public AuthenticationQuerierResult TryLogon(IdLogonClientModel idLogonClientModel, string userAgent)
		{
			return doAuthenticationCall(_tenantServerConfiguration.FullPath("Authenticate/IdLogon"), idLogonClientModel, userAgent);
		}

		private AuthenticationQuerierResult doAuthenticationCall(string path, object clientModel, string userAgent)
		{
			var json = _jsonSerializer.SerializeObject(clientModel);
			return _resultConverter.Convert(_postHttpRequest.Send<AuthenticationInternalQuerierResult>(path, json, userAgent));
		}
	}
}