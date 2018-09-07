using System;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public class InvalidStateCodeException : Exception
	{
		public InvalidStateCodeException(string message) : base(message)
		{
		}
	}
}