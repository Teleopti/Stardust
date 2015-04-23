using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class CheckPasswordStrengthFake : ICheckPasswordStrength
	{
		private Exception _exceptionToThrow;

		public void Validate(string newPassword)
		{
			if (_exceptionToThrow != null)
				throw _exceptionToThrow;
		}

		public void WillThrow(Exception exceptionToThrow)
		{
			_exceptionToThrow = exceptionToThrow;
		}
	}
}