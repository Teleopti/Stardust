using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class LegacyAuthenticationKeyException : Exception
	{
		public LegacyAuthenticationKeyException(string message)
			:base(message)
		{
		}
	}
}