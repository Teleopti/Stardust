using System;

namespace Teleopti.Wfm.Adherence.Domain.Service
{
	public class InvalidUserCodeException : Exception
	{
		public InvalidUserCodeException(string message) : base(message)
		{
		}
	}
}