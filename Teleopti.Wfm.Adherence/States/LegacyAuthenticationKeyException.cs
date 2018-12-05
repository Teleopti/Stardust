using System;

namespace Teleopti.Wfm.Adherence.States
{
	public class LegacyAuthenticationKeyException : Exception
	{
		public LegacyAuthenticationKeyException(string message)
			:base(message)
		{
		}
	}
}