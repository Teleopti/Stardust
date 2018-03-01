using System;

namespace Teleopti.Ccc.Domain.Rta.Service
{
	public class LegacyAuthenticationKeyException : Exception
	{
		public LegacyAuthenticationKeyException(string message)
			:base(message)
		{
		}
	}
}