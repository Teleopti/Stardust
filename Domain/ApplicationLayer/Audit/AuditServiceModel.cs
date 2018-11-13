using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Audit
{
	public class AuditServiceModel
	{
		public DateTime TimeStamp { get; set; }
		public string ActionPerformedBy{ get; set; }
		public string Action { get; set; }
		public string Context { get; set; }
		public string Data { get; set; }
	}
}