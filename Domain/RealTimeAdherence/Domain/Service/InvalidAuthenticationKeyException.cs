using System;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public class InvalidAuthenticationKeyException : Exception
	{
		public InvalidAuthenticationKeyException(string message) : base(message)
		{
		}
	}
}