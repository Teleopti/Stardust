using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class InvalidPlatformException : Exception
	{
		public InvalidPlatformException(string message) : base(message)
		{
		}
	}
}