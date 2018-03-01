using System;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public class InvalidUserCodeException : Exception
	{
		public InvalidUserCodeException(string message) : base(message)
		{
		}
	}
}