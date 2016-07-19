using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ApproveRequestsWithValidatorsEvent : EventWithInfrastructureContext
	{
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public IEnumerable<Guid> PersonRequestIdList { get; set; }
		public RequestValidatorsFlag Validator { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}
}
