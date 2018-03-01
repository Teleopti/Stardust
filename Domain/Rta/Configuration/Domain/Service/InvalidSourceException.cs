using System;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public class InvalidSourceException : Exception
	{
		public InvalidSourceException(string message) : base(message)
		{
		}
	}
}