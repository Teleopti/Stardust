using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAgentBadgeInfo
	{
		Guid PersonId { get; set; }
		int Total { get; set; }
	}
}