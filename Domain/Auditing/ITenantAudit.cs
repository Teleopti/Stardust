using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Auditing
{
	public interface ITenantAudit : IAggregateRoot
	{
		DateTime TimeStamp { get; set; }
		Guid ActionPerformedBy { get; set; }
		Guid ActionPerformedOn { get; set; }
		string Action { get; set; }
		string ActionResult { get; set; }
		string Data { get; set; }
		Guid Correlation { get; set; }
	}
}