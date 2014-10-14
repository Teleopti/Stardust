using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadgeInfo
	{
		Guid PersonId { get; set; }
		int Total { get; set; }
	}
}