using System;

namespace Teleopti.Wfm.Adherence.Domain.Service
{
	public class FloodedStateQueueException : Exception
	{
		public FloodedStateQueueException(string message) : base(message)
		{
		}
	}
}