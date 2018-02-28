using System;

namespace Teleopti.Ccc.Domain.Rta.Service
{
	public class InvalidSourceException : Exception
	{
		public InvalidSourceException(string message) : base(message)
		{
		}
	}
}