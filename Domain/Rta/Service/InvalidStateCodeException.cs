using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class InvalidStateCodeException : Exception
	{
		public InvalidStateCodeException(string message) : base(message)
		{
		}
	}
}