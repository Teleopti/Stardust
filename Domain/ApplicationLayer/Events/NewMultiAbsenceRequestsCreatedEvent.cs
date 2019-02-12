using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class NewMultiAbsenceRequestsCreatedEvent : StardustJobInfo
	{
		public IEnumerable<Guid> PersonRequestIds { get; set; }
		public DateTime Sent { get; set; }
		public IEnumerable<Guid> Ids { get; set; }
	}
}
