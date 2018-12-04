using System;

namespace Teleopti.Wfm.Adherence.States
{
	public class InvalidUserCodeException : Exception
	{
		public InvalidUserCodeException(string message) : base(message)
		{
		}
	}
}