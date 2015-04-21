using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class PasswordVerifier : IPasswordVerifier
	{
		private readonly Func<IPasswordPolicy> _passwordPolicy;
		private readonly INow _now;

		public PasswordVerifier(Func<IPasswordPolicy> passwordPolicy, INow now)
		{
			_passwordPolicy = passwordPolicy;
			_now = now;
		}

		public bool Check(string userPassword, ApplicationLogonInfo applicationLogonInfo)
		{
			return applicationLogonInfo.IsValidPassword(_now, _passwordPolicy(), userPassword);
		}
	}
}