using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class AgentBadgeInfo : IAgentBadgeInfo
	{
		public Guid PersonId { get; set; }
		public int Total { get; set; }
	}
}