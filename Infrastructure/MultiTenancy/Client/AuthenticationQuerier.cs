﻿using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class AuthenticationQuerier : IAuthenticationQuerier
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IJsonSerializer _jsonSerializer;
		private readonly IAuthenticationQuerierResultConverter _resultConverter;

		public AuthenticationQuerier(ITenantServerConfiguration tenantServerConfiguration, 
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

		private AuthenticationQuerierResult doAuthenticationCall(string path, object clientModel, string userAgent)
		{
			var json = _jsonSerializer.SerializeObject(clientModel);
			return _resultConverter.Convert(_postHttpRequest.Send<AuthenticationQueryResult>(path, json, userAgent));
		}
	}
}