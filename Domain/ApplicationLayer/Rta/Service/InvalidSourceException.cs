using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class InvalidSourceException : Exception
	{
		public InvalidSourceException(string message) : base(message)
		{
		}
	}
}