using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.Common
{
	public class AgentBadge:AggregateRoot
	{
		public Guid PersonId { get; set; }
		public int BronzeBadge { get; set; }
		public int SilverBadge { get; set; }
		public int GoldenBadge { get; set; }
	}
}