using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Auditing
{
	public interface IPersonAccess : IAggregateRoot
	{
		DateTime TimeStamp { get; set; }
		Guid ActionPerformedById { get; set; }
		string ActionPerformedBy { get; set; }
		Guid ActionPerformedOnId { get; set; }
		string ActionPerformedOn { get; set; }
		string Action { get; set; }
		string ActionResult { get; set; }
		string Data { get; set; }
		Guid Correlation { get; set; }
	}
}
