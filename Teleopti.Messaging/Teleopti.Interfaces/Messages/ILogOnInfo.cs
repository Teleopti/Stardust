using System;

namespace Teleopti.Interfaces.Messages
{
	public interface ILogOnInfo
	{
		string Datasource { get; set; }
		Guid BusinessUnitId { get; set; }
	}
}