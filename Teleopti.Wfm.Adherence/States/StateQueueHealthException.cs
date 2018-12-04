using System;

namespace Teleopti.Wfm.Adherence.States
{
	public class StateQueueHealthException : Exception
	{
		public StateQueueHealthException(string message) : base(message)
		{
		}
	}
}