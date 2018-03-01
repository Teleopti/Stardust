using System;

namespace Teleopti.Ccc.Domain.Rta.Service
{
	public class InvalidUserCodeException : Exception
	{
		public InvalidUserCodeException(string message) : base(message)
		{
		}
	}
}