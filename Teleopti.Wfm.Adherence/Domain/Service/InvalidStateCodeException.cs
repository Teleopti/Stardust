using System;

namespace Teleopti.Wfm.Adherence.Domain.Service
{
	public class InvalidStateCodeException : Exception
	{
		public InvalidStateCodeException(string message) : base(message)
		{
		}
	}
}