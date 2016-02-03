using System;

namespace Teleopti.Interfaces.Messages
{
	public interface ILogOnInfo
	{
		string LogOnDatasource { get; set; }
		Guid LogOnBusinessUnitId { get; set; }
	}
}