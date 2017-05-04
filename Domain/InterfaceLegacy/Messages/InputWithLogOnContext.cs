using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Messages
{

	public class InputWithLogOnContext : ILogOnContext
	{
		public InputWithLogOnContext(string dataSource, Guid businessUnitId)
		{
			LogOnDatasource = dataSource;
			LogOnBusinessUnitId = businessUnitId;
		}

		public string LogOnDatasource { get; set; }
		public Guid LogOnBusinessUnitId { get; set; }
	}
}
