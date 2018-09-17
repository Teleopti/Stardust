using System;

namespace Teleopti.Wfm.Adherence.Domain.Service
{
	public class LegacyAuthenticationKeyException : Exception
	{
		public LegacyAuthenticationKeyException(string message)
			:base(message)
		{
		}
	}
}