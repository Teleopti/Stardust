using System;

namespace Teleopti.Wfm.Adherence.Domain.Service
{
	public class InvalidAuthenticationKeyException : Exception
	{
		public InvalidAuthenticationKeyException(string message) : base(message)
		{
		}
	}
}