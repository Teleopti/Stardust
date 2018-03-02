using System;

namespace Teleopti.Ccc.Domain.Auditing
{
	public interface IPersonAccess
	{
		DateTime TimeStamp { get; set; }
		Guid ActionPerformedBy { get; set; }
		Guid ActionPerformeOn { get; set; }
		string Action { get; set; }
		string Data { get; set; }
		Guid? Correlation { get; set; }
	}
}
