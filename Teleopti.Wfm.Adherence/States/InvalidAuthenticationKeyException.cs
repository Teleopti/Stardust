using System;

namespace Teleopti.Wfm.Adherence.States
{
	public class InvalidAuthenticationKeyException : Exception
	{
		public InvalidAuthenticationKeyException(string message) : base(message)
		{
		}
	}
}