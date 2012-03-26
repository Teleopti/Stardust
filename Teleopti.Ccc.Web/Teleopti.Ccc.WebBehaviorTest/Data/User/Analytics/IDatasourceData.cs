using System.Collections.Generic;
using System.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public interface IDatasourceData
	{
		int RaptorDefaultDatasourceId { get; }
		IEnumerable<DataRow> Rows { get; }
	}
}