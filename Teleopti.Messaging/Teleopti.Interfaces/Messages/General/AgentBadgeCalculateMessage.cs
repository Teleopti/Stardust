
using System;

namespace Teleopti.Interfaces.Messages.General
{
	public class AgentBadgeCalculateMessage : RaptorDomainMessage
	{
		public override Guid Identity
		{
			get { return Guid.NewGuid(); }
		}
	}
}