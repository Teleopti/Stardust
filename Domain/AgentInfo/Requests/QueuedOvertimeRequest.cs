using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class QueuedOvertimeRequest : NonversionedAggregateRootWithBusinessUnit, IQueuedOvertimeRequest
	{
		public virtual Guid PersonRequest { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual DateTime StartDateTime { get; set; }
		public virtual DateTime EndDateTime { get; set; }
		public virtual DateTime? Sent { get; set; }
	}
}
