using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Messages
{
	public interface ILogOnContext
	{
		string LogOnDatasource { get; set; }
		Guid LogOnBusinessUnitId { get; set; }
	}
}