using System;

namespace Teleopti.Wfm.Adherence.States
{
	public class InvalidSourceException : Exception
	{
		public InvalidSourceException(string message) : base(message)
		{
		}
	}
}