using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class CheckPasswordStrength : ICheckPasswordStrength
	{
		private readonly Func<IPasswordPolicy> _passwordPolicy;

		public CheckPasswordStrength(Func<IPasswordPolicy> passwordPolicy)
		{
			_passwordPolicy = passwordPolicy;
		}

		public void Validate(string newPassword)
		{
			if(!_passwordPolicy().CheckPasswordStrength(newPassword))
				throw new PasswordStrengthException();
		}
	}
}