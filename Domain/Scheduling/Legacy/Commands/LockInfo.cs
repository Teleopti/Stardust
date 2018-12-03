using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class LockInfo
	{
		public Guid AgentId { get; set; }
		public DateOnly Date { get; set; }
		public LockType LockType { get; set; }
	}
}