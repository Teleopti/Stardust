using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public interface IStaffingAudit : IAggregateRoot
	{
		DateTime TimeStamp { get; set; }
		Guid ActionPerformedById { get; set; }
		string ActionPerformedBy { get; set; }
		string Action { get; set; }
		string Area { get; set; }
		string ImportFileName { get; set; }
		Guid? BpoId { get; set; }
		DateTime? ClearPeriodStart { get; set; }
		DateTime? ClearPeriodEnd { get; set; }
	}
}