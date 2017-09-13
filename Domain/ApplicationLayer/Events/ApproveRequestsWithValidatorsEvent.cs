using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ApproveRequestsWithValidatorsEvent : StardustJobInfo
	{
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public IEnumerable<Guid> PersonRequestIdList { get; set; }
		public RequestValidatorsFlag Validator { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}
}