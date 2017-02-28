using System;

namespace Teleopti.Interfaces.Messages
{
	public interface ILogOnContext
	{
		string LogOnDatasource { get; set; }
		Guid LogOnBusinessUnitId { get; set; }
	}
}