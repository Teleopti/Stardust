using System;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public class LegacyAuthenticationKeyException : Exception
	{
		public LegacyAuthenticationKeyException(string message)
			:base(message)
		{
		}
	}
}