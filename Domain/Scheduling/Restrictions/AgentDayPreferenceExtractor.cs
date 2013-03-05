using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{

	/// <summary>
	/// Extracts and creates AgentDayPreference class
	/// </summary>
	public interface IAgentDayPreferenceExtractor
	{
		AgentDayPreference Extract();
	}

	public class AgentDayPreferenceExtractor : IAgentDayPreferenceExtractor
	{
		public AgentDayPreference Extract()
		{
			throw new NotImplementedException();
		}
	}
}
