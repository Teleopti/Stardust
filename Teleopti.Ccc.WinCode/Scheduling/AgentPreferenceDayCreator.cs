using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface  IAgentPreferenceDayCreator
	{
		IAgentPreferenceCanCreateResult CanCreate(TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd, TimeSpan? minLength, TimeSpan? maxLength, bool endNextDay)
	}

	public class AgentPreferenceDayCreator : IAgentPreferenceDayCreator
	{
		public IAgentPreferenceCanCreateResult CanCreate(TimeSpan? minStart, TimeSpan? maxStart, TimeSpan? minEnd, TimeSpan? maxEnd, TimeSpan? minLength, TimeSpan? maxLength, bool endNextDay)
		{
			throw new NotImplementedException();
		}
	}
}
