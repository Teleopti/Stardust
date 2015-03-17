﻿using System.IO;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class AuthenticationFromFileQuerier : IAuthenticationQuerier
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;

		public AuthenticationFromFileQuerier(ITenantServerConfiguration tenantServerConfiguration)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
		}

		public AuthenticationQueryResult TryApplicationLogon(ApplicationLogonClientModel applicationLogonClientModel, string userAgent)
		{
			return readFile();
		}

		public AuthenticationQueryResult TryIdentityLogon(IdentityLogonClientModel identityLogonClientModel, string userAgent)
		{
			return readFile();
		}

		private AuthenticationQueryResult readFile()
		{
			if (!File.Exists(_tenantServerConfiguration.Path))
				return new AuthenticationQueryResult { FailReason = string.Format("No file with name {0}", _tenantServerConfiguration.Path), Success = false };

			var json = File.ReadAllText(_tenantServerConfiguration.Path);
			var ret = JsonConvert.DeserializeObject<AuthenticationQueryResult>(json);
			ret.PasswordPolicy =
				"<!--Default config data-->\r\n<PasswordPolicy MaxNumberOfAttempts=\"3\" InvalidAttemptWindow=\"0\" PasswordValidForDayCount=\"2147483647\" PasswordExpireWarningDayCount=\"0\" />";
			return ret;
		}
	}
}