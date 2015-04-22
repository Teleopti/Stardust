using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class CheckPasswordStrengthFailing : ICheckPasswordStrength
	{
		public void Validate(string newPassword)
		{
			throw new NotImplementedException("fail!");
		}
	}
}