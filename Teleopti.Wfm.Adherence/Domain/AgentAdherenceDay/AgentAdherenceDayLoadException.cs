using System;

namespace Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay
{
	public class AgentAdherenceDayLoadException : Exception
	{
		public AgentAdherenceDayLoadException(string message, Exception innerExceptoin) : base(message, innerExceptoin)
		{
		}
	}
}