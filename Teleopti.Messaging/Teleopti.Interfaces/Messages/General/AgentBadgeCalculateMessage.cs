
using System;

namespace Teleopti.Interfaces.Messages.General
{
	public class AgentBadgeCalculateMessage : RaptorDomainMessage
	{
		public override Guid Identity
		{
			get { return Guid.NewGuid(); }
		}

		public bool IsInitialization { get; set; }
		public int TimezoneId { get; set; }
	}
}