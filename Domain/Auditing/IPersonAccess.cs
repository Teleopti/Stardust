using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Auditing
{
	public interface IPersonAccess : IAggregateRoot
	{
		DateTime TimeStamp { get; set; }
		IPerson ActionPerformedBy { get; set; }
		IPerson ActionPerformedOn { get; set; }
		string Action { get; set; }
		string ActionResult { get; set; }
		string Data { get; set; }
		Guid Correlation { get; set; }
	}
}
