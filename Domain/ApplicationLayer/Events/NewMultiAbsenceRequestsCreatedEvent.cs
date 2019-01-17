using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class NewMultiAbsenceRequestsCreatedEvent : StardustJobInfo
	{
		public List<Guid> PersonRequestIds { get; set; }
		public DateTime Sent { get; set; }
		public List<Guid> Ids { get; set; }
	}
}
