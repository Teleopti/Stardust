using System;

namespace Teleopti.Interfaces.Messages
{
	public interface ILogOnInfo
	{
		Guid InitiatorId { get; set; }
		string Datasource { get; set; }
		Guid BusinessUnitId { get; set; }
	}
}