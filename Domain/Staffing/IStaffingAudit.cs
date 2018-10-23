using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public interface IStaffingAudit : IAggregateRoot
	{
		DateTime TimeStamp { get; set; }
		IPerson ActionPerformedBy { get; set; }
		string Action { get; set; }
		string Data { get; set; }
		string Area { get; set; }
	}
}