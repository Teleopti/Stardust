using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class CheckPasswordStrength : ICheckPasswordStrength
	{
		private readonly Func<IPasswordPolicy> _passwordPolicy;
		private const string exMessage = "'{0}' breaks password strength rule.";

		public CheckPasswordStrength(Func<IPasswordPolicy> passwordPolicy)
		{
			_passwordPolicy = passwordPolicy;
		}

		public void Validate(string newPassword)
		{
			if(!_passwordPolicy().CheckPasswordStrength(newPassword))
				throw new PasswordStrengthException(string.Format(exMessage, newPassword));
		}
	}
}