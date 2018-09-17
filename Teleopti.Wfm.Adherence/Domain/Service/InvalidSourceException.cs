using System;

namespace Teleopti.Wfm.Adherence.Domain.Service
{
	public class InvalidSourceException : Exception
	{
		public InvalidSourceException(string message) : base(message)
		{
		}
	}
}