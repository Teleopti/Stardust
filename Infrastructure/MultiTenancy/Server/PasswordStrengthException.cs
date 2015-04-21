using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class PasswordStrengthException : Exception
	{
		public PasswordStrengthException(string exceptionMessage) : base(exceptionMessage)
		{
		}
	}
}
