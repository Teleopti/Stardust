using System;

namespace Teleopti.Wfm.Adherence.States
{
	public class InvalidStateCodeException : Exception
	{
		public InvalidStateCodeException(string message) : base(message)
		{
		}
	}
}