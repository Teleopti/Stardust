using System;

namespace Teleopti.Interfaces.Messages
{
	public interface IRaptorDomainMessageInfo
	{
		DateTime Timestamp { get; set; }
		Guid InitiatorId { get; set; }
		string Datasource { get; set; }
		Guid BusinessUnitId { get; set; }
	}
}