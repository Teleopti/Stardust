using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public class AgentDayDateSetter : IAgentDayDateSetter
	{
		private readonly IEnumerable<IPersonAssignmentConverter> _agentDayDatePartSetters;

		public AgentDayDateSetter(IEnumerable<IPersonAssignmentConverter> agentDayDatePartSetters)
		{
			_agentDayDatePartSetters = agentDayDatePartSetters;
		}
	}
}