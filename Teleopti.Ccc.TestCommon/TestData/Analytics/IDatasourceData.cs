using System.Collections.Generic;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public interface IDatasourceData
	{
		int RaptorDefaultDatasourceId { get; }
		IEnumerable<DataRow> Rows { get; }
	}
}