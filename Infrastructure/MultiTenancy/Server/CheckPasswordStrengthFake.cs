using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class CheckPasswordStrengthFake : ICheckPasswordStrength
	{
		private bool willThrow;

		public void Validate(string newPassword)
		{
			if (willThrow)
				throw new PasswordStrengthException();
		}

		public void WillThrow()
		{
			willThrow = true;
		}
	}
}