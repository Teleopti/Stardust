using System;

namespace Teleopti.Ccc.Domain.Rta.Service
{
	public class InvalidAuthenticationKeyException : Exception
	{
		public InvalidAuthenticationKeyException(string message) : base(message)
		{
		}
	}
}