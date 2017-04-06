using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class InvalidAuthenticationKeyException : Exception
	{
		public InvalidAuthenticationKeyException(string message) : base(message)
		{
		}
	}

	public class NoLoggedOffStateGroupsException : Exception
	{
		public NoLoggedOffStateGroupsException(string message) : base(message)
		{
		}
	}
}