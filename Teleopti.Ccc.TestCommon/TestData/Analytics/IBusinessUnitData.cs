using System.Collections.Generic;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public interface IBusinessUnitData
	{
		IEnumerable<DataRow> Rows { get; }
		int BusinessUnitId { get; }
	}
}