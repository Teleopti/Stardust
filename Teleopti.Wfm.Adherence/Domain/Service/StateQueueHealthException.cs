using System;

namespace Teleopti.Wfm.Adherence.Domain.Service
{
	public class StateQueueHealthException : Exception
	{
		public StateQueueHealthException(string message) : base(message)
		{
		}
	}
}