using System;

namespace Teleopti.Ccc.Domain.Rta.Service
{
	public class InvalidStateCodeException : Exception
	{
		public InvalidStateCodeException(string message) : base(message)
		{
		}
	}
}