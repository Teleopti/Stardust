using System;

namespace Teleopti.Interfaces.Messages
{
	public interface IRaptorDomainMessageInfo
	{
		Guid InitiatorId { get; set; }
		string Datasource { get; set; }
		Guid BusinessUnitId { get; set; }
	}
}