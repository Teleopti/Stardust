using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class InvalidUserCodeException : Exception
	{
		public InvalidUserCodeException(string message) : base(message)
		{
		}
	}
}