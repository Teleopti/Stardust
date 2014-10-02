using System.Collections.Generic;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence
{
	public class AgentsAdherenceMessage
	{	public IEnumerable<AgentAdherenceStateInfo> AgentStates { get; set; }
	}
}
